using System.Linq;

namespace SMF {
	/// <summary>
	/// ノート・イベント
	/// </summary>
	public class Note : Event {
		/// <summary>
		/// ノート・オンであるか
		/// </summary>
		public bool IsNoteOn { get { return (MSG)(Data[0] & 0xF0) == MSG.NOTE_ON && Data[2] > 0; } }

		/// <summary>
		/// 音程
		/// </summary>
		public int Tone {
			get { return Data[1]; }
			set {
				Data[1] = (byte)value;
				Pair.Data[1] = (byte)value;
			}
		}

		/// <summary>
		/// ベロシティ
		/// </summary>
		public int Velocity {
			get { return Data[2]; }
			set { Data[2] = (byte)value; }
		}

		/// <summary>
		/// 開始位置
		/// </summary>
		public int Start { get { return IsNoteOn ? Tick : Pair.Tick; } }

		/// <summary>
		/// 終了位置
		/// </summary>
		public int End { get { return IsNoteOn ? Pair.Tick : Tick; } }

		/// <summary>
		/// ペアイベント
		/// </summary>
		public Note Pair { get; set; }

		/// <summary>
		/// ノート・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Note(byte[] message) : base(message) { }

		/// <summary>
		/// ノート・イベントを作成
		/// </summary>
		/// <param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <param name="tone">音程</param>
		/// <param name="velocity">ベロシティ</param>
		public Note(int startTick, int endTick, int tone, int velocity) : base() {
			Data = new byte[] { (byte)MSG.NOTE_ON, (byte)tone, (byte)velocity };
			Tick = startTick;
			Pair = new Note(this, tone, endTick);
		}

		Note(Note noteOn, int tone, int tick) : base() {
			Data = new byte[] { (byte)MSG.NOTE_OFF, (byte)tone, 64 };
			Tick = tick;
			Pair = noteOn;
		}

		/// <summary>
		/// 引数の値に合致するイベントであるかを返します
		/// </summary>
		///	<param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <param name="minTone">最低音程</param>
		/// <param name="maxTone">最高音程</param>
		/// <param name="tracks">トラック</param>
		/// <returns></returns>
		public bool Contains(int startTick, int endTick, int minTone, int maxTone, params int[] tracks) {
			return Tone >= minTone && Tone <= maxTone
				&& ((Start <= startTick && End >= endTick) || (Start > startTick && Start < endTick) || (End > startTick && End < endTick))
				&& tracks.Contains(Track);
		}
	}
}
