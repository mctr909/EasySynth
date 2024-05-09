namespace SMF {
	/// <summary>
	/// 調・イベント
	/// </summary>
	public class Key : Meta {
		public override int Track { get { return -2; } }

		/// <summary>
		/// 調
		/// </summary>
		public KEY Value {
			get { return (KEY)((Data[3] << 8) | Data[4]); }
			set {
				Data[3] = (byte)((int)value >> 8);
				Data[4] = (byte)((int)value & 0xFF);
			}
		}

		/// <summary>
		/// 調号
		/// </summary>
		public KEY_SIG Signature {
			get { return (KEY_SIG)(Data[3] << 8); }
			set { Data[3] = (byte)((int)value >> 8); }
		}

		/// <summary>
		/// 長調/短調
		/// </summary>
		public KEY_SIG MajMin {
			get { return (KEY_SIG)Data[4]; }
			set { Data[4] = (byte)((int)value & 0xFF); }
		}

		Key() { }

		/// <summary>
		/// 調・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Key(byte[] message) : base(message) { }

		/// <summary>
		/// 調・イベントを作成します
		/// </summary>
		/// <param name="key">調</param>
		public Key(KEY key) : base() {
			Data = new byte[] { (byte)MSG.META, (byte)META.KEY, 2, 0, 0 };
			Value = key;
		}

		/// <summary>
		/// 調・イベントを作成します
		/// </summary>
		/// <param name="keySignature">調号</param>
		public Key(KEY_SIG keySignature) : base() {
			Data = new byte[] { (byte)MSG.META, (byte)META.KEY, 2, 0, 0 };
			Value = (KEY)keySignature;
		}
	}
}
