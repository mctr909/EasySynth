namespace SMF {
	/// <summary>
	/// メタ・イベント
	/// </summary>
	public class Meta : Event {
		protected Meta() { }

		/// <summary>
		/// メタ・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Meta(params byte[] message) : base(message) { }

		/// <summary>
		/// 引数の値に合致するイベントであるかを返します
		/// </summary>
		/// <param name="type">メタ・イベントの種類</param>
		/// <param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <returns></returns>
		public bool Contains(META type, int startTick, int endTick) {
			return MetaType == type && Tick >= startTick && Tick <= endTick;
		}
	}
}
