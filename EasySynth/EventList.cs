using System;
using System.Collections.Generic;
using Synth;

namespace EasySynth {
	class EventList {
		struct NoteOn {
			public int Tick;
			public int Track;
			public int Channel;
			public int Tone;
			public int Velocity;
		}

		public static List<Event> List = new List<Event>();

		public static bool Updated { get; private set; } = false;

		public static int LoadSmfEvent(string filePath) {
			List.Clear();
			var noteOnList = new List<NoteOn>();
			var events = new SMF(filePath).EventList;
			var lastTick = 0;
			foreach (var ev in events) {
				lastTick = Math.Max(lastTick, ev.Tick);
				switch (ev.Type) {
				case SMF.MESSAGE.NOTE_OFF:
					AddNote(ev);
					break;
				case SMF.MESSAGE.NOTE_ON:
					if (0 == ev.Data[2]) {
						AddNote(ev);
					} else {
						noteOnList.Add(new NoteOn() {
							Tick = ev.Tick,
							Track = ev.Track,
							Channel = ev.Channel,
							Tone = ev.Data[1],
							Velocity = ev.Data[2]
						});
					}
					break;
				case SMF.MESSAGE.CTRL_CHG:
					switch (ev.CtrlType) {
					case SMF.CTRL.MOD:
					case SMF.CTRL.VOL:
					case SMF.CTRL.EXP:
					case SMF.CTRL.RESONANCE:
					case SMF.CTRL.CUTOFF:
					case SMF.CTRL.REV_SEND:
					case SMF.CTRL.CHO_SEND:
					case SMF.CTRL.DEL_SEND:
						List.Add(new Event(ev.Tick, ev.Track, ev.Channel, (Event.CTRL)ev.Data[1], ev.Data[2] / 127.0));
						break;
					case SMF.CTRL.PAN:
						List.Add(new Event(ev.Tick, ev.Track, ev.Channel, (Event.CTRL)ev.Data[1], (ev.Data[2] - 64) / 64.0));
						break;
					case SMF.CTRL.HOLD:
						List.Add(new Event(ev.Tick, ev.Track, ev.Channel, (Event.CTRL)ev.Data[1], ev.Data[2] >= 64 ? 1 : 0));
						break;
					case SMF.CTRL.DATA_MSB:
						break;
					case SMF.CTRL.RPN_LSB:
						break;
					case SMF.CTRL.RPN_MSB:
						break;
					}
					break;
				case SMF.MESSAGE.PROG_CHG:
					List.Add(new Event(ev.Tick, ev.Track, ev.Channel, Event.CTRL.PROG_CHG, ev.Data[1]));
					break;
				case SMF.MESSAGE.PITCH:
					List.Add(new Event(ev.Tick, ev.Track, ev.Channel, Event.CTRL.PITCH, 1));
					break;
				case SMF.MESSAGE.META:
					switch (ev.MetaType) {
					case SMF.META.TEMPO:
						List.Add(new Event(ev.Tick, ev.Tempo));
						break;
					case SMF.META.MEASURE:
						List.Add(new Event(ev.Tick, ev.Measure.Denominator, ev.Measure.Numerator));
						break;
					case SMF.META.KEY:
						List.Add(new Event(ev.Tick, (int)ev.Key));
						break;
					}
					break;
				}
			}
			return lastTick;
			void AddNote(SMF.Message ev) {
				var on = noteOnList.Find(x => x.Track == ev.Track && x.Channel == ev.Channel && x.Tone == ev.Data[1]);
				var note = new Event(on.Tick, on.Track, on.Channel, ev.Tick, on.Tone, on.Velocity);
				List.Add(note);
				List.Add(note.NotePair);
				noteOnList.Remove(on);
			}
		}

		public static void AddNote(int track, int channel, int start, int end, int tone, int velocity) {
			if (start < 0 || tone > 127 || tone < 0) {
				return;
			}
			if (velocity < 1) {
				velocity = 1;
			}
			if (velocity > 127) {
				velocity = 127;
			}
			var notes = SelectNotes(start, end, tone, tone, false, track);
			foreach (var n in notes) {
				if (end > n.Tick && (n.Tick >= start || n.TickEnd >= start)) {
					end = n.Tick;
				}
			}
			if (start >= end) {
				return;
			}
			var note = new Event(start, track, channel, end, tone, velocity);
			List.Add(note);
			List.Add(note.NotePair);
			Updated = true;
		}

		public static void AddCtrl(int track, int channel, int tick, Event.CTRL type, double value) {
			if (tick < 0) {
				return;
			}
			RemoveCtrls(tick, tick, type, track);
			List.Add(new Event(tick, track, channel, type, value));
			Updated = true;
		}

		public static void AddMeasure(int tick, byte denominator, byte numerator) {
			if (tick < 0) {
				return;
			}
			var last = SelectLastMeasure(tick);
			var elapse = tick - last.Tick;
			if ((elapse % last.UnitTick) > 0) {
				return;
			}
			if (elapse <= 0) {
				RemoveMetas(last.Tick, tick, Event.META.MEASURE);
				RemoveMetas(last.Tick, tick, Event.META.KEY);
			}
			List.Add(new Event(tick, denominator, numerator));
		}

		public static void AddKey(int tick, SMF.KEY key) {
			if (tick < 0) {
				return;
			}
			var last = SelectLastMeasure(tick);
			var elapse = tick - last.Tick;
			if ((elapse % last.UnitTick) > 0) {
				return;
			}
			RemoveMetas(tick, tick, Event.META.KEY);
			List.Add(new Event(tick, (int)key));
		}

		public static void AddTempo(int tick, double tempo) {
			if (tick < 0) {
				return;
			}
			RemoveMetas(tick, tick, Event.META.TEMPO);
			List.Add(new Event(tick, tempo));
			Updated = true;
		}

		public static void AddRange(List<Event> events, int offsetTick = 0, int offsetTone = 0) {
			foreach(var ev in events) {
				switch (ev.Type) {
				case Event.TYPE.NOTE_ON:
					AddNote(ev.Track, ev.Channel, ev.Tick + offsetTick, ev.TickEnd + offsetTick, ev.Tone + offsetTone, (int)ev.Value);
					break;
				case Event.TYPE.CTRL_CHG:
					AddCtrl(ev.Track, ev.Channel, ev.Tick + offsetTick, ev.CtrlType, ev.Value);
					break;
				}
			}
		}

		public static void AddRange(List<Event> events, int track, int channel, int offsetTick = 0, int offsetTone = 0) {
			foreach (var ev in events) {
				switch (ev.Type) {
				case Event.TYPE.NOTE_ON:
					AddNote(track, channel, ev.Tick + offsetTick, ev.TickEnd + offsetTick, ev.Tone + offsetTone, (int)ev.Value);
					break;
				case Event.TYPE.CTRL_CHG:
					AddCtrl(track, channel, ev.Tick + offsetTick, ev.CtrlType, ev.Value);
					break;
				}
			}
		}

		public static void RemoveNotes(int start, int end, int minTone, int maxTone, params int[] tracks) {
			List.RemoveAll(v => {
				if (v.ContainsNote(start, end, minTone, maxTone, tracks)) {
					v.NotePair.NotePair = null;
					return true;
				}
				return false;
			});
			List.RemoveAll(v => v.Type == Event.TYPE.NOTE_OFF && v.NotePair == null);
			Updated = true;
		}

		public static void RemoveCtrls(int start, int end, Event.CTRL type, params int[] tracks) {
			List.RemoveAll(v => v.ContainsCtrl(start, end, type, tracks));
			Updated = true;
		}

		public static void RemoveMetas(int start, int end, Event.META type) {
			List.RemoveAll(v => v.ContainsMeta(start, end, type));
			Updated = type == Event.META.TEMPO;
		}

		public static void ClearSelected() {
			List.ForEach(v => { v.Selected = false; });
		}

		public static List<Event> SelectNotes(int start, int end, int minTone, int maxTone, bool setFlag, params int[] tracks) {
			return List.FindAll(v => {
				if (v.ContainsNote(start, end, minTone, maxTone, tracks)) {
					if (setFlag) {
						v.Selected = !v.Selected;
					}
					return true;
				}
				return false;
			});
		}

		public static List<Event> SelectCtrls(int start, int end, Event.CTRL type, bool setFlag, params int[] tracks) {
			return List.FindAll(v => {
				if (v.ContainsCtrl(start, end, type, tracks)) {
					if (setFlag) {
						v.Selected = !v.Selected;
					}
					return true;
				}
				return false;
			});
		}

		public static List<Event> SelectMetas(int start, int end, Event.META type, bool setFlag) {
			return List.FindAll(v => {
				if (v.ContainsMeta(start, end, type)) {
					if (setFlag) {
						v.Selected = !v.Selected;
					}
					return true;
				}
				return false;
			});
		}

		public static Event.MEASURE SelectLastMeasure(int tick) {
			var ev = List.FindLast(v => v.Type == Event.TYPE.META
				&& v.MetaType == Event.META.MEASURE
				&& v.Tick <= tick);
			if (null == ev) {
				return new Event(0, 4, 4).Measure;
			} else {
				return ev.Measure;
			}
		}

		public static Queue<Event> GetQueue(int start = 0) {
			Updated = false;
			List.Sort((a, b) => {
				var diffTick = a.Tick - b.Tick;
				if (0 == diffTick) {
					return a.Track - b.Track;
				}
				return diffTick;
			});
			return new Queue<Event>(List.FindAll(v => v.Tick >= start));
		}
	}
}
