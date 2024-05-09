using System.Linq;
using static Synth.SMF;

namespace Synth {
	/// <summary>
	/// ノート・イベント
	/// </summary>
	public class Note : SMF.Note {
		/// <summary>
		/// 音程
		/// </summary>
		public override int Tone {
			get { return Msg[1]; }
			set {
				Msg[1] = (byte)value;
				Pair.Msg[1] = (byte)value;
			}
		}

		/// <summary>
		/// 終了位置
		/// </summary>
		public int End { get { return IsNoteOn ? Pair.Tick : Tick; } }

		/// <summary>
		/// ペアイベント
		/// </summary>
		public Note Pair { get; set; }

		/// <summary>
		/// ノート・イベントを作成
		/// </summary>
		public Note(int start, int end, int tone, int velocity) : base(MSG.NOTE_ON, tone, velocity) {
			Tick = start;
			Pair = new Note(this, tone, end);
		}

		Note(Note noteOn, int tone, int tick) : base(MSG.NOTE_OFF, tone, 64) {
			Tick = tick;
			Pair = noteOn;
		}

		public bool Contains(int start, int end, int minTone, int maxTone, params int[] tracks) {
			return Tone >= minTone && Tone <= maxTone
				&& ((Tick <= start && End >= end) || (Tick > start && Tick < end) || (End > start && End < end))
				&& tracks.Contains(Track);
		}
	}

	/// <summary>
	/// コントロール・イベント
	/// </summary>
	public class Ctrl : SMF.Ctrl {
		/// <summary>
		/// コントロールの種類
		/// </summary>
		public enum TYPE : byte {
			BANK_MSB = CTRL.BANK_MSB,
			BANK_LSB = CTRL.BANK_LSB,

			MOD = CTRL.MOD,

			POLTA_TIME   = CTRL.POLTA_TIME,
			POLTA_SWITCH = CTRL.POLTA_SWITCH,

			VOL  = CTRL.VOL,
			PAN  = CTRL.PAN,
			EXP  = CTRL.EXP,
			HOLD = CTRL.HOLD1,

			RESONANCE = CTRL.RESONANCE,
			CUTOFF    = CTRL.CUTOFF,

			REV_SEND = CTRL.REV_SEND,
			CHO_SEND = CTRL.CHO_SEND,
			DEL_SEND = CTRL.DEL_SEND,

			ALL_SOUND_OFF  = CTRL.ALL_SOUND_OFF,
			RESET_ALL_CTRL = CTRL.RESET_ALL_CTRL,
			ALL_NOTE_OFF   = CTRL.ALL_NOTE_OFF,

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

			INVALID = CTRL.INVALID
		}

		/// <summary>
		/// コントロールの値
		/// </summary>
		public double CtrlValue { get; set; }

		/// <summary>
		/// コントロール・イベントを作成
		/// </summary>
		/// <param name="ev"></param>
		public Ctrl(Event ev) : base(ev.Msg) {
			switch (MsgType) {
			case MSG.CTRL_CHG:
				switch (CtrlType) {
				case CTRL.MOD:
				case CTRL.VOL:
				case CTRL.EXP:
				case CTRL.RESONANCE:
				case CTRL.REV_SEND:
				case CTRL.CHO_SEND:
				case CTRL.DEL_SEND:
					CtrlValue = new SMF.Ctrl(Msg).Value / 127.0;
					break;
				case CTRL.PAN:
					CtrlValue = (new SMF.Ctrl(Msg).Value - 64) / 64.0;
					break;
				case CTRL.HOLD1:
					CtrlValue = new SMF.Ctrl(Msg).Value < 64 ? 0 : 1;
					break;
				case CTRL.CUTOFF:
					CtrlValue = (new SMF.Ctrl(Msg).Value - 64) / 64.0 + 1;
					break;
				}
				break;
			case MSG.PROG_CHG:
				CtrlValue = new Prog(ev.Msg).Number;
				SetMessage((byte)MSG.CTRL_CHG, (byte)TYPE.PROG_CHG, 0);
				break;
			case MSG.PITCH:
				CtrlValue = new Pitch(ev.Msg).Value;
				SetMessage((byte)MSG.CTRL_CHG, (byte)TYPE.PITCH, 0);
				break;
			}
		}

		/// <summary>
		/// コントロール・イベントを作成
		/// </summary>
		/// <param name="type">コントロールの種類</param>
		/// <param name="value">値</param>
		public Ctrl(TYPE type, double value) : base((CTRL)type, 0) {
			CtrlValue = value;
		}

		public bool Contains(int start, int end, TYPE type, params int[] tracks) {
			return CtrlType == (CTRL)type
				&& Tick >= start && Tick <= end
				&& tracks.Contains(Track);
		}
	}
}
