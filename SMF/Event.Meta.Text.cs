using System.Text;
using System;

namespace SMF {
	/// <summary>
	/// 文字列・イベント
	/// </summary>
	public class Text : Meta {
		/// <summary>
		/// 文字列
		/// </summary>
		public string Value {
			get {
				var start = 2 + SMF.GetDeltaSize(2, Data);
				return Encoding.Default.GetString(Data, start, Data.Length - start);
			}
			set {
				var type = Data[1];
				var data = Encoding.Default.GetBytes(value);
				var delta = SMF.GetDelta(data.Length);
				var start = 2 + delta.Length;
				Data = new byte[start + data.Length];
				Data[0] = (byte)MSG.META;
				Data[1] = type;
				Array.Copy(delta, 0, Data, 2, delta.Length);
				Array.Copy(data, 0, Data, start, data.Length);
			}
		}

		Text() { }

		/// <summary>
		/// 文字列・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Text(byte[] message) : base(message) { }

		/// <summary>
		/// 文字列・イベントを作成します
		/// </summary>
		/// <param name="type">文字列の種類</param>
		public Text(TEXT type) : base() {
			Data = new byte[] { (byte)MSG.META, (byte)type, 0 };
		}
	}
}
