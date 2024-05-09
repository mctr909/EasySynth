using System.Linq;

namespace SMF {
	/// <summary>
	/// プログラム・チェンジ・イベント
	/// </summary>
	public class Prog : Event {
		/// <summary>
		/// プログラムナンバー
		/// </summary>
		public int Number {
			get { return Data[1]; }
			set { Data[1] = (byte)value; }
		}

		Prog() { }

		/// <summary>
		/// プログラム・チェンジ・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Prog(byte[] message) : base(message) { }

		/// <summary>
		/// プログラム・チェンジ・イベントを作成します
		/// </summary>
		/// <param name="number">プログラムナンバー</param>
		public Prog(int number) {
			Data = new byte[] { (byte)MSG.PROG_CHG, (byte)number };
		}

		/// <summary>
		/// 引数の値に合致するイベントであるかを返します
		/// </summary>
		/// <param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <param name="tracks">トラック</param>
		/// <returns></returns>
		public bool Contains(int startTick, int endTick, params int[] tracks) {
			return MsgType == MSG.PROG_CHG
				&& Tick >= startTick && Tick <= endTick
				&& tracks.Contains(Track);
		}
	}
}
