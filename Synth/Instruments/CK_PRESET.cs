using System.Runtime.InteropServices;

namespace Synth.Instruments {
	public struct CK_PRESET {
		public byte IsTonal;
		public byte BankMSB;
		public byte BankLSB;
		public byte ProgNum;
		public uint LayerIndex;
		public uint LayerCount;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=32)]
		public char[] Name;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst=20)]
		public char[] Category;
	}
}
