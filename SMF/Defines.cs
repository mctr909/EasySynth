namespace SMF {
	partial class SMF {
		/// <summary>
		/// 4分音符単位時間
		/// </summary>
		public const int UnitTick = 960;

		/// <summary>
		/// 無効なチャンネル
		/// </summary>
		public const byte InvalidChannel = 255;
	}

	/// <summary>
	/// メッセージの種類
	/// </summary>
	public enum MSG : byte {
		/// <summary>
		/// ノート・オフ
		/// </summary>
		NOTE_OFF = 0x80,
		/// <summary>
		/// ノート・オン
		/// </summary>
		NOTE_ON = 0x90,
		/// <summary>
		/// キー・プレッシャー
		/// </summary>
		KEY_PRESS = 0xA0,
		/// <summary>
		/// コントロール・チェンジ
		/// </summary>
		CTRL_CHG = 0xB0,
		/// <summary>
		/// プログラム・チェンジ
		/// </summary>
		PROG_CHG = 0xC0,
		/// <summary>
		/// チャンネル・プレッシャー
		/// </summary>
		CH_PRESS = 0xD0,
		/// <summary>
		/// ピッチベンド
		/// </summary>
		PITCH = 0xE0,
		/// <summary>
		/// システムエクスクルーシブ
		/// </summary>
		SYS_EX = 0xF0,
		/// <summary>
		/// メタデータ
		/// </summary>
		META = 0xFF
	}

	/// <summary>
	/// コントロール・チェンジの種類
	/// </summary>
	public enum CTRL : byte {
		/// <summary>
		/// バンクセレクト(MSB)
		/// </summary>
		BANK_MSB = 0,
		/// <summary>
		/// バンクセレクト(LSB)
		/// </summary>
		BANK_LSB = 32,
		/// <summary>
		/// ビブラートの深さ
		/// </summary>
		MOD = 1,
		/// <summary>
		/// ポルタメント・タイム
		/// </summary>
		POLTA_TIME = 5,
		/// <summary>
		/// ポルタンメント・オン/オフ
		/// </summary>
		POLTA_SWITCH = 65,
		/// <summary>
		/// チャンネル・ボリューム
		/// </summary>
		VOL = 7,
		/// <summary>
		/// 定位
		/// </summary>
		PAN = 10,
		/// <summary>
		/// エクスプレッション
		/// </summary>
		EXP = 11,
		/// <summary>
		/// ホールド１（ダンパー） 
		/// </summary>
		HOLD1 = 64,
		/// <summary>
		/// フィルター・レゾナンス
		/// </summary>
		RESONANCE = 71,
		/// <summary>
		/// リリース・タイム
		/// </summary>
		RELEASE = 72,
		/// <summary>
		/// アタック・タイム
		/// </summary>
		ATTACK = 73,
		/// <summary>
		/// フィルター・カットオフ
		/// </summary>
		CUTOFF = 74,
		/// <summary>
		/// ビブラート・レイト
		/// </summary>
		VIB_RATE = 76,
		/// <summary>
		/// ビブラート・デプス
		/// </summary>
		VIB_DEPTH = 77,
		/// <summary>
		/// ビブラート・ディレイ
		/// </summary>
		VIB_DELAY = 78,
		/// <summary>
		/// リバーブ・センドレベル
		/// </summary>
		REV_SEND = 91,
		/// <summary>
		/// コーラス・センドレベル
		/// </summary>
		CHO_SEND = 93,
		/// <summary>
		/// ディレイ・センドレベル
		/// </summary>
		DEL_SEND = 94,

		/// <summary>
		/// データエントリー(MSB)
		/// </summary>
		DATA_MSB = 6,
		/// <summary>
		/// データエントリー(LSB)
		/// </summary>
		DATA_LSB = 38,
		/// <summary>
		/// ノンレジスタード・パラメーター(LSB)
		/// </summary>
		NRPN_LSB = 98,
		/// <summary>
		/// ノンレジスタード・パラメーター(MSB)
		/// </summary>
		NRPN_MSB = 99,
		/// <summary>
		/// レジスタード・パラメーター(LSB)
		/// </summary>
		RPN_LSB = 100,
		/// <summary>
		/// レジスタード・パラメーター(MSB)
		/// </summary>
		RPN_MSB = 101,

		/// <summary>
		/// オール・サウンド・オフ
		/// </summary>
		ALL_SOUND_OFF = 120,
		/// <summary>
		/// リセット・オール・コントローラー
		/// </summary>
		RESET_ALL_CTRL = 121,
		/// <summary>
		/// ローカル・コントロール
		/// </summary>
		LOCAL_CTRL = 122,
		/// <summary>
		/// オール・ノート・オフ
		/// </summary>
		ALL_NOTE_OFF = 123,

		/// <summary>
		/// 無効値
		/// </summary>
		INVALID = 255
	}

	/// <summary>
	/// レジスタード・パラメーターの種類
	/// </summary>
	public enum RPN : ushort {
		/// <summary>
		/// ピッチベンド・レンジ
		/// </summary>
		BEND_RANGE = 0x0000,
		/// <summary>
		/// チャンネル・ファイン・チューニング
		/// </summary>
		FINE_TUNE = 0x0001,
		/// <summary>
		/// チャンネル・コース・チューニング
		/// </summary>
		COURSE_TUNE = 0x0002,
		/// <summary>
		/// ビブラート・デプス・レンジ
		/// </summary>
		VIB_RANGE = 0x0005,
		/// <summary>
		/// NULL値
		/// </summary>
		NULL = 0x7F7F
	}

	/// <summary>
	/// メタデータの種類
	/// </summary>
	public enum META : byte {
		/// <summary>
		/// 文字列
		/// </summary>
		TEXT = 1,
		/// <summary>
		/// 著作者
		/// </summary>
		COPYRIGHT = 2,
		/// <summary>
		/// トラック名
		/// </summary>
		TRACK_NAME = 3,
		/// <summary>
		/// 楽器名
		/// </summary>
		INST_NAME = 4,
		/// <summary>
		/// 歌詞
		/// </summary>
		LYRIC = 5,
		/// <summary>
		/// マーカー
		/// </summary>
		MARKER = 6,
		/// <summary>
		/// キューポイント
		/// </summary>
		QUEUE = 7,
		/// <summary>
		/// 曲名
		/// </summary>
		PROG_NAME = 8,
		/// <summary>
		/// デバイス名
		/// </summary>
		DEVICE_NAME = 9,

		/// <summary>
		/// シーケンス番号
		/// <para>2バイトデータ</para>
		/// </summary>
		SEQ_NO = 0x00,
		/// <summary>
		/// チャンネル・プリフィックス
		/// <para>1バイトデータ</para>
		/// </summary>
		CH_PREFIX = 0x20,
		/// <summary>
		/// ポート番号
		/// <para>1バイトデータ</para>
		/// </summary>
		PORT = 0x21,
		/// <summary>
		/// トラック終端
		/// </summary>
		EOT = 0x2F,

		/// <summary>
		/// テンポ
		/// </summary>
		TEMPO = 0x51,
		/// <summary>
		/// 拍子
		/// </summary>
		MEASURE = 0x58,
		/// <summary>
		/// 調
		/// </summary>
		KEY = 0x59,

		/// <summary>
		/// バイナリデータ
		/// </summary>
		DATA = 0x7F,

		/// <summary>
		/// 無効値
		/// </summary>
		INVALID = 0xFF
	}

	/// <summary>
	/// 文字列の種類
	/// </summary>
	public enum TEXT : byte {
		TEXT        = META.TEXT,
		COPYRIGHT   = META.COPYRIGHT,
		TRACK_NAME  = META.TRACK_NAME,
		INST_NAME   = META.INST_NAME,
		LYRIC       = META.LYRIC,
		MARKER      = META.MARKER,
		QUEUE       = META.QUEUE,
		PROG_NAME   = META.PROG_NAME,
		DEVICE_NAME = META.DEVICE_NAME
	}

	/// <summary>
	/// 調号
	/// </summary>
	public enum KEY_SIG : ushort {
		MAJOR = 0x0000,
		MINOR = 0x0001,
		b7 = 0xF900,
		b6 = 0xFA00,
		b5 = 0xFB00,
		b4 = 0xFC00,
		b3 = 0xFD00,
		b2 = 0xFE00,
		b1 = 0xFF00,
		N  = 0x0000,
		s1 = 0x0100,
		s2 = 0x0200,
		s3 = 0x0300,
		s4 = 0x0400,
		s5 = 0x0500,
		s6 = 0x0600,
		s7 = 0x0700
	}

	/// <summary>
	/// 調
	/// </summary>
	public enum KEY {
		Cb_MAJOR = KEY_SIG.b7 | KEY_SIG.MAJOR,
		Ab_MINOR = KEY_SIG.b7 | KEY_SIG.MINOR,
		Gb_MAJOR = KEY_SIG.b6 | KEY_SIG.MAJOR,
		Eb_MINOR = KEY_SIG.b6 | KEY_SIG.MINOR,
		Db_MAJOR = KEY_SIG.b5 | KEY_SIG.MAJOR,
		Bb_MINOR = KEY_SIG.b5 | KEY_SIG.MINOR,
		Ab_MAJOR = KEY_SIG.b4 | KEY_SIG.MAJOR,
		F_MINOR  = KEY_SIG.b4 | KEY_SIG.MINOR,
		Eb_MAJOR = KEY_SIG.b3 | KEY_SIG.MAJOR,
		C_MINOR  = KEY_SIG.b3 | KEY_SIG.MINOR,
		Bb_MAJOR = KEY_SIG.b2 | KEY_SIG.MAJOR,
		G_MINOR  = KEY_SIG.b2 | KEY_SIG.MINOR,
		F_MAJOR  = KEY_SIG.b1 | KEY_SIG.MAJOR,
		D_MINOR  = KEY_SIG.b1 | KEY_SIG.MINOR,
		C_MAJOR  = KEY_SIG.N  | KEY_SIG.MAJOR,
		A_MINOR  = KEY_SIG.N  | KEY_SIG.MINOR,
		G_MAJOR  = KEY_SIG.s1 | KEY_SIG.MAJOR,
		E_MINOR  = KEY_SIG.s1 | KEY_SIG.MINOR,
		D_MAJOR  = KEY_SIG.s2 | KEY_SIG.MAJOR,
		B_MINOR  = KEY_SIG.s2 | KEY_SIG.MINOR,
		A_MAJOR  = KEY_SIG.s3 | KEY_SIG.MAJOR,
		Fs_MINOR = KEY_SIG.s3 | KEY_SIG.MINOR,
		E_MAJOR  = KEY_SIG.s4 | KEY_SIG.MAJOR,
		Cs_MINOR = KEY_SIG.s4 | KEY_SIG.MINOR,
		B_MAJOR  = KEY_SIG.s5 | KEY_SIG.MAJOR,
		Gs_MINOR = KEY_SIG.s5 | KEY_SIG.MINOR,
		Fs_MAJOR = KEY_SIG.s6 | KEY_SIG.MAJOR,
		Ds_MINOR = KEY_SIG.s6 | KEY_SIG.MINOR,
		Cs_MAJOR = KEY_SIG.s7 | KEY_SIG.MAJOR,
		As_MINOR = KEY_SIG.s7 | KEY_SIG.MINOR,

		INVALID = 0xFFFF
	}
}
