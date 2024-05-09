using System;
using System.Drawing;

namespace EasySynth.Forms {
	public partial class PianoRoll {
		static Pen MeasureBorder;
		static Pen BeatBorder;
		static Pen OctBorder;
		static Pen KeyBorder;

		static Color CtrlBackground;
		static Pen CtrlValueTop;
		static Pen CtrlValueLeft;
		static Brush CtrlValueSolid;
		static Pen CtrlGridZero;
		static Pen CtrlGridMajor;
		static Pen CtrlGridMinor;

		static Brush WhiteKey;
		static Brush BlackKey;
		static Pen BlackKeyBorderT;
		static Pen BlackKeyBorderB;
		static Brush EditableNote;
		static Pen EditableNoteBorderT;
		static Pen EditableNoteBorderB;
		static Pen EditableNoteBorderR;
		static Brush GrippedNote;
		static Pen GrippedNoteBorderT;
		static Pen GrippedNoteBorderB;
		static Pen GrippedNoteBorderR;
		static Brush SelectedNote;
		static Pen SelectedNoteBorderT;
		static Pen SelectedNoteBorderB;
		static Pen SelectedNoteBorderR;
		static Brush ReadonlyNote;
		static Pen ReadonlyNoteBorderT;
		static Pen ReadonlyNoteBorderB;
		static Pen ReadonlyNoteBorderR;

		public static void SetColor() {
			var cMeasureBorder = Color.FromArgb(0, 0, 0);
			var cBeatBorder = Color.FromArgb(167, 167, 131);
			var cOctBorder = Color.FromArgb(0, 0, 0);

			var cCtrlBackground = Color.FromArgb(191, 255, 255, 255);
			var cCtrlGrid = Color.FromArgb(0, 0, 255);
			var cCtrlZero = Color.FromArgb(255, 0, 0);
			var cCtrlValue = Color.FromArgb(0, 0, 0);
			var cCtrlValueSolid = Color.FromArgb(17, 0, 255, 0);

			var cWhiteKey = Color.FromArgb(245, 245, 245);
			var cBlackKey = Color.FromArgb(191, 191, 191);
			var cEditableNote = Color.FromArgb(0, 191, 0);
			var cSelectedNote = Color.FromArgb(211, 95, 95);
			var cReadonlyNote = Color.FromArgb(71, 171, 211);

			MeasureBorder = new Pen(cMeasureBorder);
			BeatBorder = new Pen(cBeatBorder);
			OctBorder = new Pen(cOctBorder);
			KeyBorder = new Pen(SetLA(cWhiteKey, 0.66, 1));

			CtrlBackground = cCtrlBackground;
			CtrlGridMajor = new Pen(cCtrlGrid, 2);
			CtrlGridMinor = new Pen(cCtrlGrid) {
				DashPattern = new float[] { 2, 1 }
			};
			CtrlGridZero = new Pen(cCtrlZero, 2);
			CtrlValueTop = new Pen(cCtrlValue, 4);
			CtrlValueLeft = new Pen(cCtrlValue);
			CtrlValueSolid = new Pen(cCtrlValueSolid).Brush;

			WhiteKey = new Pen(cWhiteKey).Brush;
			BlackKey = new Pen(cBlackKey).Brush;
			BlackKeyBorderT = new Pen(SetLA(cBlackKey, 1.00, 1));
			BlackKeyBorderB = new Pen(SetLA(cBlackKey, 0.75, 1));
			EditableNote = new Pen(cEditableNote).Brush;
			EditableNoteBorderT = new Pen(SetLA(cEditableNote, 2.0, 1));
			EditableNoteBorderB = new Pen(SetLA(cEditableNote, 0.8, 1));
			EditableNoteBorderR = new Pen(SetLA(cEditableNote, 1.2, 1));
			GrippedNote = new Pen(SetLA(cEditableNote, 2.0, 0.5)).Brush;
			GrippedNoteBorderT = new Pen(SetLA(cEditableNote, 2.0, 0.5));
			GrippedNoteBorderB = new Pen(SetLA(cEditableNote, 0.5, 0.5));
			GrippedNoteBorderR = new Pen(SetLA(cEditableNote, 1.2, 0.5));
			SelectedNote = new Pen(cSelectedNote).Brush;
			SelectedNoteBorderT = new Pen(SetLA(cSelectedNote, 1.25, 1));
			SelectedNoteBorderB = new Pen(SetLA(cSelectedNote, 0.8, 1));
			SelectedNoteBorderR = new Pen(SetLA(cSelectedNote, 1.1, 1));
			ReadonlyNote = new Pen(cReadonlyNote).Brush;
			ReadonlyNoteBorderT = new Pen(SetLA(cReadonlyNote, 1.5, 1));
			ReadonlyNoteBorderB = new Pen(SetLA(cReadonlyNote, 0.8, 1));
			ReadonlyNoteBorderR = new Pen(SetLA(cReadonlyNote, 1.1, 1));
		}

		public static Color SetLA(Color color, double l, double a) {
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
	}
}
