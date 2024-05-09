using System;

namespace SMF {
	/// <summary>
	/// システム・エクスクルーシブ・イベント
	/// </summary>
	public class SysEx : Event {
		/// <summary>
		/// システム・エクスクルーシブの値
		/// </summary>
		public byte[] Value {
			get {
				var start = 1;
				var size = Data.Length - start;
				var ret = new byte[size];
				Array.Copy(Data, start, ret, 0, size);
				return ret;
			}
			set {
				var start = 1;
				Data = new byte[start + value.Length];
				Data[0] = (byte)MSG.SYS_EX;
				Array.Copy(value, 0, Data, start, value.Length);
			}
		}

		/// <summary>
		/// システム・エクスクルーシブ・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public SysEx(byte[] message) : base(message) { }

		/// <summary>
		/// システム・エクスクルーシブ・イベントを作成します
		/// </summary>
		public SysEx() : base() {
			Data = new byte[] { (byte)MSG.SYS_EX, 0xF7 };
		}

		/// <summary>
		/// 引数の値に合致するイベントであるかを返します
		/// </summary>
		/// <param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <returns></returns>
		public bool Contains(int startTick, int endTick) {
			return MsgType == MSG.SYS_EX && Tick >= startTick && Tick <= endTick;
		}
	}
}
