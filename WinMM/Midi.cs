using System;
using System.Runtime.InteropServices;

namespace WinMM {
	public abstract class Midi : IDisposable {
		protected enum MIDIHDR_FLAG : uint {
			MHDR_NONE = 0,
			MHDR_DONE = 0x00000001,
			MHDR_PREPARED = 0x00000002,
			MHDR_INQUEUE = 0x00000004,
			MHDR_ISSTRM = 0x00000008
		}

		[StructLayout(LayoutKind.Sequential)]
		protected struct MIDIHDR {
			public IntPtr lpData;
			public uint dwBufferLength;
			public uint dwBytesRecorded;
			public uint dwUser;
			public MIDIHDR_FLAG dwFlags;
			public IntPtr lpNext;
			public uint reserved;
			public uint dwOffset;
			public uint dwReserved1;
			public uint dwReserved2;
			public uint dwReserved3;
			public uint dwReserved4;
		}

		protected const uint MIDI_MAPPER = unchecked((uint)-1);

		#region dynamic variable
		protected IntPtr mHandle;
		protected IntPtr[] mpMidiHeader;
		protected int mBufferCount;
		protected byte[] mBuffer;
		protected bool mDoStop = false;
		protected bool mStopped = true;
		#endregion

		#region property
		public bool Enabled { get; protected set; }
		public uint DeviceId { get; private set; } = MIDI_MAPPER;
		public int BufferSize { get; private set; }
		#endregion

		protected Midi(int bufferSize, int bufferCount) {
			BufferSize = bufferSize;
			mBufferCount = bufferCount;
			mBuffer = new byte[bufferSize];
		}

		protected void AllocHeader() {
			var defaultValue = new byte[BufferSize];
			mpMidiHeader = new IntPtr[mBufferCount];
			for (int i = 0; i < mBufferCount; ++i) {
				var hdr = new MIDIHDR() {
					dwFlags = MIDIHDR_FLAG.MHDR_NONE,
					dwBufferLength = (uint)BufferSize
				};
				hdr.lpData = Marshal.AllocHGlobal(BufferSize);
				Marshal.Copy(defaultValue, 0, hdr.lpData, BufferSize);
				mpMidiHeader[i] = Marshal.AllocHGlobal(Marshal.SizeOf<MIDIHDR>());
				Marshal.StructureToPtr(hdr, mpMidiHeader[i], true);
			}
		}

		protected void DisposeHeader() {
			for (int i = 0; i < mBufferCount; ++i) {
				if (mpMidiHeader[i] == IntPtr.Zero) {
					continue;
				}
				var hdr = Marshal.PtrToStructure<MIDIHDR>(mpMidiHeader[i]);
				if (hdr.lpData != IntPtr.Zero) {
					Marshal.FreeHGlobal(hdr.lpData);
				}
				Marshal.FreeHGlobal(mpMidiHeader[i]);
				mpMidiHeader[i] = IntPtr.Zero;
			}
		}

		public void Dispose() {
			MidiClose();
		}

		public void SetDevice(uint deviceId) {
			var enable = Enabled;
			MidiClose();
			DeviceId = deviceId;
			if (enable) {
				MidiOpen();
			}
		}

		protected abstract void MidiOpen();

		protected abstract void MidiClose();
	}
}
