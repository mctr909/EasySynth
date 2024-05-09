using System;

namespace SMF {
	/// <summary>
	/// SMFイベント
	/// </summary>
	public abstract class Event {
		#region 実データ
		/// <summary>
		/// 位置
		/// </summary>
		public int Tick { get; set; }

		/// <summary>
		/// トラック番号
		/// </summary>
		public virtual int Track { get; set; }

		/// <summary>
		/// ポート番号
		/// </summary>
		public byte Port { get; private set; }

		/// <summary>
		/// 選択状態
		/// </summary>
		public bool Selected { get; set; }

		/// <summary>
		/// メッセージデータ
		/// </summary>
		public byte[] Data { get; protected set; }
		#endregion

		#region プロパティ
		/// <summary>
		/// チャンネル
		/// </summary>
		public byte Channel {
			get { return (byte)(MsgType < MSG.SYS_EX ? (Data[0] & 0xF) : SMF.InvalidChannel); }
			set {
				if (MsgType < MSG.SYS_EX) {
					Data[0] = (byte)((Data[0] & 0xF0) | (value & 0xF));
				}
			}
		}

		/// <summary>
		/// メッセージの種類
		/// </summary>
		public MSG MsgType { get { return (MSG)(Data[0] >= (int)MSG.SYS_EX ? Data[0] : (Data[0] & 0xF0)); } }

		/// <summary>
		/// コントロール・チェンジの種類
		/// </summary>
		public CTRL CtrlType { get { return MsgType == MSG.CTRL_CHG ? (CTRL)Data[1] : CTRL.INVALID; } }

		/// <summary>
		/// メタデータの種類
		/// </summary>
		public META MetaType { get { return MsgType == MSG.META ? (META)Data[1] : META.INVALID; } }
		#endregion

		protected Event(params byte[] message) {
			Data = new byte[message.Length];
			Array.Copy(message, Data, Data.Length);
		}
	}
}
