using System;

namespace SMF {
	/// <summary>
	/// バイナリデータ・イベント
	/// </summary>
	public class Binary : Meta {
		/// <summary>
		/// バイナリデータ
		/// </summary>
		public byte[] Value {
			get {
				var start = 2 + SMF.GetDeltaSize(2, Data);
				var size = Data.Length - start;
				var ret = new byte[size];
				Array.Copy(Data, start, ret, 0, size);
				return ret;
			}
			set {
				var delta = SMF.GetDelta(value.Length);
				var start = 2 + delta.Length;
				Data = new byte[start + value.Length];
				Data[0] = (byte)MSG.META;
				Data[1] = (byte)META.DATA;
				Array.Copy(delta, 0, Data, 2, delta.Length);
				Array.Copy(value, 0, Data, start, delta.Length);
			}
		}

		/// <summary>
		/// バイナリデータ・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Binary(byte[] message) : base(message) { }

		/// <summary>
		/// バイナリデータ・イベントを作成します
		/// </summary>
		public Binary() : base() {
			Data = new byte[] { (byte)MSG.META, (byte)META.DATA, 0 };
		}
	}
}
