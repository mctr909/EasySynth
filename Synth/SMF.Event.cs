using System;
using System.Linq;
using System.Text;

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
		public virtual int Track { get; set; }

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
		public int Tone {
			get { return Msg[1]; }
			set {
				Msg[1] = (byte)value;
				Pair.Msg[1] = (byte)value;
			}
		}

		/// <summary>
		/// ベロシティ
		/// </summary>
		public int Velocity {
			get { return Msg[2]; }
			set { Msg[2] = (byte)value; }
		}

		/// <summary>
		/// 終了位置
		/// </summary>
		public int End { get { return IsNoteOn ? Pair.Tick : Tick; } }

		/// <summary>
		/// ペアイベント
		/// </summary>
		public Note Pair { get; set; }

		/// <summary>
		/// ノート・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Note(byte[] message) : base(message) { }

		/// <summary>
		/// ノート・イベントを作成
		/// </summary>
		/// <param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <param name="tone">音程</param>
		/// <param name="velocity">ベロシティ</param>
		public Note(int startTick, int endTick, int tone, int velocity) : base() {
			Msg = new byte[] { (byte)MSG.NOTE_ON, (byte)tone, (byte)velocity };
			Tick = startTick;
			Pair = new Note(this, tone, endTick);
		}

		Note(Note noteOn, int tone, int tick) : base() {
			Msg = new byte[] { (byte)MSG.NOTE_OFF, (byte)tone, 64 };
			Tick = tick;
			Pair = noteOn;
		}

		/// <summary>
		/// 引数の値に合致するイベントであるかを返します
		/// </summary>
		///	<param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <param name="minTone">最低音程</param>
		/// <param name="maxTone">最高音程</param>
		/// <param name="tracks">トラック</param>
		/// <returns></returns>
		public bool Contains(int startTick, int endTick, int minTone, int maxTone, params int[] tracks) {
			return Tone >= minTone && Tone <= maxTone
				&& ((Tick <= startTick && End >= endTick) || (Tick > startTick && Tick < endTick) || (End > startTick && End < endTick))
				&& tracks.Contains(Track);
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
		/// コントロール・チェンジの値
		/// </summary>
		public int Value {
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
		/// <param name="type">コントロール・チェンジの種類</param>
		public Ctrl(CTRL type) {
			Msg = new byte[] { (byte)MSG.CTRL_CHG, (byte)type, 0 };
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

	/// <summary>
	/// プログラム・チェンジ・イベント
	/// </summary>
	public class Prog : Event {
		/// <summary>
		/// プログラムナンバー
		/// </summary>
		public int Number {
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
		/// <param name="number">プログラムナンバー</param>
		public Prog(int number) {
			Msg = new byte[] { (byte)MSG.PROG_CHG, (byte)number };
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

	/// <summary>
	/// ピッチベンド・イベント
	/// </summary>
	public class Pitch : Event {
		/// <summary>
		/// ピッチベンドの値
		/// </summary>
		public int Value {
			get { return ((Msg[1] << 8) | Msg[2]) - 8192; }
			set {
				var temp = value + 8192;
				Msg[1] = (byte)(temp >> 8);
				Msg[2] = (byte)(temp & 0xFF);
			}
		}

		Pitch() { }

		/// <summary>
		/// ピッチベンド・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Pitch(byte[] message) : base(message) { }

		/// <summary>
		/// ピッチベンド・イベントを作成します
		/// </summary>
		/// <param name="pitch">ピッチベンドの値</param>
		public Pitch(int pitch) {
			Msg = new byte[] { (byte)MSG.PITCH, 0, 0 };
			Value = pitch;
		}

		/// <summary>
		/// 引数の値に合致するイベントであるかを返します
		/// </summary>
		/// <param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <param name="tracks">トラック</param>
		/// <returns></returns>
		public bool Contains(int startTick, int endTick, params int[] tracks) {
			return MsgType == MSG.PITCH
				&& Tick >= startTick && Tick <= endTick
				&& tracks.Contains(Track);
		}
	}

	/// <summary>
	/// システム・エクスクルーシブ・イベント
	/// </summary>
	public class SysEx : Event {
		/// <summary>
		/// システム・エクスクルーシブの値
		/// </summary>
		public byte[] Value {
			get {
				var start = 1;
				var size = Msg.Length - start;
				var ret = new byte[size];
				Array.Copy(Msg, start, ret, 0, size);
				return ret;
			}
			set {
				var start = 1;
				Msg = new byte[start + value.Length];
				Msg[0] = (byte)MSG.SYS_EX;
				Array.Copy(value, 0, Msg, start, value.Length);
			}
		}

		/// <summary>
		/// システム・エクスクルーシブ・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public SysEx(byte[] message) : base(message) { }

		/// <summary>
		/// システム・エクスクルーシブ・イベントを作成します
		/// </summary>
		public SysEx() : base() {
			Msg = new byte[] { (byte)MSG.SYS_EX, 0xF7 };
		}

		/// <summary>
		/// 引数の値に合致するイベントであるかを返します
		/// </summary>
		/// <param name="startTick">開始位置</param>
		/// <param name="endTick">終了位置</param>
		/// <returns></returns>
		public bool Contains(int startTick, int endTick) {
			return MsgType == MSG.SYS_EX && Tick >= startTick && Tick <= endTick;
		}
	}

	/// <summary>
	/// メタ・イベント
	/// </summary>
	public class Meta : Event {
		/// <summary>
		/// メタ・イベントの値
		/// </summary>
		public byte[] Data {
			get {
				var start = 2 + GetDeltaSize(2, Msg);
				var size = Msg.Length - start;
				var ret = new byte[size];
				Array.Copy(Msg, start, ret, 0, size);
				return ret;
			}
		}

		protected Meta() { }

		/// <summary>
		/// メタ・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Meta(params byte[] message) : base(message) { }

		/// <summary>
		/// メタ・イベントを作成します
		/// </summary>
		/// <param name="type">メタ・イベントの種類</param>
		public Meta(META type) {
			Msg = new byte[] { (byte)MSG.META, (byte)type, 0 };
		}

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

	/// <summary>
	/// テンポ・イベント
	/// </summary>
	public class Tempo : Meta {
		public override int Track { get { return -1; } }

		/// <summary>
		/// テンポ
		/// </summary>
		public double Value {
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
		/// <param name="tempo">テンポ</param>
		public Tempo(double tempo) : base() {
			Msg = new byte[] { (byte)MSG.META, (byte)META.TEMPO, 3, 0, 0, 0 };
			Value = tempo;
		}
	}

	/// <summary>
	/// 拍子・イベント
	/// </summary>
	public class Measure : Meta {
		public override int Track { get { return -3; } }

		/// <summary>
		/// 分子
		/// </summary>
		public int Numerator {
			get { return Msg[3]; }
			set { Msg[3] = (byte)value; }
		}

		/// <summary>
		/// 分母
		/// </summary>
		public int Denominator {
			get { return (int)Math.Pow(2, Msg[4]); }
			set { Msg[4] = (byte)Math.Log(value, 2); }
		}

		/// <summary>
		/// 小節単位時間
		/// </summary>
		public int Unit { get { return 4 * UnitTick * Numerator / Denominator; } }

		/// <summary>
		/// 拍単位時間
		/// </summary>
		public int UnitBeat { get { return 4 * UnitTick / Denominator; } }

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
			Msg = new byte[] { (byte)MSG.META, (byte)META.MEASURE, 4, 0, 0, 24, 8 };
			Numerator = numerator;
			Denominator = denominator;
		}
	}

	/// <summary>
	/// 調・イベント
	/// </summary>
	public class Key : Meta {
		public override int Track { get { return -2; } }

		/// <summary>
		/// 調
		/// </summary>
		public KEY Value {
			get { return (KEY)((Msg[3] << 8) | Msg[4]); }
			set {
				Msg[3] = (byte)((int)value >> 8);
				Msg[4] = (byte)((int)value & 0xFF);
			}
		}

		/// <summary>
		/// 調号
		/// </summary>
		public KEY_SIG Signature {
			get { return (KEY_SIG)(Msg[3] << 8); }
			set { Msg[3] = (byte)((int)value >> 8); }
		}

		/// <summary>
		/// 長調/短調
		/// </summary>
		public KEY_SIG MajMin {
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
		/// <param name="key">調</param>
		public Key(KEY key) : base() {
			Msg = new byte[] { (byte)MSG.META, (byte)META.KEY, 2, 0, 0 };
			Value = key;
		}

		/// <summary>
		/// 調・イベントを作成します
		/// </summary>
		/// <param name="keySignature">調号</param>
		public Key(KEY_SIG keySignature) : base() {
			Msg = new byte[] { (byte)MSG.META, (byte)META.KEY, 2, 0, 0 };
			Value = (KEY)keySignature;
		}
	}

	/// <summary>
	/// 文字列・イベント
	/// </summary>
	public class Text : Meta {
		/// <summary>
		/// 文字列
		/// </summary>
		public string Value {
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
		/// 文字列・イベントを作成します
		/// </summary>
		/// <param name="message"></param>
		public Text(byte[] message) : base(message) { }

		/// <summary>
		/// 文字列・イベントを作成します
		/// </summary>
		/// <param name="type">文字列の種類</param>
		public Text(TEXT type) : base() {
			Msg = new byte[] { (byte)MSG.META, (byte)type, 0 };
		}
	}

	/// <summary>
	/// バイナリデータ・イベント
	/// </summary>
	public class Binary : Meta {
		/// <summary>
		/// バイナリデータ
		/// </summary>
		public byte[] Value {
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
		public Binary() : base() {
			Msg = new byte[] { (byte)MSG.META, (byte)META.DATA, 0 };
		}
	}
}
