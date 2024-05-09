using System;
using System.Text;

namespace Synth {
	partial class SMF {
		/// <summary>
		/// イベント
		/// </summary>
		public class Event {
			/// <summary>
			/// 選択状態
			/// </summary>
			public bool Selected { get; set; }

			/// <summary>
			/// 位置
			/// </summary>
			public int Tick { get; set; }

			/// <summary>
			/// トラック番号
			/// </summary>
			public int Track { get; set; }

			/// <summary>
			/// ポート番号
			/// </summary>
			public int Port { get; private set; }

			/// <summary>
			/// チャンネル
			/// </summary>
			public int Channel {
				get { return MsgType < MSG.SYS_EX ? ((Port << 4) | (Msg[0] & 0xF)) : -1; }
				set {
					if (MsgType < MSG.SYS_EX) {
						Msg[0] = (byte)((Msg[0] & 0xF0) | (value & 0xF));
						Port = value >> 4;
					}
				}
			}

			/// <summary>
			/// メッセージデータ
			/// </summary>
			public byte[] Msg { get; protected set; }

			/// <summary>
			/// メッセージの種類
			/// </summary>
			public MSG MsgType { get { return (MSG)(Msg[0] < (int)MSG.SYS_EX ? (Msg[0] & 0xF0) : Msg[0]); } }

			/// <summary>
			/// コントロール・チェンジの種類
			/// </summary>
			public CTRL CtrlType { get { return MsgType == MSG.CTRL_CHG ? (CTRL)Msg[1] : CTRL.INVALID; } }

			/// <summary>
			/// メタデータの種類
			/// </summary>
			public META MetaType { get { return MsgType == MSG.META ? (META)Msg[1] : META.INVALID; } }

			/// <summary>
			/// 任意のイベントを作成
			/// </summary>
			/// <param name="message">メッセージデータ</param>
			public Event(params byte[] message) {
				Msg = new byte[message.Length];
				Array.Copy(message, Msg, Msg.Length);
			}

			Event() { }

			/// <summary>
			/// メッセージデータを設定
			/// </summary>
			/// <param name="message">メッセージデータ</param>
			protected void SetMessage(params byte[] message) {
				Msg = new byte[message.Length];
				Array.Copy(message, Msg, Msg.Length);
			}
		}

		/// <summary>
		/// ノート・イベント
		/// </summary>
		public class Note : Event {
			/// <summary>
			/// ノート・オンであるか
			/// </summary>
			public bool IsNoteOn { get { return Msg[0] >= (int)MSG.NOTE_ON && Msg[2] > 0; } }

			/// <summary>
			/// 音程
			/// </summary>
			public virtual int Tone {
				get { return Msg[1]; }
				set { Msg[1] = (byte)value; }
			}

			/// <summary>
			/// ベロシティ
			/// </summary>
			public virtual int Velocity {
				get { return Msg[2]; }
				set { Msg[2] = (byte)value; }
			}

			Note() { }

			/// <summary>
			/// ノート・イベントを作成します
			/// </summary>
			/// <param name="message"></param>
			public Note(byte[] message) : base(message) { }

			/// <summary>
			/// ノート・イベントを作成します
			/// </summary>
			/// <param name="type"></param>
			public Note(MSG type, int tone, int velocity) {
				Msg = new byte[] { (byte)type, (byte)tone, (byte)velocity };
			}
		}

		/// <summary>
		/// コントロール・チェンジ・イベント
		/// </summary>
		public class Ctrl : Event {
			/// <summary>
			/// コントロール・チェンジの種類
			/// </summary>
			public CTRL Type { get { return (CTRL)Msg[1]; } }

			/// <summary>
			/// 値
			/// </summary>
			public virtual int Value {
				get { return Msg[2]; }
				set { Msg[2] = (byte)value; }
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
			/// <param name="type"></param>
			/// <param name="value"></param>
			public Ctrl(CTRL type, int value) {
				Msg = new byte[] { (byte)MSG.CTRL_CHG, (byte)type, (byte)value };
			}
		}

		/// <summary>
		/// プログラム・チェンジ・イベント
		/// </summary>
		public class Prog : Event {
			/// <summary>
			/// プログラムナンバー
			/// </summary>
			public virtual int Number {
				get { return Msg[1]; }
				set { Msg[1] = (byte)value; }
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
			/// <param name="progNum"></param>
			public Prog(int progNum) {
				Msg = new byte[] { (byte)MSG.PROG_CHG, (byte)progNum };
			}
		}

		/// <summary>
		/// ピッチベンド・イベント
		/// </summary>
		public class Pitch : Event {
			/// <summary>
			/// 値
			/// </summary>
			public virtual int Value {
				get { return ((Msg[1] << 8) | Msg[2]) - 8192; }
				set {
					var temp = value + 8192;
					Msg[1] = (byte)(temp >> 8);
					Msg[2] = (byte)(temp & 0xFF);
				}
			}

			Pitch() { }

			/// <summary>
			/// ピッチベンド・チェンジ・イベントを作成します
			/// </summary>
			/// <param name="message"></param>
			public Pitch(byte[] message) : base(message) { }

			/// <summary>
			/// ピッチベンド・イベントを作成します
			/// </summary>
			/// <param name="pitch"></param>
			public Pitch(int pitch) {
				Msg = new byte[] { (byte)MSG.PITCH, 0, 0 };
				Value = pitch;
			}
		}

		/// <summary>
		/// テンポ・イベント
		/// </summary>
		public class Tempo : Event {
			/// <summary>
			/// 値
			/// </summary>
			public virtual double Value {
				get { return 60000000.0 / ((Msg[3] << 16) | (Msg[4] << 8) | Msg[5]); }
				set {
					var x = (int)(60000000.0 / value);
					Msg[3] = (byte)((x >> 16) & 0xFF);
					Msg[4] = (byte)((x >> 8) & 0xFF);
					Msg[5] = (byte)(x & 0xFF);
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
			/// <param name="tempo"></param>
			public Tempo(double tempo) {
				Msg = new byte[] { (byte)MSG.META, (byte)META.TEMPO, 3, 0, 0, 0 };
				Value = tempo;
			}
		}

		/// <summary>
		/// 拍子・イベント
		/// </summary>
		public class Measure : Event {
			/// <summary>
			/// 分子
			/// </summary>
			public virtual int Numerator {
				get { return Msg[3]; }
				set { Msg[3] = (byte)value; }
			}

			/// <summary>
			/// 分母
			/// </summary>
			public virtual int Denominator {
				get { return (int)Math.Pow(2, Msg[4]); }
				set { Msg[4] = (byte)Math.Log(value, 2); }
			}

			/// <summary>
			/// 小節単位時間
			/// </summary>
			public int UnitTick { get { return 4 * SMF.UnitTick * Numerator / Denominator; } }

			Measure() { }

			/// <summary>
			/// 拍子・イベントを作成します
			/// </summary>
			/// <param name="message"></param>
			public Measure(byte[] message) : base(message) { }

			/// <summary>
			/// 拍子・イベントを作成します
			/// </summary>
			/// <param name="numerator"></param>
			/// <param name="denominator"></param>
			public Measure(int numerator, int denominator) {
				Msg = new byte[] { (byte)MSG.META, (byte)META.MEASURE, 4, 0, 0, 24, 8 };
				Numerator = numerator;
				Denominator = denominator;
			}
		}

		/// <summary>
		/// 調・イベント
		/// </summary>
		public class Key : Event {
			/// <summary>
			/// 値
			/// </summary>
			public virtual KEY Value {
				get { return (KEY)((Msg[3] << 8) | Msg[4]); }
				set {
					Msg[3] = (byte)((int)value >> 8);
					Msg[4] = (byte)((int)value & 0xFF);
				}
			}

			/// <summary>
			/// 調号
			/// </summary>
			public virtual KEY_SIG Signature {
				get { return (KEY_SIG)(Msg[3] << 8); }
				set { Msg[3] = (byte)((int)value >> 8); }
			}

			/// <summary>
			/// 長調/短調
			/// </summary>
			public virtual KEY_SIG MajMin {
				get { return (KEY_SIG)Msg[4]; }
				set { Msg[4] = (byte)((int)value & 0xFF); }
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
			/// <param name="key"></param>
			public Key(KEY key) {
				Msg = new byte[] { (byte)MSG.META, (byte)META.KEY, 2, 0, 0 };
				Value = key;
			}

			/// <summary>
			/// 調・イベントを作成します
			/// </summary>
			/// <param name="keySignature"></param>
			public Key(KEY_SIG keySignature) {
				Msg = new byte[] { (byte)MSG.META, (byte)META.KEY, 2, 0, 0 };
				Value = (KEY)keySignature;
			}
		}

		/// <summary>
		/// バイナリデータ・イベント
		/// </summary>
		public class Binary : Event {
			/// <summary>
			/// 値
			/// </summary>
			public virtual byte[] Value {
				get {
					var start = 2 + GetDeltaSize(2, Msg);
					var size = Msg.Length - start;
					var ret = new byte[size];
					Array.Copy(Msg, start, ret, 0, size);
					return ret;
				}
				set {
					var delta = GetDelta(value.Length);
					var start = 2 + delta.Length;
					Msg = new byte[start + value.Length];
					Msg[0] = (byte)MSG.META;
					Msg[1] = (byte)META.DATA;
					Array.Copy(delta, 0, Msg, 2, delta.Length);
					Array.Copy(value, 0, Msg, start, delta.Length);
				}
			}

			/// <summary>
			/// バイナリデータ・イベントを作成します
			/// </summary>
			/// <param name="message"></param>
			public Binary(byte[] message) : base(message) { }

			/// <summary>
			/// バイナリデータ・イベントを作成します
			/// </summary>
			public Binary() {
				Msg = new byte[] { (byte)MSG.META, (byte)META.DATA, 0 };
			}
		}

		/// <summary>
		/// 文字列データ・イベント
		/// </summary>
		public class Text : Event {
			/// <summary>
			/// 値
			/// </summary>
			public virtual string Value {
				get {
					var start = 2 + GetDeltaSize(2, Msg);
					return Encoding.Default.GetString(Msg, start, Msg.Length - start);
				}
				set {
					var type = Msg[1];
					var data = Encoding.Default.GetBytes(value);
					var delta = GetDelta(data.Length);
					var start = 2 + delta.Length;
					Msg = new byte[start + data.Length];
					Msg[0] = (byte)MSG.META;
					Msg[1] = type;
					Array.Copy(delta, 0, Msg, 2, delta.Length);
					Array.Copy(data, 0, Msg, start, data.Length);
				}
			}

			Text() { }

			/// <summary>
			/// 文字列データ・イベントを作成します
			/// </summary>
			/// <param name="message"></param>
			public Text(byte[] message) : base(message) { }

			/// <summary>
			/// 文字列データ・イベントを作成します
			/// </summary>
			/// <param name="type"></param>
			/// <param name="value"></param>
			public Text(TEXT type, string value) {
				Msg = new byte[] { (byte)MSG.META, (byte)type, 0 };
				Value = value;
			}
		}
	}
}
