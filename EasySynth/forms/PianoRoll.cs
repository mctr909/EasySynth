using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace EasySynth.Forms
{
	public partial class PianoRoll : Form
	{
		static readonly Font FontOctName = new Font("MS Gothic", 9.0f);
		static readonly StringFormat FormatOct = new StringFormat()
		{
			Alignment = StringAlignment.Far,
			LineAlignment = StringAlignment.Center
		};
		static readonly Color CtrlBackground = Color.FromArgb(211, 255, 255, 255);

		static Pen MeasureBorder;
		static Pen BeatBorder;
		static Pen OctBorder;
		static Pen KeyBorder;

		static Brush WhiteKey;
		static Brush BlackKey;
		static Pen BlackKeyBorderT;
		static Pen BlackKeyBorderB;
		static Brush EditableNote;
		static Pen EditableNoteBorderT;
		static Pen EditableNoteBorderB;
		static Brush GrippedNote;
		static Pen GrippedNoteBorderT;
		static Pen GrippedNoteBorderB;
		static Brush SelectedNote;
		static Pen SelectedNoteBorderT;
		static Pen SelectedNoteBorderB;
		static Brush ReadonlyNote;
		static Pen ReadonlyNoteBorderT;
		static Pen ReadonlyNoteBorderB;

		bool mChangeSize = true;
		bool mChangeEditTarget = false;
		bool mScrollTone = true;
		bool mUpdateScreen = false;
		bool mDragging = false;
		bool mDragMove = false;
		bool mDragEnd = false;

		int mDisplayTones;
		int mDisplayMaxTone;
		int mDisplayMinTone;
		int mDisplayMaxTick;
		int mDisplayMinTick;

		int mQuarterNoteWidth = 80;
		int mKeyHeight = 11;
		int mCenterTone = 60;
		int mCursorTone;
		Point mCursor;
		Point mDragStartPos;

		List<Ev> mEventList = new List<Ev>();

		class Ev
		{
			public int Begin;
			public int End;
			public int Tone;
		}

		static PianoRoll()
		{
			SetColor();
		}

		public PianoRoll()
		{
			InitializeComponent();
			編集_Click(編集_書き込み, null);
			編集対象_Click(編集対象_音符, null);
			時間単位_Click(時間単位_16分, null);
			PicNote.Controls.Add(PicInput);
			Timer1.Enabled = true;
			Timer1.Interval = 16;
			Timer1.Start();
		}

		public static void SetColor()
		{
			var cMeasureBorder = Color.FromArgb(0, 0, 0);
			var cBeatBorder = Color.FromArgb(0, 0, 0);
			var cOctBorder = Color.FromArgb(0, 0, 0);

			var cWhiteKey = Color.FromArgb(245, 245, 245);
			var cBlackKey = Color.FromArgb(127, 127, 127);
			var cEditableNote = Color.FromArgb(0, 191, 0);
			var cSelectedNote = Color.FromArgb(211, 95, 95);
			var cReadonlyNote = Color.FromArgb(0, 171, 211);

			MeasureBorder = new Pen(cMeasureBorder);
			BeatBorder = new Pen(cBeatBorder);
			OctBorder = new Pen(cOctBorder);
			KeyBorder = new Pen(SetLA(cWhiteKey, 0.66, 1));

			WhiteKey = new Pen(cWhiteKey).Brush;
			BlackKey = new Pen(cBlackKey).Brush;
			BlackKeyBorderT = new Pen(SetLA(cWhiteKey, 0.5, 1));
			BlackKeyBorderB = new Pen(SetLA(cBlackKey, 0.75, 1));
			EditableNote = new Pen(cEditableNote).Brush;
			EditableNoteBorderT = new Pen(SetLA(cEditableNote, 2.2, 1));
			EditableNoteBorderB = new Pen(SetLA(cEditableNote, 0.5, 1));
			GrippedNote = new Pen(SetLA(cEditableNote, 2.0, 0.5)).Brush;
			GrippedNoteBorderT = new Pen(SetLA(cEditableNote, 2.0, 0.5));
			GrippedNoteBorderB = new Pen(SetLA(cEditableNote, 0.5, 0.5));
			SelectedNote = new Pen(cSelectedNote).Brush;
			SelectedNoteBorderT = new Pen(SetLA(cSelectedNote, 1.33, 1));
			SelectedNoteBorderB = new Pen(SetLA(cSelectedNote, 0.5, 1));
			ReadonlyNote = new Pen(cReadonlyNote).Brush;
			ReadonlyNoteBorderT = new Pen(SetLA(cReadonlyNote, 1.33, 1));
			ReadonlyNoteBorderB = new Pen(SetLA(cReadonlyNote, 0.5, 1));
		}

		public static Color SetLA(Color color, double l, double a)
		{
			var r = (double)color.R;
			var g = (double)color.G;
			var b = (double)color.B;
			var x = (r * 2 - g - b) / 3;
			var y = (g - b) / Math.Sqrt(3);
			var h = Math.Atan2(y, x);
			var s = Math.Sqrt(x * x + y * y) * 2 / 3;
			l *= (r + g + b) / 3;
			r = l + s * Math.Cos(h);
			g = l - s * Math.Cos(h + Math.PI / 3);
			b = l - s * Math.Cos(h - Math.PI / 3);
			r = Math.Min(255, Math.Max(0, r));
			g = Math.Min(255, Math.Max(0, g));
			b = Math.Min(255, Math.Max(0, b));
			return Color.FromArgb((byte)(255 * a), (byte)r, (byte)g, (byte)b);
		}

		#region メニューイベント
		private void 編集_Click(object sender, EventArgs e)
		{
			SetMenuChecked(sender, 編集);
		}

		private void 編集対象_Click(object sender, EventArgs e)
		{
			SetMenuChecked(sender, 編集対象);
		}

		private void 時間単位_Click(object sender, EventArgs e)
		{
			SetMenuChecked(sender, 時間単位);
		}

		private void 表示_Click(object sender, EventArgs e)
		{
			if (表示_次の小節へ移動 == sender)
			{
				mUpdateScreen = true;
			}
			if (表示_前の小節へ移動 == sender)
			{
				mUpdateScreen = true;
			}
			if (表示_時間方向拡大 == sender)
			{
				mUpdateScreen = true;
			}
			if (表示_時間方向縮小 == sender)
			{
				mUpdateScreen = true;
			}
			if (表示_音高方向拡大 == sender)
			{
				mKeyHeight++;
				mScrollTone = true;
				if (mKeyHeight >= 24)
				{
					表示_音高方向拡大.Enabled = false;
				}
				表示_音高方向縮小.Enabled = true;
			}
			if (表示_音高方向縮小 == sender)
			{
				mKeyHeight--;
				mScrollTone = true;
				if (mKeyHeight <= 6)
				{
					表示_音高方向縮小.Enabled = false;
				}
				表示_音高方向拡大.Enabled = true;
			}
		}

		void SetMenuChecked(object sender, ToolStripMenuItem parent)
		{
			if (sender is ToolStripMenuItem s && null == s.Image)
			{
				return;
			}
			for (int i = 0; i < parent.DropDownItems.Count; i++)
			{
				if (parent.DropDownItems[i] is ToolStripMenuItem item)
				{
					if (sender == item)
					{
						if (!item.Checked)
						{
							parent.Image = item.Image;
							parent.ToolTipText = item.Text;
							item.Checked = true;
							mChangeEditTarget = true;
						}
					}
					else
					{
						item.Checked = false;
					}
				}
			}
		}
		#endregion

		#region フォームコントロールイベント
		private void PianoRoll_SizeChanged(object sender, EventArgs e)
		{
			mChangeSize = true;
		}

		private void Vsb_Scroll(object sender, ScrollEventArgs e)
		{
			mCenterTone = 128 - Vsb.Value;
			mScrollTone = true;
		}

		private void Hsb_Scroll(object sender, ScrollEventArgs e)
		{
			mUpdateScreen = true;
		}

		private void PicInput_MouseDown(object sender, MouseEventArgs e)
		{
			SetCursor(sender);
			mDragStartPos = mCursor;
			mDragging = true;
		}

		private void PicInput_MouseUp(object sender, MouseEventArgs e)
		{
			SetCursor(sender);
			mDragging = false;
			mDragEnd = true;
		}

		private void PicInput_MouseMove(object sender, MouseEventArgs e)
		{
			SetCursor(sender);
			mDragMove = true;
		}

		void SetCursor(object sender)
		{
			mCursor = ((PictureBox)sender).PointToClient(MousePosition);
			var tone = mCenterTone - (mCursor.Y - PicNote.Height / 2.0) / mKeyHeight;
			tone = (int)(Math.Sign(tone) + tone);
			mCursorTone = (int)Math.Max(0, Math.Min(127, tone));
		}
		#endregion

		private void Timer1_Tick(object sender, EventArgs e)
		{
			if (mChangeSize)
			{
				ChangeSize();
				DrawNoteBackground();
				DrawInputBackground();
				DrawNotes();
				mChangeSize = false;
			}

			if (mScrollTone)
			{
				DrawNoteBackground();
				DrawNotes();
				mScrollTone = false;
			}

			if (mChangeEditTarget)
			{
				DrawInputBackground();
				mChangeEditTarget = false;
			}

			if (mUpdateScreen)
			{
				DrawNotes();
				mUpdateScreen = false;
			}

			if (!mDragging && mDragMove)
			{
				if (編集対象_音符.Checked)
				{
					SelectNote();
				}
				mDragMove = false;
			}

			if ((mDragging && mDragMove) || mDragEnd)
			{
				if (編集対象_音符.Checked)
				{
					EditNote();
				}
				mDragMove = false;
				mDragEnd = false;
			}
		}

		void ChangeSize()
		{
			if (null != PicNote.Image)
			{
				PicNote.Image.Dispose();
				PicNote.Image = null;
			}
			if (null != PicNote.BackgroundImage)
			{
				PicNote.BackgroundImage.Dispose();
				PicNote.BackgroundImage = null;
			}

			if (null != PicInput.Image)
			{
				PicInput.Image.Dispose();
				PicInput.Image = null;
			}
			if (null != PicInput.BackgroundImage)
			{
				PicInput.BackgroundImage.Dispose();
				PicInput.BackgroundImage = null;
			}

			PicNote.Left = 0;
			PicNote.Top = MenuStrip1.Bottom;
			PicNote.Width = Width - Vsb.Width - 16;
			PicNote.Height = Height - MenuStrip1.Height - Hsb.Height - 39;

			PicInput.Left = 0;
			PicInput.Top = 0;
			PicInput.Width = PicNote.Width;
			PicInput.Height = PicNote.Height;

			Hsb.Left = 0;
			Hsb.Top = PicNote.Bottom;
			Hsb.Width = PicNote.Width;
			Vsb.Left = PicNote.Right;
			Vsb.Top = PicNote.Top;
			Vsb.Height = PicNote.Height;

			PicNote.Image = new Bitmap(PicNote.Width, PicNote.Height);
			PicNote.BackgroundImage = new Bitmap(PicNote.Width, PicNote.Height);
			PicInput.Image = new Bitmap(PicInput.Width, PicInput.Height);
			PicInput.BackgroundImage = new Bitmap(PicInput.Width, PicInput.Height);
		}

		void SelectNote()
		{

		}

		void EditNote()
		{
			var g = Graphics.FromImage(PicInput.Image);
			g.Clear(Color.Transparent);

			if (mCursorTone <= mDisplayMinTone)
			{
				Vsb.Value = Math.Min(127, Vsb.Value + 1);
				mCenterTone = 128 - Vsb.Value;
				DrawNoteBackground();
			}
			if (mCursorTone > mDisplayMaxTone)
			{
				Vsb.Value = Math.Max(0, Vsb.Value - 1);
				mCenterTone = 128 - Vsb.Value;
				DrawNoteBackground();
			}

			var ax = Math.Min(mDragStartPos.X, mCursor.X);
			var bx = Math.Max(mDragStartPos.X, mCursor.X);

			if (mDragEnd)
			{
				mEventList.Add(new Ev()
				{
					Begin = ax,
					End = bx,
					Tone = mCursorTone
				});
				mUpdateScreen = true;
				return;
			}

			DrawSelectedNote(g, ax, bx, mCursorTone);

			PicInput.Image = PicInput.Image;
		}

		#region 描画メソッド
		void DrawNoteBackground()
		{
			mDisplayTones = PicNote.Height / mKeyHeight;
			mDisplayMinTone = Math.Max(0, mCenterTone - mDisplayTones / 2);
			mDisplayMaxTone = Math.Min(127, mCenterTone + mDisplayTones / 2);

			var g = Graphics.FromImage(PicNote.BackgroundImage);
			g.Clear(Color.Transparent);
			for (int tone = mDisplayMinTone; tone <= mDisplayMaxTone; tone++)
			{
				var top = PicNote.Height / 2 - mKeyHeight * (tone - mCenterTone);
				var bottom = top + mKeyHeight - 1;
				switch (tone % 12)
				{
					case 0:
						g.FillRectangle(WhiteKey, 0, top, PicNote.Width, mKeyHeight);
						g.DrawLine(OctBorder, 0, bottom, PicNote.Width, bottom);
						g.DrawString("C" + tone / 12, FontOctName, Brushes.Black, new Rectangle(0, top, 25, mKeyHeight), FormatOct);
						break;
					case 5:
						g.FillRectangle(WhiteKey, 0, top, PicNote.Width, mKeyHeight);
						g.DrawLine(KeyBorder, 0, bottom, PicNote.Width, bottom);
						break;
					case 2:
					case 4:
					case 7:
					case 9:
					case 11:
						g.FillRectangle(WhiteKey, 0, top, PicNote.Width, mKeyHeight);
						break;
					default:
						g.FillRectangle(BlackKey, 0, top, PicNote.Width, mKeyHeight);
						g.DrawLine(BlackKeyBorderT, 0, top, PicNote.Width, top);
						g.DrawLine(BlackKeyBorderB, 0, bottom, PicNote.Width, bottom);
						break;
				}
			}
			PicNote.Image = PicNote.Image;
		}

		void DrawInputBackground()
		{
			var g = Graphics.FromImage(PicInput.BackgroundImage);
			if (編集対象_音符.Checked)
			{
				g.Clear(Color.Transparent);
			}
			else
			{
				g.Clear(CtrlBackground);
			}

			g = Graphics.FromImage(PicInput.Image);
			g.Clear(Color.Transparent);

			PicInput.Image = PicInput.Image;
		}

		void DrawNotes()
		{
			var g = Graphics.FromImage(PicNote.Image);
			g.Clear(Color.Transparent);
			foreach (var ev in mEventList)
			{
				DrawEditableNote(g, ev.Begin, ev.End, ev.Tone);
			}
			PicNote.Image = PicNote.Image;
		}

		void DrawEditableNote(Graphics g, int a, int b, int tone)
		{
			if (tone < mDisplayMinTone || tone > mDisplayMaxTone)
			{
				return;
			}
			var top = PicNote.Height / 2 - mKeyHeight * (tone - mCenterTone);
			var bottom = top + mKeyHeight - 1;
			var width = b - a + 1;
			g.FillRectangle(EditableNote, a, top, width, mKeyHeight);
			g.DrawLine(EditableNoteBorderT, a, top, b, top);
			g.DrawLine(EditableNoteBorderB, a, bottom, b, bottom);
			g.DrawLine(EditableNoteBorderB, b, top, b, bottom);
		}

		void DrawGrippedNote(Graphics g, int a, int b, int tone)
		{
			if (tone < mDisplayMinTone || tone > mDisplayMaxTone)
			{
				return;
			}
			var top = PicNote.Height / 2 - mKeyHeight * (tone - mCenterTone);
			var bottom = top + mKeyHeight - 1;
			var width = b - a + 1;
			g.FillRectangle(GrippedNote, a, top, width, mKeyHeight);
			g.DrawLine(GrippedNoteBorderT, a, top, b, top);
			g.DrawLine(GrippedNoteBorderB, a, bottom, b, bottom);
			g.DrawLine(GrippedNoteBorderB, b, top, b, bottom);
		}

		void DrawSelectedNote(Graphics g, int a, int b, int tone)
		{
			if (tone < mDisplayMinTone || tone > mDisplayMaxTone)
			{
				return;
			}
			var top = PicNote.Height / 2 - mKeyHeight * (tone - mCenterTone);
			var bottom = top + mKeyHeight - 1;
			var width = b - a + 1;
			g.FillRectangle(SelectedNote, a, top, width, mKeyHeight);
			g.DrawLine(SelectedNoteBorderT, a, top, b, top);
			g.DrawLine(SelectedNoteBorderB, a, bottom, b, bottom);
			g.DrawLine(SelectedNoteBorderB, b, top, b, bottom);
		}

		void DrawReadonlyNote(Graphics g, int a, int b, int tone)
		{
			if (tone < mDisplayMinTone || tone > mDisplayMaxTone)
			{
				return;
			}
			var top = PicNote.Height / 2 - mKeyHeight * (tone - mCenterTone);
			var bottom = top + mKeyHeight - 1;
			var width = b - a + 1;
			g.FillRectangle(ReadonlyNote, a, top, width, mKeyHeight);
			g.DrawLine(ReadonlyNoteBorderT, a, top, b, top);
			g.DrawLine(ReadonlyNoteBorderB, a, bottom, b, bottom);
			g.DrawLine(ReadonlyNoteBorderB, b, top, b, bottom);
		}
		#endregion
	}
}
