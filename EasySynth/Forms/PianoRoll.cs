using System;
using System.Drawing;
using System.Windows.Forms;
using Synth;

namespace EasySynth.Forms {
	public partial class PianoRoll : Form {
		static readonly Font FontOctName = new Font("MS Gothic", 9.0f);
		static readonly StringFormat FormatOct = new StringFormat() {
			Alignment = StringAlignment.Far,
			LineAlignment = StringAlignment.Center
		};

		const int KeyboardWidth = 32;

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

		int mBaseTickWidth = 120;
		int mKeyHeight = 11;
		int mUnitTick = 1;
		int mCenterTone;
		int mCursorTone;
		int mCursorTick;
		int mDragStartTone;
		int mDragStartTick;

		int mEditTrack = 0;

		enum NoteType {
			EDITABLE,
			GRIPPED,
			SELECTED,
			READONLY
		}

		public PianoRoll() {
			InitializeComponent();
			SetColor();
			編集_Click(編集_書き込み, null);
			編集対象_Click(編集対象_音符, null);
			時間単位_Click(時間単位_16分, null);
			mCenterTone = 128 - Vsb.Value;
			PicNote.Controls.Add(PicInput);
			Timer1.Enabled = true;
			Timer1.Interval = 16;
			Timer1.Start();
		}

		#region メニューイベント
		private void 編集_Click(object sender, EventArgs e) {
			SetMenuChecked(sender, 編集);
		}

		private void 編集対象_Click(object sender, EventArgs e) {
			SetMenuChecked(sender, 編集対象);
		}

		private void 時間単位_Click(object sender, EventArgs e) {
			SetMenuChecked(sender, 時間単位);

			if (時間単位_フリーハンド.Checked) {
				mUnitTick = 1;
			}

			if (時間単位_8分.Checked) {
				mUnitTick = SMF.UnitTick >> 1;
			}
			if (時間単位_16分.Checked) {
				mUnitTick = SMF.UnitTick >> 2;
			}
			if (時間単位_32分.Checked) {
				mUnitTick = SMF.UnitTick >> 3;
			}
			if (時間単位_64分.Checked) {
				mUnitTick = SMF.UnitTick >> 4;
			}

			if (時間単位_8分3連.Checked) {
				mUnitTick = SMF.UnitTick / 3;
			}
			if (時間単位_16分3連.Checked) {
				mUnitTick = (SMF.UnitTick / 3) >> 1;
			}
			if (時間単位_32分3連.Checked) {
				mUnitTick = (SMF.UnitTick / 3) >> 2;
			}
			if (時間単位_64分3連.Checked) {
				mUnitTick = (SMF.UnitTick / 3) >> 3;
			}

			if (時間単位_8分5連.Checked) {
				mUnitTick = SMF.UnitTick / 5;
			}
			if (時間単位_16分5連.Checked) {
				mUnitTick = (SMF.UnitTick / 5) >> 1;
			}
			if (時間単位_32分5連.Checked) {
				mUnitTick = (SMF.UnitTick / 5) >> 2;
			}
			if (時間単位_64分5連.Checked) {
				mUnitTick = (SMF.UnitTick / 5) >> 3;
			}
		}

		private void 表示_Click(object sender, EventArgs e) {
			if (表示_次の小節へ移動 == sender) {
				mUpdateScreen = true;
			}
			if (表示_前の小節へ移動 == sender) {
				mUpdateScreen = true;
			}
			if (表示_時間方向拡大 == sender) {
				mBaseTickWidth <<= 1;
				if (mBaseTickWidth >= 240) {
					表示_時間方向拡大.Enabled = false;
				}
				表示_時間方向縮小.Enabled = true;
				mUpdateScreen = true;
			}
			if (表示_時間方向縮小 == sender) {
				mBaseTickWidth >>= 1;
				if (mBaseTickWidth <= 15) {
					表示_時間方向縮小.Enabled = false;
				}
				表示_時間方向拡大.Enabled = true;
				mUpdateScreen = true;
			}
			if (表示_音高方向拡大 == sender) {
				mKeyHeight++;
				mScrollTone = true;
				if (mKeyHeight >= 24) {
					表示_音高方向拡大.Enabled = false;
				}
				表示_音高方向縮小.Enabled = true;
			}
			if (表示_音高方向縮小 == sender) {
				mKeyHeight--;
				mScrollTone = true;
				if (mKeyHeight <= 6) {
					表示_音高方向縮小.Enabled = false;
				}
				表示_音高方向拡大.Enabled = true;
			}
		}

		void SetMenuChecked(object sender, ToolStripMenuItem parent) {
			if (sender is ToolStripMenuItem s && null == s.Image) {
				return;
			}
			for (int i = 0; i < parent.DropDownItems.Count; i++) {
				if (parent.DropDownItems[i] is ToolStripMenuItem item) {
					if (sender == item) {
						if (!item.Checked) {
							parent.Image = item.Image;
							parent.ToolTipText = item.Text;
							item.Checked = true;
							mChangeEditTarget = true;
						}
					} else {
						item.Checked = false;
					}
				}
			}
		}
		#endregion

		#region フォームコントロールイベント
		private void PianoRoll_KeyPress(object sender, KeyPressEventArgs e) {

		}

		private void PianoRoll_SizeChanged(object sender, EventArgs e) {
			mChangeSize = true;
		}

		private void Vsb_Scroll(object sender, ScrollEventArgs e) {
			mCenterTone = 128 - Vsb.Value;
			mScrollTone = true;
		}

		private void Hsb_Scroll(object sender, ScrollEventArgs e) {
			mUpdateScreen = true;
		}

		private void PicInput_MouseDown(object sender, MouseEventArgs e) {
			SetCursor(sender);
			mDragStartTone = mCursorTone;
			mDragStartTick = mCursorTick;
			mDragging = true;
		}

		private void PicInput_MouseUp(object sender, MouseEventArgs e) {
			SetCursor(sender);
			mDragging = false;
			mDragEnd = true;
		}

		private void PicInput_MouseMove(object sender, MouseEventArgs e) {
			SetCursor(sender);
			mDragMove = true;
		}

		void SetCursor(object sender) {
			var cursor = ((PictureBox)sender).PointToClient(MousePosition);
			var dTone = mCenterTone - (cursor.Y - PicNote.Height / 2.0) / mKeyHeight;
			var tone = (int)(Math.Sign(dTone) + dTone);
			mCursorTone = Math.Max(0, Math.Min(127, tone));
			mCursorTick = XToTick(cursor.X); 
		}
		#endregion

		private void Timer1_Tick(object sender, EventArgs e) {
			if (mChangeSize) {
				ChangeSize();
				DrawNoteBackground();
				DrawInputBackground();
			}

			if (mScrollTone) {
				DrawNoteBackground();
			}

			if (mChangeEditTarget) {
				DrawInputBackground();
			}

			if (編集対象_音符.Checked) {
				SelectNote();
			}
			if (編集対象_アクセント.Checked) {
				SelectVelocity();
			}

			if (mChangeSize || mScrollTone || mChangeEditTarget || mUpdateScreen) {
				DrawNotes();
				if (編集対象_アクセント.Checked) {
					DrawVelocity();
				}
			}
			mChangeSize = false;
			mScrollTone = false;
			mChangeEditTarget = false;
			mUpdateScreen = false;

			if ((mDragging && mDragMove) || mDragEnd) {
				if (編集対象_音符.Checked) {
					EditNote();
				}
				if (編集対象_アクセント.Checked) {
					EditVelocity();
				}
				mDragMove = false;
				mDragEnd = false;
			}
		}

		int TickToX(int tick) {
			return KeyboardWidth + (tick - Hsb.Value) * mBaseTickWidth / SMF.UnitTick;
		}

		int XToTick(int x) {
			var tick = (x - KeyboardWidth) * SMF.UnitTick / mBaseTickWidth + Hsb.Value;
			return (int)((double)tick / mUnitTick) * mUnitTick;
		}

		void ChangeSize() {
			if (null != PicNote.Image) {
				PicNote.Image.Dispose();
				PicNote.Image = null;
			}
			if (null != PicNote.BackgroundImage) {
				PicNote.BackgroundImage.Dispose();
				PicNote.BackgroundImage = null;
			}

			if (null != PicInput.Image) {
				PicInput.Image.Dispose();
				PicInput.Image = null;
			}
			if (null != PicInput.BackgroundImage) {
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

			int width;
			if (PicNote.Width < MinimumSize.Width) {
				width = MinimumSize.Width;
			} else {
				width = PicNote.Width;
			}
			int height;
			if (PicNote.Height < MinimumSize.Height) {
				height = MinimumSize.Height;
			} else {
				height = PicNote.Height;
			}
			PicNote.Image = new Bitmap(width, height);
			PicNote.BackgroundImage = new Bitmap(width, height);
			PicInput.Image = new Bitmap(width, height);
			PicInput.BackgroundImage = new Bitmap(width, height);
		}

		#region 選択メソッド
		void SelectNote() {
		}

		void SelectVelocity() {
		}
		#endregion

		#region 編集メソッド
		void EditNote() {
			if (mCursorTone <= mDisplayMinTone) {
				Vsb.Value = Math.Min(127, Vsb.Value + 1);
				mCenterTone = 128 - Vsb.Value;
				DrawNoteBackground();
			}
			if (mCursorTone > mDisplayMaxTone) {
				Vsb.Value = Math.Max(0, Vsb.Value - 1);
				mCenterTone = 128 - Vsb.Value;
				DrawNoteBackground();
			}

			var g = Graphics.FromImage(PicInput.Image);
			g.Clear(Color.Transparent);

			var a = Math.Min(mDragStartTick, mCursorTick);
			var b = Math.Max(mDragStartTick, mCursorTick) + mUnitTick;

			if (mDragEnd) {
				EventList.AddNote(mEditTrack, 0, a, b, mCursorTone, 63);
				mUpdateScreen = true;
				return;
			}

			DrawNote(g, NoteType.SELECTED, TickToX(a), TickToX(b), mCursorTone);

			PicInput.Image = PicInput.Image;
		}

		void EditVelocity() {
		}
		#endregion

		#region 描画メソッド
		void DrawNoteBackground() {
			mDisplayTones = PicNote.Height / mKeyHeight;
			mDisplayMinTone = Math.Max(0, mCenterTone - mDisplayTones / 2);
			mDisplayMaxTone = Math.Min(127, mCenterTone + mDisplayTones / 2);

			var g = Graphics.FromImage(PicNote.BackgroundImage);
			g.Clear(Color.Transparent);
			for (int tone = mDisplayMinTone; tone <= mDisplayMaxTone; tone++) {
				var top = PicNote.Height / 2 - mKeyHeight * (tone - mCenterTone);
				var bottom = top + mKeyHeight - 1;
				switch (tone % 12) {
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

		void DrawInputBackground() {
			var g = Graphics.FromImage(PicInput.BackgroundImage);
			if (編集対象_音符.Checked) {
				g.Clear(Color.Transparent);
			} else {
				g.Clear(CtrlBackground);
			}

			g.DrawLine(MeasureBorder, KeyboardWidth, 0, KeyboardWidth, PicNote.Height);

			if (編集対象_アクセント.Checked) {
				var min = -18;
				for (int db = -1; db > min; db--) {
					var y = PicInput.Height * db / min;
					if (db == -6) {
						g.DrawLine(CtrlGridZero, 0, y, PicInput.Width - 1, y);
					} else {
						if (db % 6 == 0) {
							g.DrawLine(CtrlGridMajor, 0, y, PicInput.Width - 1, y);
						} else {
							g.DrawLine(CtrlGridMinor, 0, y, PicInput.Width - 1, y);
						}
					}
				}
			}

			g = Graphics.FromImage(PicInput.Image);
			g.Clear(Color.Transparent);

			PicInput.Image = PicInput.Image;
		}

		void DrawNotes() {
			var g = Graphics.FromImage(PicNote.Image);
			g.Clear(Color.Transparent);
			foreach (var ev in EventList.List) {
				if (ev.Track == mEditTrack || ev.Selected) {
					continue;
				}
				if (ev is Note note) {
					DrawNote(g, NoteType.READONLY, TickToX(ev.Tick), TickToX(note.End), note.Tone);
				}
			}
			foreach (var ev in EventList.List) {
				if (ev.MsgType != SMF.MSG.NOTE_ON || ev.Track != mEditTrack || ev.Selected) {
					continue;
				}
				if (ev is Note note) {
					DrawNote(g, NoteType.EDITABLE, TickToX(ev.Tick), TickToX(note.End), note.Tone);
				}
			}
			foreach (var ev in EventList.List) {
				if (ev.MsgType != SMF.MSG.NOTE_ON || !ev.Selected) {
					continue;
				}
				if (ev is Note note) {
					DrawNote(g, NoteType.SELECTED, TickToX(ev.Tick), TickToX(note.End), note.Tone);
				}
			}
			PicNote.Image = PicNote.Image;
		}

		void DrawVelocity() {
			var g = Graphics.FromImage(PicInput.Image);
			g.Clear(Color.Transparent);
			var notes = EventList.SelectNotes(Hsb.Value, XToTick(Hsb.Width), 0, 127, false, mEditTrack);
			foreach (var ev in notes) {
				if (ev is Note note && note.IsNoteOn) {
					var a = TickToX(note.Tick);
					var b = TickToX(note.End);
					if (b >= PicInput.Width) {
						b = PicInput.Width - 1;
					}
					var db = 20 * Math.Log10(note.Velocity / 127.0);
					var y = (int)(-PicInput.Height * 0.5 * (int)(db * 2) / 18.0);
					g.FillRectangle(CtrlValueSolid, a + 1, y, b - a - 1, PicInput.Height - y);
					g.DrawLine(CtrlValueLeft, a, PicInput.Height, a, y);
					g.DrawLine(CtrlValueTop, a + 1, y, b, y);
				}
			}
			PicInput.Image = PicInput.Image;
		}

		void DrawNote(Graphics g, NoteType type, int a, int b, int tone) {
			if (tone < mDisplayMinTone || tone > mDisplayMaxTone) {
				return;
			}
			var top = PicNote.Height / 2 - mKeyHeight * (tone - mCenterTone);
			var bottom = top + mKeyHeight - 1;
			var width = b - a;
			b--;
			switch (type) {
			case NoteType.EDITABLE:
				g.FillRectangle(EditableNote, a, top, width, mKeyHeight);
				g.DrawLine(EditableNoteBorderR, b, top, b, bottom);
				g.DrawLine(EditableNoteBorderT, a, top, b, top);
				g.DrawLine(EditableNoteBorderB, a, bottom, b, bottom);
				g.DrawLine(EditableNoteBorderB, a, top, a, bottom);
				break;
			case NoteType.GRIPPED:
				g.FillRectangle(GrippedNote, a, top, width, mKeyHeight);
				g.DrawLine(GrippedNoteBorderR, b, top, b, bottom);
				g.DrawLine(GrippedNoteBorderT, a, top, b, top);
				g.DrawLine(GrippedNoteBorderB, a, bottom, b, bottom);
				g.DrawLine(GrippedNoteBorderB, a, top, a, bottom);
				break;
			case NoteType.SELECTED:
				g.FillRectangle(SelectedNote, a, top, width, mKeyHeight);
				g.DrawLine(SelectedNoteBorderR, b, top, b, bottom);
				g.DrawLine(SelectedNoteBorderT, a, top, b, top);
				g.DrawLine(SelectedNoteBorderB, a, bottom, b, bottom);
				g.DrawLine(SelectedNoteBorderB, a, top, a, bottom);
				break;
			case NoteType.READONLY:
				g.FillRectangle(ReadonlyNote, a, top, width, mKeyHeight);
				g.DrawLine(ReadonlyNoteBorderR, b, top, b, bottom);
				g.DrawLine(ReadonlyNoteBorderT, a, top, b, top);
				g.DrawLine(ReadonlyNoteBorderB, a, bottom, b, bottom);
				g.DrawLine(ReadonlyNoteBorderB, a, top, a, bottom);
				break;
			}
		}
		#endregion
	}
}
