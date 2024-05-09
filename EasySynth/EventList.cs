using System;
using System.Collections.Generic;
using Synth;
using static Synth.SMF;

namespace EasySynth {
	class EventList {
		public static List<Event> List = new List<Event>();

		public static bool Updated { get; private set; } = false;

		public static int LoadSmfEvent(string filePath) {
			List.Clear();
			var noteOnList = new List<SMF.Note>();
			var events = new SMF(filePath).EventList;
			var lastTick = 0;
			foreach (var ev in events) {
				lastTick = Math.Max(lastTick, ev.Tick);
				if (ev is SMF.Note note) {
					if (note.MsgType == MSG.NOTE_ON) {
						if (0 == note.Velocity) {
							AddNote(note);
						} else {
							noteOnList.Add(note);
						}
					} else {
						AddNote(note);
					}
				} else if (ev is SMF.Ctrl || ev is Prog || ev is Pitch) {
					List.Add(new Synth.Ctrl(ev));
				} else {
					List.Add(ev);
				}
			}
			return lastTick;
			void AddNote(SMF.Note noteOff) {
				var noteOn = noteOnList.Find(x => x.Track == noteOff.Track && x.Channel == noteOff.Channel && x.Tone == noteOff.Tone);
				if (null == noteOn) {
					return;
				}
				var note = new Synth.Note(noteOn.Tick, noteOff.Tick, noteOn.Tone, noteOn.Velocity) {
					Track = noteOn.Track,
					Channel = noteOn.Channel
				};
				List.Add(note);
				List.Add(note.Pair);
				noteOnList.Remove(noteOn);
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
			foreach (var ev in notes) {
				if (ev is Synth.Note n && end > n.Tick && (n.Tick >= start || n.End >= start)) {
					end = n.Tick;
				}
			}
			if (start >= end) {
				return;
			}
			var note = new Synth.Note(start, end, tone, velocity) {
				Track = track,
				Channel = channel
			};
			List.Add(note);
			List.Add(note.Pair);
			Updated = true;
		}

		public static void AddCtrl(int track, int channel, int tick, Synth.Ctrl.TYPE type, double value) {
			if (tick < 0) {
				return;
			}
			RemoveCtrls(tick, tick, type, track);
			List.Add(new Synth.Ctrl(type, value) {
				Tick = tick,
				Track = track,
				Channel = channel
			});
			Updated = true;
		}

		public static void AddMeasure(int tick, int denominator, int numerator) {
			if (tick < 0) {
				return;
			}
			var last = SelectLastMeasure(tick);
			var elapse = tick - last.Tick;
			if ((elapse % last.UnitTick) > 0) {
				return;
			}
			if (elapse <= 0) {
				RemoveMetas(last.Tick, tick, META.MEASURE);
				RemoveMetas(last.Tick, tick, META.KEY);
			}
			List.Add(new Measure(numerator, denominator) {
				Tick = tick,
				Track = -3
			});
		}

		public static void AddKey(int tick, KEY_SIG key) {
			if (tick < 0) {
				return;
			}
			var last = SelectLastMeasure(tick);
			var elapse = tick - last.Tick;
			if ((elapse % last.UnitTick) > 0) {
				return;
			}
			RemoveMetas(tick, tick, META.KEY);
			List.Add(new Key(key) {
				Tick = tick,
				Track = -2
			});
		}

		public static void AddTempo(int tick, double tempo) {
			if (tick < 0) {
				return;
			}
			RemoveMetas(tick, tick, META.TEMPO);
			List.Add(new Tempo(tempo) {
				Tick = tick,
				Track = -1
			});
			Updated = true;
		}

		public static void AddRange(List<Event> source, int offsetTick = 0, int offsetTone = 0) {
			foreach (var ev in source) {
				if (ev is Synth.Note note) {
					AddNote(ev.Track, ev.Channel, ev.Tick + offsetTick, note.Pair.Tick + offsetTick, note.Tone + offsetTone, note.Velocity);
				}
				if (ev is Synth.Ctrl ctrl) {
					AddCtrl(ev.Track, ev.Channel, ev.Tick + offsetTick, (Synth.Ctrl.TYPE)ctrl.CtrlType, ctrl.CtrlValue);
				}
			}
		}

		public static void RemoveNotes(int start, int end, int minTone, int maxTone, params int[] tracks) {
			List.RemoveAll(v => {
				if (v is Synth.Note note && note.Contains(start, end, minTone, maxTone, tracks)) {
					note.Pair.Pair = null;
					return true;
				}
				return false;
			});
			List.RemoveAll(v => v is Synth.Note note && note.MsgType == MSG.NOTE_OFF && note.Pair == null);
			Updated = true;
		}

		public static void RemoveCtrls(int start, int end, Synth.Ctrl.TYPE type, params int[] tracks) {
			List.RemoveAll(v => v is Synth.Ctrl ctrl && ctrl.Contains(start, end, type, tracks));
			Updated = true;
		}

		public static void RemoveMetas(int start, int end, META type) {
			List.RemoveAll(v => v.MsgType == MSG.META && v.MetaType == type && v.Tick >= start && v.Tick <= end);
			Updated = type == META.TEMPO;
		}

		public static void ClearSelected() {
			List.ForEach(v => { v.Selected = false; });
		}

		public static List<Event> SelectNotes(int start, int end, int minTone, int maxTone, bool setFlag, params int[] tracks) {
			return List.FindAll(v => {
				if (v is Synth.Note note && note.Contains(start, end, minTone, maxTone, tracks)) {
					if (setFlag) {
						v.Selected = !v.Selected;
					}
					return true;
				}
				return false;
			});
		}

		public static List<Event> SelectCtrls(int start, int end, Synth.Ctrl.TYPE type, bool setFlag, params int[] tracks) {
			return List.FindAll(v => {
				if (v is Synth.Ctrl ctrl && ctrl.Contains(start, end, type, tracks)) {
					if (setFlag) {
						v.Selected = !v.Selected;
					}
					return true;
				}
				return false;
			});
		}

		public static List<Event> SelectMetas(int start, int end, META type, bool setFlag) {
			return List.FindAll(v => {
				if (v.MsgType == MSG.META && v.MetaType == type && v.Tick >= start && v.Tick <= end) {
					if (setFlag) {
						v.Selected = !v.Selected;
					}
					return true;
				}
				return false;
			});
		}

		public static Measure SelectLastMeasure(int tick) {
			var ev = List.FindLast(v => v is Measure && v.Tick <= tick);
			if (null == ev) {
				return new Measure(4, 4);
			} else {
				return (Measure)ev;
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
