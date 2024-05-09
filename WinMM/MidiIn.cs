using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace WinMM {
	public abstract class MidiIn : Midi {
		enum MM_MIM {
			OPEN = 0x3C1,
			CLOSE = 0x3C2,
			DATA = 0x3C3,
			LONGDATA = 0x3C4
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct MIDIINCAPS {
			public ushort wMid;
			public ushort wPid;
			public uint vDriverVersion;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string szPname;
			public uint dwSupport;
		}

		delegate void DCallback(IntPtr hmi, MM_MIM uMsg, IntPtr lpMidiHdr, uint dwParam1, uint dwParam2);
		DCallback mCallback;

		#region dll
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern uint midiInGetNumDevs();
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT midiInGetDevCaps(uint uDeviceID, ref MIDIINCAPS pmic, int size);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT midiInOpen(ref IntPtr hmi, uint uDeviceID, DCallback dwCallback, IntPtr dwInstance, uint dwFlags = 0x00030000);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT midiInClose(IntPtr hmi);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT midiInPrepareHeader(IntPtr hmi, IntPtr lpMidiHdr, int size);
		[DllImport("winmm.dll")]
		static extern MMRESULT midiInUnprepareHeader(IntPtr hmi, IntPtr lpMidiHdr, int size);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT midiInReset(IntPtr hmi);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT midiInAddBuffer(IntPtr hmi, IntPtr lpMidiHdr, int size);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT midiInStart(IntPtr hmi);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT midiInMessage(IntPtr hmi, uint uMsg, IntPtr dw1, IntPtr dw2);
		#endregion

		public static List<string> GetDeviceList() {
			var list = new List<string>();
			var deviceCount = midiInGetNumDevs();
			for (uint i = 0; i < deviceCount; i++) {
				var caps = new MIDIINCAPS();
				var ret = midiInGetDevCaps(i, ref caps, Marshal.SizeOf(caps));
				if (MMRESULT.MMSYSERR_NOERROR == ret) {
					list.Add(caps.szPname);
				} else {
					list.Add(ret.ToString());
				}
			}
			return list;
		}

		public MidiIn(int bufferSize = 1024, int bufferCount = 16) : base(bufferSize, bufferCount) {
			mCallback = Callback;
		}

		protected override void MidiOpen() {
			MidiClose();
			AllocHeader();
			var mr = midiInOpen(ref mHandle, DeviceId, mCallback, IntPtr.Zero);
			if (MMRESULT.MMSYSERR_NOERROR != mr) {
				return;
			}
			for (int i = 0; i < mBufferCount; ++i) {
				midiInPrepareHeader(mHandle, mpMidiHeader[i], Marshal.SizeOf<MIDIHDR>());
				midiInAddBuffer(mHandle, mpMidiHeader[i], Marshal.SizeOf<MIDIHDR>());
			}
			midiInStart(mHandle);
		}

		protected override void MidiClose() {
			if (IntPtr.Zero == mHandle) {
				return;
			}
			mDoStop = true;
			for (int i = 0; i < 20 && !mStopped; i++) {
				Thread.Sleep(100);
			}
			for (int i = 0; i < mBufferCount; ++i) {
				midiInUnprepareHeader(mpMidiHeader[i], mHandle, Marshal.SizeOf<MIDIHDR>());
			}
			var mr = midiInReset(mHandle);
			if (MMRESULT.MMSYSERR_NOERROR != mr) {
				throw new Exception(mr.ToString());
			}
			mr = midiInClose(mHandle);
			if (MMRESULT.MMSYSERR_NOERROR != mr) {
				throw new Exception(mr.ToString());
			}
			mHandle = IntPtr.Zero;
			DisposeHeader();
		}

		void Callback(IntPtr hmi, MM_MIM uMsg, IntPtr lpMidiHdr, uint dwParam1, uint dwParam2) {
			switch (uMsg) {
			case MM_MIM.OPEN:
				mStopped = false;
				Enabled = true;
				break;
			case MM_MIM.CLOSE:
				mDoStop = false;
				Enabled = false;
				break;
			case MM_MIM.DATA:
				if (mDoStop) {
					mStopped = true;
					break;
				}
				switch (dwParam1 & 0xF0) {
				case 0x80:
				case 0x90:
				case 0xB0:
				case 0xE0:
					Receive(new byte[] {
						(byte)(dwParam1 & 0xFF),
						(byte)((dwParam1 >> 8) & 0xFF),
						(byte)((dwParam1 >> 16) & 0xFF)
					});
					break;
				case 0xC0:
					Receive(new byte[] {
						(byte)(dwParam1 & 0xFF),
						(byte)((dwParam1 >> 8) & 0xFF)
					});
					break;
				}
				midiInAddBuffer(mHandle, lpMidiHdr, Marshal.SizeOf<MIDIHDR>());
				break;
			case MM_MIM.LONGDATA:
				if (mDoStop) {
					mStopped = true;
					break;
				}
				break;
			default:
				break;
			}
		}

		protected abstract void Receive(byte[] message);

		protected virtual void ReadBuffer() { }
	}
}
