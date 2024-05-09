namespace SMF {
	/// <summary>
	/// エンド・オブ・トラック・イベント
	/// </summary>
	public class EOT : Meta {
		/// <summary>
		/// エンド・オブ・トラック・イベントを作成します
		/// </summary>
		public EOT() : base() {
			Data = new byte[] { (byte)MSG.META, (byte)META.EOT, 0 };
		}
	}
}
