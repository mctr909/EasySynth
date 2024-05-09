using System.Linq;

namespace Synth {
	public class Event {
		public enum TYPE : byte {
			NOTE_OFF = 0x80,
			NOTE_ON = 0x90,
			CTRL_CHG = 0xB0,
			META = 0xF0
		}

		public enum CTRL : byte {
			BANK_MSB = 0,
			BANK_LSB = 32,
			MOD = 1,
			POLTA_TIME = 5,
			POLTA_SWITCH = 65,
			VOL = 7,
			PAN = 10,
			EXP = 11,
			HOLD = 64,
			RESONANCE = 71,
			CUTOFF = 74,
			REV_SEND = 91,
			CHO_SEND = 93,
			DEL_SEND = 94,

			ALL_SOUND_OFF = 120,
			RESET_ALL_CTRL = 121,
			ALL_NOTE_OFF = 123,

			EG_AMP_ATTACK = 0b1010_0000,
			EG_AMP_DECAY,
			EG_AMP_RELEASE,
			EG_AMP_SUSTAIN,

			EG_FILTER_ATTACK = 0b1010_1000,
			EG_FILTER_DECAY,
			EG_FILTER_RISE,
			EG_FILTER_LEVEL,
			EG_FILTER_SUSTAIN,
			EG_FILTER_FALL,

			EG_PITCH_ATTACK = 0b1011_0000,
			EG_PITCH_DECAY,
			EG_PITCH_RISE,
			EG_PITCH_LEVEL,
			EG_PITCH_FALL,

			PROG_CHG = 0b1100_0000,

			CHO_DEPTH = 0b1100_0010,
			CHO_RATE,

			DEL_TIME = 0b1100_0100,
			DEL_FEEDBACK,
			DEL_CROSS,

			VIB_RANGE = 0b1100_1000,
			VIB_RATE,
			VIB_DELAY,

			PITCH = 0b1110_0000,

			MUTE = 0b1111_1110,

			INVALID = 255
		}

		public enum META : byte {
			MEASURE,
			KEY,
			TEMPO,
			LYRIC,
			INVALID = 255
		}

		public const int BASE_TICK = 960;

		public class MEASURE {
			public int Denominator {
				get { return (int)mEvent.Value >> 8; }
				set { mEvent.Value = (value << 8) | ((int)mEvent.Value & 0xFF); }
			}
			public int Numerator {
				get { return (int)mEvent.Value & 0xFF; }
				set { mEvent.Value = ((int)mEvent.Value & 0xFF00) | value; }
			}
			public int Tick {
				get { return mEvent.Tick; }
				set { mEvent.Tick = value; }
			}
			public int UnitTick { get { return 4 * BASE_TICK * Numerator / Denominator; } }
			Event mEvent;
			MEASURE() { }
			public MEASURE(Event ev) { mEvent = ev; }
		}

		public int Tick { get; private set; }
		public int Track { get; private set; }
		public int Channel { get; private set; }
		public TYPE Type { get; private set; }
		public float Value { get; set; }
		public string Text { get; set; }
		public bool Selected { get; set; } = false;

		int mValue;

		public int Tone {
			get { return (Type == TYPE.NOTE_OFF || Type == TYPE.NOTE_ON) ? mValue : -1; }
			private set { if (Type == TYPE.NOTE_OFF || Type == TYPE.NOTE_ON) mValue = value; }
		}
		public CTRL CtrlType {
			get { return Type == TYPE.CTRL_CHG ? (CTRL)mValue : CTRL.INVALID; }
			private set { if (Type == TYPE.CTRL_CHG) mValue = (int)value; }
		}
		public META MetaType {
			get { return Type == TYPE.META ? (META)mValue : META.INVALID; }
			private set { if (Type == TYPE.META) mValue = (int)value; }
		}
		public int TickEnd { get { return Type == TYPE.NOTE_ON ? NotePair.Tick : Tick; } }
		public MEASURE Measure { get { return MetaType == META.MEASURE ? new MEASURE(this) : null; } }

		public Event NotePair;

		Event() { }

		public Event(int tick, int track, int channel, int end, int tone, int velocity) {
			Tick = tick;
			Track = track;
			Channel = channel;
			Type = TYPE.NOTE_ON;
			Tone = tone;
			Value = velocity;
			NotePair = new Event() {
				Tick = end,
				Track = track,
				Channel = channel,
				Type = TYPE.NOTE_OFF,
				Tone = tone,
				Value = 0,
				NotePair = this
			};
		}

		public Event(int tick, int track, int channel, CTRL type, double value) {
			Tick = tick;
			Track = track;
			Channel = channel;
			Type = TYPE.CTRL_CHG;
			CtrlType = type;
			Value = (float)value;
		}

		public Event(int tick, int denominator, int numerator) {
			Tick = tick;
			Track = -3;
			Channel = -1;
			Type = TYPE.META;
			MetaType = META.MEASURE;
			Value = (denominator << 8) | numerator;
		}

		public Event(int tick, int key) {
			Tick = tick;
			Track = -2;
			Channel = -1;
			Type = TYPE.META;
			MetaType = META.KEY;
			Value = key;
		}

		public Event(int tick, double tempo) {
			Tick = tick;
			Track = -1;
			Channel = -1;
			Type = TYPE.META;
			MetaType = META.TEMPO;
			Value = (float)tempo;
		}

		public Event(Event note, string lyric) {
			Tick = note.Tick;
			Track = note.Track;
			Channel = note.Channel;
			Type = TYPE.META;
			MetaType = META.LYRIC;
			Text = lyric;
			NotePair = note;
		}

		public bool ContainsNote(int start, int end, int minTone, int maxTone, params int[] tracks) {
			return Type == TYPE.NOTE_ON
				&& Tone >= minTone && Tone <= maxTone
				&& ((Tick <= start && TickEnd >= end) || (Tick > start && Tick < end) || (TickEnd > start && TickEnd < end))
				&& tracks.Contains(Track);
		}

		public bool ContainsCtrl(int start, int end, CTRL type, params int[] tracks) {
			return Type == TYPE.CTRL_CHG
				&& CtrlType == type
				&& Tick >= start && Tick <= end
				&& tracks.Contains(Track);
		}

		public bool ContainsMeta(int start, int end, META type) {
			return Type == TYPE.META
				&& MetaType == type
				&& Tick >= start && Tick <= end;
		}
	}
}
