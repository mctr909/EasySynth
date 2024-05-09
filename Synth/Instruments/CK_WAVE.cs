using System.Runtime.InteropServices;

namespace Synth.Instruments {
	public struct CK_WAVE {
		public uint Offset;
		public uint Samplerate;
		public uint LoopStart;
		public uint LoopLength;
		public byte LoopEnable;
		public byte UnityNote;
		public short Gain;
		public float Finetune;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
		public char[] Name;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
		public char[] Category;
	}
}
