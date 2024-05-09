
namespace EasySynth.Forms
{
	partial class PianoRoll
	{
		/// <summary>
		/// 必要なデザイナー変数です。
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 使用中のリソースをすべてクリーンアップします。
		/// </summary>
		/// <param name="disposing">マネージド リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows フォーム デザイナーで生成されたコード

		/// <summary>
		/// デザイナー サポートに必要なメソッドです。このメソッドの内容を
		/// コード エディターで変更しないでください。
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.Timer1 = new System.Windows.Forms.Timer(this.components);
			this.PicNote = new System.Windows.Forms.PictureBox();
			this.PicInput = new System.Windows.Forms.PictureBox();
			this.Vsb = new System.Windows.Forms.VScrollBar();
			this.Hsb = new System.Windows.Forms.HScrollBar();
			this.編集 = new System.Windows.Forms.ToolStripMenuItem();
			this.編集_書き込み = new System.Windows.Forms.ToolStripMenuItem();
			this.編集_1トラック選択 = new System.Windows.Forms.ToolStripMenuItem();
			this.編集_複数トラック選択 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
			this.編集_元に戻す = new System.Windows.Forms.ToolStripMenuItem();
			this.編集_やり直す = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
			this.編集_切り取り = new System.Windows.Forms.ToolStripMenuItem();
			this.編集_コピー = new System.Windows.Forms.ToolStripMenuItem();
			this.編集_貼り付け = new System.Windows.Forms.ToolStripMenuItem();
			this.編集_削除 = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象 = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_音符 = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_アクセント = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_強弱 = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_ピッチ = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_ビブラートの深さ = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
			this.編集対象_音色 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
			this.編集対象_音量 = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_定位 = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_コンプレッサー = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
			this.編集対象_コーラス = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_ディレイ = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_リバーブ = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
			this.編集対象_振幅EG = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_フィルターEG = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_ピッチEG = new System.Windows.Forms.ToolStripMenuItem();
			this.編集対象_ビブラートLFO = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
			this.編集対象_テンポ = new System.Windows.Forms.ToolStripMenuItem();
			this.時間単位 = new System.Windows.Forms.ToolStripMenuItem();
			this.時間単位_フリーハンド = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
			this.時間単位_8分 = new System.Windows.Forms.ToolStripMenuItem();
			this.時間単位_16分 = new System.Windows.Forms.ToolStripMenuItem();
			this.時間単位_32分 = new System.Windows.Forms.ToolStripMenuItem();
			this.時間単位_64分 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
			this.時間単位_8分3連 = new System.Windows.Forms.ToolStripMenuItem();
			this.時間単位_16分3連 = new System.Windows.Forms.ToolStripMenuItem();
			this.時間単位_32分3連 = new System.Windows.Forms.ToolStripMenuItem();
			this.時間単位_64分3連 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
			this.時間単位_8分5連 = new System.Windows.Forms.ToolStripMenuItem();
			this.時間単位_16分5連 = new System.Windows.Forms.ToolStripMenuItem();
			this.時間単位_32分5連 = new System.Windows.Forms.ToolStripMenuItem();
			this.時間単位_64分5連 = new System.Windows.Forms.ToolStripMenuItem();
			this.表示 = new System.Windows.Forms.ToolStripMenuItem();
			this.表示_次の小節へ移動 = new System.Windows.Forms.ToolStripMenuItem();
			this.表示_前の小節へ移動 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
			this.表示_時間方向拡大 = new System.Windows.Forms.ToolStripMenuItem();
			this.表示_時間方向縮小 = new System.Windows.Forms.ToolStripMenuItem();
			this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
			this.表示_音高方向拡大 = new System.Windows.Forms.ToolStripMenuItem();
			this.表示_音高方向縮小 = new System.Windows.Forms.ToolStripMenuItem();
			this.MenuStrip1 = new System.Windows.Forms.MenuStrip();
			((System.ComponentModel.ISupportInitialize)(this.PicNote)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.PicInput)).BeginInit();
			this.MenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// Timer1
			// 
			this.Timer1.Tick += new System.EventHandler(this.Timer1_Tick);
			// 
			// PicNote
			// 
			this.PicNote.BackColor = System.Drawing.Color.Transparent;
			this.PicNote.Location = new System.Drawing.Point(12, 27);
			this.PicNote.Name = "PicNote";
			this.PicNote.Size = new System.Drawing.Size(202, 195);
			this.PicNote.TabIndex = 0;
			this.PicNote.TabStop = false;
			// 
			// PicInput
			// 
			this.PicInput.BackColor = System.Drawing.Color.Transparent;
			this.PicInput.ErrorImage = null;
			this.PicInput.InitialImage = null;
			this.PicInput.Location = new System.Drawing.Point(237, 27);
			this.PicInput.Name = "PicInput";
			this.PicInput.Size = new System.Drawing.Size(202, 195);
			this.PicInput.TabIndex = 4;
			this.PicInput.TabStop = false;
			this.PicInput.MouseDown += new System.Windows.Forms.MouseEventHandler(this.PicInput_MouseDown);
			this.PicInput.MouseMove += new System.Windows.Forms.MouseEventHandler(this.PicInput_MouseMove);
			this.PicInput.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PicInput_MouseUp);
			// 
			// Vsb
			// 
			this.Vsb.LargeChange = 1;
			this.Vsb.Location = new System.Drawing.Point(217, 27);
			this.Vsb.Maximum = 127;
			this.Vsb.Name = "Vsb";
			this.Vsb.Size = new System.Drawing.Size(17, 195);
			this.Vsb.Value = 60;
			this.Vsb.SmallChange = 0;
			this.Vsb.TabStop = false;
			this.Vsb.Scroll += new System.Windows.Forms.ScrollEventHandler(this.Vsb_Scroll);
			// 
			// Hsb
			// 
			this.Hsb.LargeChange = 3840;
			this.Hsb.Location = new System.Drawing.Point(12, 225);
			this.Hsb.Maximum = 96000;
			this.Hsb.Name = "Hsb";
			this.Hsb.Size = new System.Drawing.Size(202, 17);
			this.Hsb.SmallChange = 0;
			this.Hsb.TabStop = false;
			this.Hsb.Scroll += new System.Windows.Forms.ScrollEventHandler(this.Hsb_Scroll);
			// 
			// 編集
			// 
			this.編集.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.編集_書き込み,
            this.編集_1トラック選択,
            this.編集_複数トラック選択,
            this.toolStripSeparator12,
            this.編集_元に戻す,
            this.編集_やり直す,
            this.toolStripSeparator13,
            this.編集_切り取り,
            this.編集_コピー,
            this.編集_貼り付け,
            this.編集_削除});
			this.編集.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集.Name = "編集";
			this.編集.Size = new System.Drawing.Size(57, 20);
			this.編集.Text = "編集(E)";
			// 
			// 編集_書き込み
			// 
			this.編集_書き込み.Image = global::EasySynth.Properties.Resources.mode_write;
			this.編集_書き込み.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集_書き込み.Name = "編集_書き込み";
			this.編集_書き込み.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.W)));
			this.編集_書き込み.Size = new System.Drawing.Size(204, 30);
			this.編集_書き込み.Text = "書き込み";
			this.編集_書き込み.Click += new System.EventHandler(this.編集_Click);
			// 
			// 編集_1トラック選択
			// 
			this.編集_1トラック選択.Image = global::EasySynth.Properties.Resources.mode_select;
			this.編集_1トラック選択.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集_1トラック選択.Name = "編集_1トラック選択";
			this.編集_1トラック選択.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.F)));
			this.編集_1トラック選択.Size = new System.Drawing.Size(204, 30);
			this.編集_1トラック選択.Text = "1トラック選択";
			this.編集_1トラック選択.Click += new System.EventHandler(this.編集_Click);
			// 
			// 編集_複数トラック選択
			// 
			this.編集_複数トラック選択.Image = global::EasySynth.Properties.Resources.mode_select_multi;
			this.編集_複数トラック選択.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集_複数トラック選択.Name = "編集_複数トラック選択";
			this.編集_複数トラック選択.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.G)));
			this.編集_複数トラック選択.Size = new System.Drawing.Size(204, 30);
			this.編集_複数トラック選択.Text = "複数トラック選択";
			this.編集_複数トラック選択.Click += new System.EventHandler(this.編集_Click);
			// 
			// toolStripSeparator12
			// 
			this.toolStripSeparator12.Name = "toolStripSeparator12";
			this.toolStripSeparator12.Size = new System.Drawing.Size(201, 6);
			// 
			// 編集_元に戻す
			// 
			this.編集_元に戻す.Name = "編集_元に戻す";
			this.編集_元に戻す.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Z)));
			this.編集_元に戻す.Size = new System.Drawing.Size(204, 30);
			this.編集_元に戻す.Text = "元に戻す(U)";
			this.編集_元に戻す.Click += new System.EventHandler(this.編集_Click);
			// 
			// 編集_やり直す
			// 
			this.編集_やり直す.Name = "編集_やり直す";
			this.編集_やり直す.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Y)));
			this.編集_やり直す.Size = new System.Drawing.Size(204, 30);
			this.編集_やり直す.Text = "やり直し(R)";
			this.編集_やり直す.Click += new System.EventHandler(this.編集_Click);
			// 
			// toolStripSeparator13
			// 
			this.toolStripSeparator13.Name = "toolStripSeparator13";
			this.toolStripSeparator13.Size = new System.Drawing.Size(201, 6);
			// 
			// 編集_切り取り
			// 
			this.編集_切り取り.Name = "編集_切り取り";
			this.編集_切り取り.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
			this.編集_切り取り.Size = new System.Drawing.Size(204, 30);
			this.編集_切り取り.Text = "切り取り(T)";
			this.編集_切り取り.Click += new System.EventHandler(this.編集_Click);
			// 
			// 編集_コピー
			// 
			this.編集_コピー.Name = "編集_コピー";
			this.編集_コピー.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
			this.編集_コピー.Size = new System.Drawing.Size(204, 30);
			this.編集_コピー.Text = "コピー(C)";
			this.編集_コピー.Click += new System.EventHandler(this.編集_Click);
			// 
			// 編集_貼り付け
			// 
			this.編集_貼り付け.Name = "編集_貼り付け";
			this.編集_貼り付け.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
			this.編集_貼り付け.Size = new System.Drawing.Size(204, 30);
			this.編集_貼り付け.Text = "貼り付け(P)";
			this.編集_貼り付け.Click += new System.EventHandler(this.編集_Click);
			// 
			// 編集_削除
			// 
			this.編集_削除.Name = "編集_削除";
			this.編集_削除.ShortcutKeys = System.Windows.Forms.Keys.Delete;
			this.編集_削除.Size = new System.Drawing.Size(204, 30);
			this.編集_削除.Text = "削除(D)";
			this.編集_削除.Click += new System.EventHandler(this.編集_Click);
			// 
			// 編集対象
			// 
			this.編集対象.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.編集対象_音符,
            this.編集対象_アクセント,
            this.編集対象_強弱,
            this.編集対象_ピッチ,
			this.編集対象_ビブラートの深さ,
            this.toolStripSeparator2,
            this.編集対象_音色,
            this.toolStripSeparator3,
            this.編集対象_音量,
            this.編集対象_定位,
            this.編集対象_コンプレッサー,
            this.toolStripSeparator4,
            this.編集対象_コーラス,
            this.編集対象_ディレイ,
            this.編集対象_リバーブ,
            this.toolStripSeparator5,
            this.編集対象_振幅EG,
            this.編集対象_フィルターEG,
            this.編集対象_ピッチEG,
            this.編集対象_ビブラートLFO,
            this.toolStripSeparator11,
            this.編集対象_テンポ});
			this.編集対象.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象.Name = "編集対象";
			this.編集対象.Size = new System.Drawing.Size(81, 20);
			this.編集対象.Text = "編集対象(T)";
			// 
			// 編集対象_音符
			// 
			this.編集対象_音符.Image = global::EasySynth.Properties.Resources.edit_note;
			this.編集対象_音符.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_音符.Name = "編集対象_音符";
			this.編集対象_音符.ShortcutKeys = System.Windows.Forms.Keys.F1;
			this.編集対象_音符.Size = new System.Drawing.Size(180, 30);
			this.編集対象_音符.Text = "音符";
			this.編集対象_音符.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// 編集対象_アクセント
			// 
			this.編集対象_アクセント.Image = global::EasySynth.Properties.Resources.edit_accent;
			this.編集対象_アクセント.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_アクセント.Name = "編集対象_アクセント";
			this.編集対象_アクセント.ShortcutKeys = System.Windows.Forms.Keys.F2;
			this.編集対象_アクセント.Size = new System.Drawing.Size(180, 30);
			this.編集対象_アクセント.Text = "アクセント";
			this.編集対象_アクセント.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// 編集対象_強弱
			// 
			this.編集対象_強弱.Image = global::EasySynth.Properties.Resources.edit_exp;
			this.編集対象_強弱.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_強弱.Name = "編集対象_強弱";
			this.編集対象_強弱.ShortcutKeys = System.Windows.Forms.Keys.F3;
			this.編集対象_強弱.Size = new System.Drawing.Size(180, 30);
			this.編集対象_強弱.Text = "強弱";
			this.編集対象_強弱.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// 編集対象_ピッチ
			// 
			this.編集対象_ピッチ.Image = global::EasySynth.Properties.Resources.edit_pitch;
			this.編集対象_ピッチ.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_ピッチ.Name = "編集対象_ピッチ";
			this.編集対象_ピッチ.ShortcutKeys = System.Windows.Forms.Keys.F4;
			this.編集対象_ピッチ.Size = new System.Drawing.Size(180, 30);
			this.編集対象_ピッチ.Text = "ピッチ";
			this.編集対象_ピッチ.Click += new System.EventHandler(this.編集対象_Click);
			//
			// 編集対象_ビブラートの深さ
			//
			this.編集対象_ビブラートの深さ.Image = global::EasySynth.Properties.Resources.edit_vib_depth;
			this.編集対象_ビブラートの深さ.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_ビブラートの深さ.Name = "編集対象_ビブラートの深さ";
			this.編集対象_ビブラートの深さ.Size = new System.Drawing.Size(180, 30);
			this.編集対象_ビブラートの深さ.Text = "ビブラートの深さ";
			this.編集対象_ビブラートの深さ.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// toolStripSeparator2
			// 
			this.toolStripSeparator2.Name = "toolStripSeparator2";
			this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
			// 
			// 編集対象_音色
			// 
			this.編集対象_音色.Image = global::EasySynth.Properties.Resources.edit_inst;
			this.編集対象_音色.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_音色.Name = "編集対象_音色";
			this.編集対象_音色.Size = new System.Drawing.Size(180, 30);
			this.編集対象_音色.Text = "音色";
			this.編集対象_音色.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// toolStripSeparator3
			// 
			this.toolStripSeparator3.Name = "toolStripSeparator3";
			this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
			// 
			// 編集対象_音量
			// 
			this.編集対象_音量.Image = global::EasySynth.Properties.Resources.edit_vol;
			this.編集対象_音量.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_音量.Name = "編集対象_音量";
			this.編集対象_音量.Size = new System.Drawing.Size(180, 30);
			this.編集対象_音量.Text = "音量";
			this.編集対象_音量.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// 編集対象_定位
			// 
			this.編集対象_定位.Image = global::EasySynth.Properties.Resources.edit_pan;
			this.編集対象_定位.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_定位.Name = "編集対象_定位";
			this.編集対象_定位.Size = new System.Drawing.Size(180, 30);
			this.編集対象_定位.Text = "定位";
			this.編集対象_定位.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// 編集対象_コンプレッサー
			// 
			this.編集対象_コンプレッサー.Image = global::EasySynth.Properties.Resources.edit_comp;
			this.編集対象_コンプレッサー.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_コンプレッサー.Name = "編集対象_コンプレッサー";
			this.編集対象_コンプレッサー.Size = new System.Drawing.Size(180, 30);
			this.編集対象_コンプレッサー.Text = "コンプレッサー";
			this.編集対象_コンプレッサー.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// toolStripSeparator4
			// 
			this.toolStripSeparator4.Name = "toolStripSeparator4";
			this.toolStripSeparator4.Size = new System.Drawing.Size(177, 6);
			// 
			// 編集対象_コーラス
			// 
			this.編集対象_コーラス.Image = global::EasySynth.Properties.Resources.edit_cho;
			this.編集対象_コーラス.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_コーラス.Name = "編集対象_コーラス";
			this.編集対象_コーラス.Size = new System.Drawing.Size(180, 30);
			this.編集対象_コーラス.Text = "コーラス";
			this.編集対象_コーラス.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// 編集対象_ディレイ
			// 
			this.編集対象_ディレイ.Image = global::EasySynth.Properties.Resources.edit_del;
			this.編集対象_ディレイ.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_ディレイ.Name = "編集対象_ディレイ";
			this.編集対象_ディレイ.Size = new System.Drawing.Size(180, 30);
			this.編集対象_ディレイ.Text = "ディレイ";
			this.編集対象_ディレイ.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// 編集対象_リバーブ
			// 
			this.編集対象_リバーブ.Image = global::EasySynth.Properties.Resources.edit_rev;
			this.編集対象_リバーブ.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_リバーブ.Name = "編集対象_リバーブ";
			this.編集対象_リバーブ.Size = new System.Drawing.Size(180, 30);
			this.編集対象_リバーブ.Text = "リバーブ";
			this.編集対象_リバーブ.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// toolStripSeparator5
			// 
			this.toolStripSeparator5.Name = "toolStripSeparator5";
			this.toolStripSeparator5.Size = new System.Drawing.Size(177, 6);
			// 
			// 編集対象_振幅EG
			// 
			this.編集対象_振幅EG.Image = global::EasySynth.Properties.Resources.edit_eg_amp;
			this.編集対象_振幅EG.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_振幅EG.Name = "編集対象_振幅EG";
			this.編集対象_振幅EG.Size = new System.Drawing.Size(180, 30);
			this.編集対象_振幅EG.Text = "振幅EG";
			this.編集対象_振幅EG.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// 編集対象_フィルターEG
			// 
			this.編集対象_フィルターEG.Image = global::EasySynth.Properties.Resources.edit_eg_lpf;
			this.編集対象_フィルターEG.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_フィルターEG.Name = "編集対象_フィルターEG";
			this.編集対象_フィルターEG.Size = new System.Drawing.Size(180, 30);
			this.編集対象_フィルターEG.Text = "フィルターEG";
			this.編集対象_フィルターEG.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// 編集対象_ピッチEG
			// 
			this.編集対象_ピッチEG.Image = global::EasySynth.Properties.Resources.edit_eg_pitch;
			this.編集対象_ピッチEG.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_ピッチEG.Name = "編集対象_ピッチEG";
			this.編集対象_ピッチEG.Size = new System.Drawing.Size(180, 30);
			this.編集対象_ピッチEG.Text = "ピッチEG";
			this.編集対象_ピッチEG.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// 編集対象_ビブラートLFO
			// 
			this.編集対象_ビブラートLFO.Image = global::EasySynth.Properties.Resources.edit_vib;
			this.編集対象_ビブラートLFO.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_ビブラートLFO.Name = "編集対象_ビブラートLFO";
			this.編集対象_ビブラートLFO.Size = new System.Drawing.Size(180, 30);
			this.編集対象_ビブラートLFO.Text = "ビブラートLFO";
			this.編集対象_ビブラートLFO.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// toolStripSeparator11
			// 
			this.toolStripSeparator11.Name = "toolStripSeparator11";
			this.toolStripSeparator11.Size = new System.Drawing.Size(177, 6);
			// 
			// 編集対象_テンポ
			// 
			this.編集対象_テンポ.Image = global::EasySynth.Properties.Resources.edit_tempo;
			this.編集対象_テンポ.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.編集対象_テンポ.Name = "編集対象_テンポ";
			this.編集対象_テンポ.Size = new System.Drawing.Size(180, 30);
			this.編集対象_テンポ.Text = "テンポ";
			this.編集対象_テンポ.Click += new System.EventHandler(this.編集対象_Click);
			// 
			// 時間単位
			// 
			this.時間単位.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.時間単位_フリーハンド,
            this.toolStripSeparator6,
            this.時間単位_8分,
            this.時間単位_16分,
            this.時間単位_32分,
            this.時間単位_64分,
            this.toolStripSeparator7,
            this.時間単位_8分3連,
            this.時間単位_16分3連,
            this.時間単位_32分3連,
            this.時間単位_64分3連,
            this.toolStripSeparator8,
            this.時間単位_8分5連,
            this.時間単位_16分5連,
            this.時間単位_32分5連,
            this.時間単位_64分5連});
			this.時間単位.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位.Name = "時間単位";
			this.時間単位.Size = new System.Drawing.Size(83, 20);
			this.時間単位.Text = "時間単位(U)";
			// 
			// 時間単位_フリーハンド
			// 
			this.時間単位_フリーハンド.Image = global::EasySynth.Properties.Resources.freehand;
			this.時間単位_フリーハンド.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_フリーハンド.Name = "時間単位_フリーハンド";
			this.時間単位_フリーハンド.Size = new System.Drawing.Size(132, 30);
			this.時間単位_フリーハンド.Text = "フリーハンド";
			this.時間単位_フリーハンド.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// toolStripSeparator6
			// 
			this.toolStripSeparator6.Name = "toolStripSeparator6";
			this.toolStripSeparator6.Size = new System.Drawing.Size(129, 6);
			// 
			// 時間単位_8分
			// 
			this.時間単位_8分.Image = global::EasySynth.Properties.Resources.timeunit_1_8;
			this.時間単位_8分.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_8分.Name = "時間単位_8分";
			this.時間単位_8分.Size = new System.Drawing.Size(132, 30);
			this.時間単位_8分.Text = "8分";
			this.時間単位_8分.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// 時間単位_16分
			// 
			this.時間単位_16分.Image = global::EasySynth.Properties.Resources.timeunit_1_16;
			this.時間単位_16分.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_16分.Name = "時間単位_16分";
			this.時間単位_16分.Size = new System.Drawing.Size(132, 30);
			this.時間単位_16分.Text = "16分";
			this.時間単位_16分.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// 時間単位_32分
			// 
			this.時間単位_32分.Image = global::EasySynth.Properties.Resources.timeunit_1_32;
			this.時間単位_32分.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_32分.Name = "時間単位_32分";
			this.時間単位_32分.Size = new System.Drawing.Size(132, 30);
			this.時間単位_32分.Text = "32分";
			this.時間単位_32分.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// 時間単位_64分
			// 
			this.時間単位_64分.Image = global::EasySynth.Properties.Resources.timeunit_1_64;
			this.時間単位_64分.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_64分.Name = "時間単位_64分";
			this.時間単位_64分.Size = new System.Drawing.Size(132, 30);
			this.時間単位_64分.Text = "64分";
			this.時間単位_64分.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// toolStripSeparator7
			// 
			this.toolStripSeparator7.Name = "toolStripSeparator7";
			this.toolStripSeparator7.Size = new System.Drawing.Size(129, 6);
			// 
			// 時間単位_8分3連
			// 
			this.時間単位_8分3連.Image = global::EasySynth.Properties.Resources.timeunit_3_8;
			this.時間単位_8分3連.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_8分3連.Name = "時間単位_8分3連";
			this.時間単位_8分3連.Size = new System.Drawing.Size(132, 30);
			this.時間単位_8分3連.Text = "8分3連";
			this.時間単位_8分3連.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// 時間単位_16分3連
			// 
			this.時間単位_16分3連.Image = global::EasySynth.Properties.Resources.timeunit_3_16;
			this.時間単位_16分3連.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_16分3連.Name = "時間単位_16分3連";
			this.時間単位_16分3連.Size = new System.Drawing.Size(132, 30);
			this.時間単位_16分3連.Text = "16分3連";
			this.時間単位_16分3連.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// 時間単位_32分3連
			// 
			this.時間単位_32分3連.Image = global::EasySynth.Properties.Resources.timeunit_3_32;
			this.時間単位_32分3連.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_32分3連.Name = "時間単位_32分3連";
			this.時間単位_32分3連.Size = new System.Drawing.Size(132, 30);
			this.時間単位_32分3連.Text = "32分3連";
			this.時間単位_32分3連.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// 時間単位_64分3連
			// 
			this.時間単位_64分3連.Image = global::EasySynth.Properties.Resources.timeunit_3_64;
			this.時間単位_64分3連.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_64分3連.Name = "時間単位_64分3連";
			this.時間単位_64分3連.Size = new System.Drawing.Size(132, 30);
			this.時間単位_64分3連.Text = "64分3連";
			this.時間単位_64分3連.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// toolStripSeparator8
			// 
			this.toolStripSeparator8.Name = "toolStripSeparator8";
			this.toolStripSeparator8.Size = new System.Drawing.Size(129, 6);
			// 
			// 時間単位_8分5連
			// 
			this.時間単位_8分5連.Image = global::EasySynth.Properties.Resources.timeunit_5_8;
			this.時間単位_8分5連.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_8分5連.Name = "時間単位_8分5連";
			this.時間単位_8分5連.Size = new System.Drawing.Size(132, 30);
			this.時間単位_8分5連.Text = "8分5連";
			this.時間単位_8分5連.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// 時間単位_16分5連
			// 
			this.時間単位_16分5連.Image = global::EasySynth.Properties.Resources.timeunit_5_16;
			this.時間単位_16分5連.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_16分5連.Name = "時間単位_16分5連";
			this.時間単位_16分5連.Size = new System.Drawing.Size(132, 30);
			this.時間単位_16分5連.Text = "16分5連";
			this.時間単位_16分5連.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// 時間単位_32分5連
			// 
			this.時間単位_32分5連.Image = global::EasySynth.Properties.Resources.timeunit_5_32;
			this.時間単位_32分5連.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_32分5連.Name = "時間単位_32分5連";
			this.時間単位_32分5連.Size = new System.Drawing.Size(132, 30);
			this.時間単位_32分5連.Text = "32分5連";
			this.時間単位_32分5連.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// 時間単位_64分5連
			// 
			this.時間単位_64分5連.Image = global::EasySynth.Properties.Resources.timeunit_5_64;
			this.時間単位_64分5連.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.時間単位_64分5連.Name = "時間単位_64分5連";
			this.時間単位_64分5連.Size = new System.Drawing.Size(132, 30);
			this.時間単位_64分5連.Text = "64分5連";
			this.時間単位_64分5連.Click += new System.EventHandler(this.時間単位_Click);
			// 
			// 表示
			// 
			this.表示.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.表示_次の小節へ移動,
            this.表示_前の小節へ移動,
            this.toolStripSeparator9,
            this.表示_時間方向拡大,
            this.表示_時間方向縮小,
            this.toolStripSeparator10,
            this.表示_音高方向拡大,
            this.表示_音高方向縮小});
			this.表示.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.表示.Name = "表示";
			this.表示.Size = new System.Drawing.Size(59, 20);
			this.表示.Text = "表示(D)";
			// 
			// 表示_次の小節へ移動
			// 
			this.表示_次の小節へ移動.Image = global::EasySynth.Properties.Resources.scroll_next;
			this.表示_次の小節へ移動.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.表示_次の小節へ移動.Name = "表示_次の小節へ移動";
			this.表示_次の小節へ移動.ShortcutKeyDisplayString = "Right";
			this.表示_次の小節へ移動.Size = new System.Drawing.Size(218, 30);
			this.表示_次の小節へ移動.Text = "次の小節へ移動";
			this.表示_次の小節へ移動.Click += new System.EventHandler(this.表示_Click);
			// 
			// 表示_前の小節へ移動
			// 
			this.表示_前の小節へ移動.Image = global::EasySynth.Properties.Resources.scroll_prev;
			this.表示_前の小節へ移動.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.表示_前の小節へ移動.Name = "表示_前の小節へ移動";
			this.表示_前の小節へ移動.ShortcutKeyDisplayString = "Left";
			this.表示_前の小節へ移動.Size = new System.Drawing.Size(218, 30);
			this.表示_前の小節へ移動.Text = "前の小節へ移動";
			this.表示_前の小節へ移動.Click += new System.EventHandler(this.表示_Click);
			// 
			// toolStripSeparator9
			// 
			this.toolStripSeparator9.Name = "toolStripSeparator9";
			this.toolStripSeparator9.Size = new System.Drawing.Size(215, 6);
			// 
			// 表示_時間方向拡大
			// 
			this.表示_時間方向拡大.Image = global::EasySynth.Properties.Resources.time_zoom;
			this.表示_時間方向拡大.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.表示_時間方向拡大.Name = "表示_時間方向拡大";
			this.表示_時間方向拡大.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Right)));
			this.表示_時間方向拡大.Size = new System.Drawing.Size(218, 30);
			this.表示_時間方向拡大.Text = "時間方向拡大";
			this.表示_時間方向拡大.Click += new System.EventHandler(this.表示_Click);
			// 
			// 表示_時間方向縮小
			// 
			this.表示_時間方向縮小.Image = global::EasySynth.Properties.Resources.time_zoomout;
			this.表示_時間方向縮小.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.表示_時間方向縮小.Name = "表示_時間方向縮小";
			this.表示_時間方向縮小.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Left)));
			this.表示_時間方向縮小.Size = new System.Drawing.Size(218, 30);
			this.表示_時間方向縮小.Text = "時間方向縮小";
			this.表示_時間方向縮小.Click += new System.EventHandler(this.表示_Click);
			// 
			// toolStripSeparator10
			// 
			this.toolStripSeparator10.Name = "toolStripSeparator10";
			this.toolStripSeparator10.Size = new System.Drawing.Size(215, 6);
			// 
			// 表示_音高方向拡大
			// 
			this.表示_音高方向拡大.Image = global::EasySynth.Properties.Resources.tone_zoom;
			this.表示_音高方向拡大.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.表示_音高方向拡大.Name = "表示_音高方向拡大";
			this.表示_音高方向拡大.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Up)));
			this.表示_音高方向拡大.Size = new System.Drawing.Size(218, 30);
			this.表示_音高方向拡大.Text = "音高方向拡大";
			this.表示_音高方向拡大.Click += new System.EventHandler(this.表示_Click);
			// 
			// 表示_音高方向縮小
			// 
			this.表示_音高方向縮小.Image = global::EasySynth.Properties.Resources.tone_zoomout;
			this.表示_音高方向縮小.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.表示_音高方向縮小.Name = "表示_音高方向縮小";
			this.表示_音高方向縮小.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Down)));
			this.表示_音高方向縮小.Size = new System.Drawing.Size(218, 30);
			this.表示_音高方向縮小.Text = "音高方向縮小";
			this.表示_音高方向縮小.Click += new System.EventHandler(this.表示_Click);
			// 
			// MenuStrip1
			// 
			this.MenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.編集,
            this.編集対象,
            this.時間単位,
            this.表示});
			this.MenuStrip1.Location = new System.Drawing.Point(0, 0);
			this.MenuStrip1.Name = "MenuStrip1";
			this.MenuStrip1.ShowItemToolTips = true;
			this.MenuStrip1.Size = new System.Drawing.Size(800, 24);
			this.MenuStrip1.TabIndex = 1;
			this.MenuStrip1.Text = "menuStrip1";
			// 
			// PianoRoll
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(800, 450);
			this.Controls.Add(this.PicInput);
			this.Controls.Add(this.PicNote);
			this.Controls.Add(this.Hsb);
			this.Controls.Add(this.Vsb);
			this.Controls.Add(this.MenuStrip1);
			this.KeyPreview = true;
			this.MainMenuStrip = this.MenuStrip1;
			this.MinimumSize = new System.Drawing.Size(128, 128);
			this.Name = "PianoRoll";
			this.Text = "Form1";
			this.SizeChanged += new System.EventHandler(this.PianoRoll_SizeChanged);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PianoRoll_KeyDown);
			((System.ComponentModel.ISupportInitialize)(this.PicNote)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.PicInput)).EndInit();
			this.MenuStrip1.ResumeLayout(false);
			this.MenuStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Timer Timer1;
		private System.Windows.Forms.PictureBox PicNote;
		private System.Windows.Forms.PictureBox PicInput;
		private System.Windows.Forms.VScrollBar Vsb;
		private System.Windows.Forms.HScrollBar Hsb;
		private System.Windows.Forms.ToolStripMenuItem 編集;
		private System.Windows.Forms.ToolStripMenuItem 編集_書き込み;
		private System.Windows.Forms.ToolStripMenuItem 編集_1トラック選択;
		private System.Windows.Forms.ToolStripMenuItem 編集_複数トラック選択;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
		private System.Windows.Forms.ToolStripMenuItem 編集_元に戻す;
		private System.Windows.Forms.ToolStripMenuItem 編集_やり直す;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
		private System.Windows.Forms.ToolStripMenuItem 編集_切り取り;
		private System.Windows.Forms.ToolStripMenuItem 編集_コピー;
		private System.Windows.Forms.ToolStripMenuItem 編集_貼り付け;
		private System.Windows.Forms.ToolStripMenuItem 編集_削除;
		private System.Windows.Forms.ToolStripMenuItem 編集対象;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_音符;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_アクセント;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_強弱;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_ピッチ;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_ビブラートの深さ;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_音色;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_音量;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_定位;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_コンプレッサー;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_コーラス;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_ディレイ;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_リバーブ;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_振幅EG;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_フィルターEG;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_ピッチEG;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_ビブラートLFO;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
		private System.Windows.Forms.ToolStripMenuItem 編集対象_テンポ;
		private System.Windows.Forms.ToolStripMenuItem 時間単位;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_フリーハンド;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_8分;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_16分;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_32分;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_64分;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_8分3連;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_16分3連;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_32分3連;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_64分3連;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_8分5連;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_16分5連;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_32分5連;
		private System.Windows.Forms.ToolStripMenuItem 時間単位_64分5連;
		private System.Windows.Forms.ToolStripMenuItem 表示;
		private System.Windows.Forms.ToolStripMenuItem 表示_次の小節へ移動;
		private System.Windows.Forms.ToolStripMenuItem 表示_前の小節へ移動;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
		private System.Windows.Forms.ToolStripMenuItem 表示_時間方向拡大;
		private System.Windows.Forms.ToolStripMenuItem 表示_時間方向縮小;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
		private System.Windows.Forms.ToolStripMenuItem 表示_音高方向拡大;
		private System.Windows.Forms.ToolStripMenuItem 表示_音高方向縮小;
		private System.Windows.Forms.MenuStrip MenuStrip1;
	}
}

