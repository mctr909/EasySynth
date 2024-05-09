using System;
using System.Collections.Generic;
using SMF;

namespace EasySynth {
	class EventList {
		public static List<Event> List = new List<Event>();

		public static bool Updated { get; private set; } = false;

		public static int LoadSmfEvent(string filePath) {
			List.Clear();
			var events = new SMF.SMF(filePath).EventList;
			var lastTick = 0;
			foreach (var ev in events) {
				lastTick = Math.Max(lastTick, ev.Tick);
				if (ev is Ctrl ctrl) {
					List.Add(new CustomCtrl(ctrl.Data) {
						Tick = ctrl.Tick,
						Track = ctrl.Track
					});
				} else {
					List.Add(ev);
				}
			}
			return lastTick;
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
				if (ev is Note n && end > n.Tick && (n.Start >= start || n.End >= start)) {
					end = n.Tick;
				}
			}
			if (start >= end) {
				return;
			}
			var note = new Note(start, end, tone, velocity) {
				Track = track,
				Channel = (byte)channel
			};
			List.Add(note);
			List.Add(note.Pair);
			Updated = true;
		}

		public static void AddCtrl(int track, int channel, int tick, CTRL type, double value) {
			if (tick < 0) {
				return;
			}
			RemoveCtrls(tick, tick, type, track);
			List.Add(new CustomCtrl(type) {
				Tick = tick,
				Track = track,
				Channel = (byte)channel,
				Value = value
			});
			Updated = true;
		}

		public static void AddMeasure(int tick, int denominator, int numerator) {
			if (tick < 0) {
				return;
			}
			var last = SelectLastMeasure(tick);
			var elapse = tick - last.Tick;
			if ((elapse % last.Unit) > 0) {
				return;
			}
			if (elapse <= 0) {
				RemoveMetas(last.Tick, tick, META.MEASURE);
				RemoveMetas(last.Tick, tick, META.KEY);
			}
			List.Add(new Measure(numerator, denominator) {
				Tick = tick
			});
		}

		public static void AddKey(int tick, KEY_SIG key) {
			if (tick < 0) {
				return;
			}
			var last = SelectLastMeasure(tick);
			var elapse = tick - last.Tick;
			if ((elapse % last.Unit) > 0) {
				return;
			}
			RemoveMetas(tick, tick, META.KEY);
			List.Add(new Key(key) {
				Tick = tick
			});
		}

		public static void AddTempo(int tick, double tempo) {
			if (tick < 0) {
				return;
			}
			RemoveMetas(tick, tick, META.TEMPO);
			List.Add(new Tempo(tempo) {
				Tick = tick
			});
			Updated = true;
		}

		public static void AddRange(List<Event> source, int offsetTick = 0, int offsetTone = 0) {
			foreach (var ev in source) {
				if (ev is Note note) {
					AddNote(ev.Track, ev.Channel, ev.Tick + offsetTick, note.Pair.Tick + offsetTick, note.Tone + offsetTone, note.Velocity);
				}
				if (ev is CustomCtrl ctrl) {
					AddCtrl(ev.Track, ev.Channel, ev.Tick + offsetTick, ctrl.CtrlType, ctrl.Value);
				}
			}
		}

		public static void RemoveNotes(int start, int end, int minTone, int maxTone, params int[] tracks) {
			List.RemoveAll(v => {
				if (v is Note note && note.Contains(start, end, minTone, maxTone, tracks)) {
					note.Pair.Pair = null;
					return true;
				}
				return false;
			});
			List.RemoveAll(v => v is Note note && note.MsgType == MSG.NOTE_OFF && note.Pair == null);
			Updated = true;
		}

		public static void RemoveCtrls(int start, int end, CTRL type, params int[] tracks) {
			List.RemoveAll(v => v is CustomCtrl ctrl && ctrl.Contains(type, start, end, tracks));
			Updated = true;
		}

		public static void RemoveMetas(int start, int end, META type) {
			List.RemoveAll(v => v is Meta meta && meta.Contains(type, start, end));
			Updated = type == META.TEMPO;
		}

		public static void ClearSelected() {
			List.ForEach(v => { v.Selected = false; });
		}

		public static List<Event> SelectNotes(int start, int end, int minTone, int maxTone, bool setFlag, params int[] tracks) {
			return List.FindAll(v => {
				if (v is Note note && note.Contains(start, end, minTone, maxTone, tracks)) {
					if (setFlag) {
						v.Selected = !v.Selected;
					}
					return true;
				}
				return false;
			});
		}

		public static List<Event> SelectCtrls(int start, int end, CTRL type, bool setFlag, params int[] tracks) {
			return List.FindAll(v => {
				if (v is CustomCtrl ctrl && ctrl.Contains(type, start, end, tracks)) {
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
				if (v is Meta meta && meta.Contains(type, start, end)) {
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

		public static Measure SelectNextMeasure(int tick) {
			return (Measure)List.Find(v => v is Measure && v.Tick > tick);
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
