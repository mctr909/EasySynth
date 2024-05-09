namespace SMF {
	/// <summary>
	/// テンポ・イベント
	/// </summary>
	public class Tempo : Meta {
		public override int Track { get { return -1; } }

		/// <summary>
		/// テンポ
		/// </summary>
		public double Value {
			get { return 60000000.0 / ((Data[3] << 16) | (Data[4] << 8) | Data[5]); }
			set {
				var x = (int)(60000000.0 / value);
				Data[3] = (byte)((x >> 16) & 0xFF);
				Data[4] = (byte)((x >> 8) & 0xFF);
				Data[5] = (byte)(x & 0xFF);
			}
		}

		Tempo() { }

		/// <summary>
		/// テンポ・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Tempo(byte[] message) : base(message) { }

		/// <summary>
		/// テンポ・イベントを作成します
		/// </summary>
		/// <param name="tempo">テンポ</param>
		public Tempo(double tempo) : base() {
			Data = new byte[] { (byte)MSG.META, (byte)META.TEMPO, 3, 0, 0, 0 };
			Value = tempo;
		}
	}
}
