using System.Linq;

namespace SMF {
	/// <summary>
	/// コントロール・チェンジ・イベント
	/// </summary>
	public class Ctrl : Event {
		/// <summary>
		/// コントロール・チェンジの種類
		/// </summary>
		public CTRL Type { get { return (CTRL)Data[1]; } }

		/// <summary>
		/// コントロール・チェンジの値
		/// </summary>
		public int Value {
			get { return Data[2]; }
			set { Data[2] = (byte)value; }
		}

		Ctrl() { }

		/// <summary>
		/// コントロール・チェンジ・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Ctrl(byte[] message) : base(message) { }

		/// <summary>
		/// コントロール・チェンジ・イベントを作成します
		/// </summary>
		/// <param name="type">コントロール・チェンジの種類</param>
		public Ctrl(CTRL type) {
			Data = new byte[] { (byte)MSG.CTRL_CHG, (byte)type, 0 };
		}

		/// <summary>
		/// 引数の値に合致するイベントであるかを返します
		/// </summary>
		/// <param name="type">コントロール・チェンジの種類</param>
		/// <param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <param name="tracks">トラック</param>
		/// <returns></returns>
		public bool Contains(CTRL type, int startTick, int endTick, params int[] tracks) {
			return CtrlType == type
				&& Tick >= startTick && Tick <= endTick
				&& tracks.Contains(Track);
		}
	}
}
