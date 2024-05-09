using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasySynth.Forms
{
	public partial class Spectrum : Form
	{
		const double BaseFreq = 16.3516;
		const int BankCount = 126;
		const int ValueStep = 5;
		const int ValueMax = 5;
		const int ValueMin = -60;
		const int MarginLeft = 26;
		const int MarginBottom = 19;
		const int TrackAreaWidth = 320;

		static readonly int[] GRID_FREQ = new int[] {
			20, 30, 40, 50, 60, 70, 80, 90,
			100, 200, 300, 400, 500, 600, 700, 800, 900,
			1000, 2000, 3000, 4000, 5000, 6000, 7000, 8000, 9000,
			10000, 20000
		};

		static readonly Rectangle BtnNotePos = new Rectangle(0, 0, 24, 24);

		static readonly Font FText = new Font("Segoe UI", 9.0f);
		static readonly Brush BText = new Pen(Color.FromArgb(191, 191, 191), 1).Brush;

		static readonly Pen PBorder = new Pen(Color.FromArgb(191, 0, 0), 1);
		static readonly Pen PBorderOct = new Pen(Color.FromArgb(111, 111, 111), 1);
		static readonly Pen PBorderKey = new Pen(Color.FromArgb(83, 83, 83), 1);
		static readonly Brush BWhiteKey = new Pen(Color.FromArgb(63, 63, 63), 1).Brush;

		static readonly Color CGrid = Color.FromArgb(127, 0, 0);
		static readonly Pen PGrid = new Pen(CGrid, 1);
		static readonly Pen PGridDash = new Pen(CGrid, 1)
		{
			DashPattern = new float[] { 2, 4 }
		};
		static readonly StringFormat FmtFreq = new StringFormat()
		{
			Alignment = StringAlignment.Center,
			LineAlignment = StringAlignment.Near
		};
		static readonly StringFormat FmtDb = new StringFormat()
		{
			Alignment = StringAlignment.Far,
			LineAlignment = StringAlignment.Center
		};

		Rectangle mBtnNoteArea;
		Rectangle mTrackArea;

		float mHalfToneWidth;
		float mGridHeight;
		float mGridRight;
		bool mDispNote;
		bool mDispCtrl;

		public Spectrum()
		{
			InitializeComponent();
			MinimumSize = new Size(280 + TrackAreaWidth, 256);
			Width = MinimumSize.Width;
			Height = MinimumSize.Height;
			SetSize();
			timer1.Interval = 16;
			timer1.Enabled = true;
			timer1.Start();
		}

		private void Spectrum_SizeChanged(object sender, EventArgs e)
		{
			SetSize();
		}

		private void pictureBox1_Click(object s, EventArgs e)
		{
			var pos = ((PictureBox)s).PointToClient(MousePosition);
			if (mBtnNoteArea.Contains(pos))
			{
				mDispNote = !mDispNote;
				SetGrid();
				return;
			}
			if (mTrackArea.Width == 0 || mTrackArea.Contains(pos))
			{
				mDispCtrl = !mDispCtrl;
				SetSize();
			}
		}

		private void timer1_Tick(object sender, EventArgs e)
		{
			var g = Graphics.FromImage(pictureBox1.Image);
			g.Clear(Color.Transparent);

			var sp = Playback.GetSpectrum();
			if (null == sp)
			{
				return;
			}

			pictureBox1.Image = pictureBox1.Image;
		}

		void SetSize()
		{
			var width = Width - 16;
			var height = Height - 39;
			if (width < 1)
			{
				width = 1;
			}
			if (height < 1)
			{
				height = 1;
			}

			if (null != pictureBox1.Image)
			{
				pictureBox1.Image.Dispose();
				pictureBox1.Image = null;
			}
			if (null != pictureBox1.BackgroundImage)
			{
				pictureBox1.BackgroundImage.Dispose();
				pictureBox1.BackgroundImage = null;
			}

			pictureBox1.Width = width;
			pictureBox1.Height = height;
			pictureBox1.Image = new Bitmap(width, height);
			pictureBox1.BackgroundImage = new Bitmap(width, height);

			SetGrid();
		}

		void SetGrid()
		{
			var g = Graphics.FromImage(pictureBox1.BackgroundImage);
			g.Clear(Color.Black);

			var width = pictureBox1.Width;
			var height = pictureBox1.Height;
			var gh = height / 2 - 3;
			mGridHeight = gh - MarginBottom;

			if (mDispCtrl)
			{
				mTrackArea = new Rectangle(
					width - TrackAreaWidth,
					BtnNotePos.Height,
					TrackAreaWidth,
					height - BtnNotePos.Height
				);
				mBtnNoteArea = new Rectangle(
					BtnNotePos.X + width - TrackAreaWidth,
					BtnNotePos.Y,
					BtnNotePos.Width,
					BtnNotePos.Height
				);
				mHalfToneWidth = (float)(width - MarginLeft - TrackAreaWidth) / BankCount;
				mGridRight = width - TrackAreaWidth;
			}
			else
			{
				mTrackArea = new Rectangle(0, 0, 0, 0);
				mBtnNoteArea = new Rectangle(
					BtnNotePos.X + width - BtnNotePos.Width,
					BtnNotePos.Y,
					BtnNotePos.Width,
					BtnNotePos.Height
				);
				mHalfToneWidth = (float)(width - MarginLeft - BtnNotePos.Width) / BankCount;
				mGridRight = width - BtnNotePos.Width;
			}

			if (mDispNote)
			{
				SetNoteGrid(g, 1);
				SetNoteGrid(g, gh + 7);
			}
			else
			{
				SetLogGrid(g, 1);
				SetLogGrid(g, gh + 7);
			}

			pictureBox1.Image = pictureBox1.Image;
		}

		void DrawDb(Graphics g, float y)
		{
			var dv = ValueMax - ValueMin;
			var dy = mGridHeight / dv;
			for (int i = ValueStep; i <= dv; i += ValueStep)
			{
				var db = ValueMax - i;
				if (db % (ValueStep * 2) == 0)
				{
					var py = y + i * dy;
					g.DrawLine(PGrid, MarginLeft, py, mGridRight, py);
					g.DrawLine(PBorder, MarginLeft - 3, py, MarginLeft, py);
					g.DrawString(db.ToString(), FText, BText, new RectangleF(-5, py - 9.5f, 28, 20), FmtDb);
				}
			}
		}

		void DrawGridBorder(Graphics g, float y)
		{
			var bottom = y + mGridHeight;
			g.DrawLine(PBorder, MarginLeft, y, mGridRight, y);
			g.DrawLine(PBorder, MarginLeft, bottom, mGridRight, bottom);
			g.DrawLine(PBorder, MarginLeft, y, MarginLeft, bottom);
			g.DrawLine(PBorder, mGridRight, y, mGridRight, bottom);
		}

		void SetLogGrid(Graphics g, float y)
		{
			/* DrawFreq */
			var bottom = y + mGridHeight;
			foreach (var gridFreq in GRID_FREQ)
			{
				var gridHalfTone = 12 * Math.Log10(gridFreq / BaseFreq) / Math.Log10(2);
				var gridX = MarginLeft + (float)(mHalfToneWidth * (gridHalfTone + 0.5));
				var l10 = Math.Log10(gridFreq);
				var l5 = Math.Log10(2 * gridFreq);
				var l2 = Math.Log10(5 * gridFreq);
				if (0 == l10 - (int)l10)
				{
					g.DrawLine(PGrid, gridX, y, gridX, bottom);
				}
				else
				{
					g.DrawLine(PGridDash, gridX, y, gridX, bottom);
				}
				if (0 == l10 - (int)l10 || 0 == l5 - (int)l5 || 0 == l2 - (int)l2)
				{
					string txt;
					if (gridFreq >= 1000)
					{
						txt = gridFreq / 1000 + "k";
					}
					else
					{
						txt = gridFreq + "";
					}
					g.DrawLine(PBorder, gridX, bottom, gridX, bottom + 6);
					g.DrawString(txt, FText, BText, gridX, bottom + 4, FmtFreq);
				}
			}
			DrawDb(g, y);
			DrawGridBorder(g, y);
		}

		void SetNoteGrid(Graphics g, float y)
		{
			var bottom = y + mGridHeight;

			/* DrawKeyborad */
			if (mHalfToneWidth < 3)
			{
				for (int i = 0; i < BankCount; i += 12)
				{
					var gridX = MarginLeft + (float)(mHalfToneWidth * (i + 0.5));
					g.DrawLine(PGrid, gridX, y, gridX, bottom);
				}
			}
			else
			{
				for (int i = 0; i < BankCount; i++)
				{
					var gridX = MarginLeft + (float)(mHalfToneWidth * i);
					switch (i % 12)
					{
						case 0:
							g.FillRectangle(BWhiteKey, new RectangleF(gridX, y, mHalfToneWidth, mGridHeight));
							g.DrawLine(PBorderOct, gridX, y, gridX, bottom);
							break;
						case 5:
							g.FillRectangle(BWhiteKey, new RectangleF(gridX, y, mHalfToneWidth, mGridHeight));
							g.DrawLine(PBorderKey, gridX, y, gridX, bottom);
							break;
						case 1:
						case 3:
						case 6:
						case 8:
						case 10:
							break;
						default:
							g.FillRectangle(BWhiteKey, new RectangleF(gridX, y, mHalfToneWidth, mGridHeight));
							break;
					}
				}
			}

			/* DrawOctNo */
			for (int i = 0; i < BankCount; i += 12)
			{
				var gridX = MarginLeft + (float)(mHalfToneWidth * (i + 0.5));
				var txt = "C" + (i / 12);
				g.DrawLine(PBorder, gridX, bottom, gridX, bottom + 6);
				g.DrawString(txt, FText, BText, gridX, bottom + 4, FmtFreq);
			}

			DrawDb(g, y);
			DrawGridBorder(g, y);
		}
	}
}
