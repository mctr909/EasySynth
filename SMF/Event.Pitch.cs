using System.Linq;

namespace SMF {
	/// <summary>
	/// ピッチベンド・イベント
	/// </summary>
	public class Pitch : Event {
		/// <summary>
		/// ピッチベンドの値
		/// </summary>
		public int Value {
			get { return ((Data[1] << 8) | Data[2]) - 8192; }
			set {
				var temp = value + 8192;
				Data[1] = (byte)(temp >> 8);
				Data[2] = (byte)(temp & 0xFF);
			}
		}

		Pitch() { }

		/// <summary>
		/// ピッチベンド・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Pitch(byte[] message) : base(message) { }

		/// <summary>
		/// ピッチベンド・イベントを作成します
		/// </summary>
		/// <param name="pitch">ピッチベンドの値</param>
		public Pitch(int pitch) {
			Data = new byte[] { (byte)MSG.PITCH, 0, 0 };
			Value = pitch;
		}

		/// <summary>
		/// 引数の値に合致するイベントであるかを返します
		/// </summary>
		/// <param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <param name="tracks">トラック</param>
		/// <returns></returns>
		public bool Contains(int startTick, int endTick, params int[] tracks) {
			return MsgType == MSG.PITCH
				&& Tick >= startTick && Tick <= endTick
				&& tracks.Contains(Track);
		}
	}
}
