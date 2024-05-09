using System.Linq;

namespace SMF {
	/// <summary>
	/// チャンネル・キー・プレッシャー・イベント
	/// </summary>
	public class ChPress : Event {
		/// <summary>
		/// キー・プレッシャーの値
		/// </summary>
		public int Value {
			get { return Data[1]; }
			set { Data[1] = (byte)value; }
		}

		ChPress() { }

		/// <summary>
		/// チャンネル・キー・プレッシャー・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public ChPress(byte[] message) : base(message) { }

		/// <summary>
		/// 引数の値に合致するイベントであるかを返します
		/// </summary>
		/// <param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <param name="tracks">トラック</param>
		/// <returns></returns>
		public bool Contains(int startTick, int endTick, params int[] tracks) {
			return Tick >= startTick && Tick <= endTick
				&& tracks.Contains(Track);
		}
	}
}
