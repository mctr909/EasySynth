using System;

namespace SMF {
	/// <summary>
	/// 拍子・イベント
	/// </summary>
	public class Measure : Meta {
		public override int Track { get { return -3; } }

		/// <summary>
		/// 分子
		/// </summary>
		public int Numerator {
			get { return Data[3]; }
			set { Data[3] = (byte)value; }
		}

		/// <summary>
		/// 分母
		/// </summary>
		public int Denominator {
			get { return (int)Math.Pow(2, Data[4]); }
			set { Data[4] = (byte)Math.Log(value, 2); }
		}

		/// <summary>
		/// 小節単位時間
		/// </summary>
		public int Unit { get { return 4 * SMF.UnitTick * Numerator / Denominator; } }

		/// <summary>
		/// 拍単位時間
		/// </summary>
		public int UnitBeat { get { return 4 * SMF.UnitTick / Denominator; } }

		Measure() { }

		/// <summary>
		/// 拍子・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Measure(byte[] message) : base(message) { }

		/// <summary>
		/// 拍子・イベントを作成します
		/// </summary>
		/// <param name="numerator">分子</param>
		/// <param name="denominator">分母</param>
		public Measure(int numerator, int denominator) : base() {
			Data = new byte[] { (byte)MSG.META, (byte)META.MEASURE, 4, 0, 0, 24, 8 };
			Numerator = numerator;
			Denominator = denominator;
		}
	}
}
