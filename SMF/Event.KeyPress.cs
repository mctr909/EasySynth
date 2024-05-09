using System.Linq;

namespace SMF {
	/// <summary>
	/// ポリフォニック・キー・プレッシャー・イベント
	/// </summary>
	public class KeyPress : Event {
		/// <summary>
		/// 音程
		/// </summary>
		public int Tone {
			get { return Data[1]; }
			set { Data[1] = (byte)value; }
		}

		/// <summary>
		/// キー・プレッシャーの値
		/// </summary>
		public int Value {
			get { return Data[2]; }
			set { Data[2] = (byte)value; }
		}

		KeyPress() { }

		/// <summary>
		/// ポリフォニック・キー・プレッシャー・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public KeyPress(byte[] message) : base(message) { }

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
				&& Tick >= startTick && Tick <= endTick
				&& tracks.Contains(Track);
		}
	}
}
