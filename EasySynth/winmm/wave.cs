﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace WINMM {
	public abstract class WaveLib : IDisposable {
		protected enum WAVEHDR_FLAG : uint {
			WHDR_NONE = 0,
			WHDR_DONE = 0x00000001,
			WHDR_PREPARED = 0x00000002,
			WHDR_BEGINLOOP = 0x00000004,
			WHDR_ENDLOOP = 0x00000008,
			WHDR_INQUEUE = 0x00000010
		}
		protected enum WAVE_FORMAT : uint {
			MONO_8bit_11kHz = 0x1,
			MONO_8bit_22kHz = 0x10,
			MONO_8bit_44kHz = 0x100,
			MONO_8bit_48kHz = 0x1000,
			MONO_8bit_96kHz = 0x10000,
			STEREO_8bit_11kHz = 0x2,
			STEREO_8bit_22kHz = 0x20,
			STEREO_8bit_44kHz = 0x200,
			STEREO_8bit_48kHz = 0x2000,
			STEREO_8bit_96kHz = 0x20000,
			MONO_16bit_11kHz = 0x4,
			MONO_16bit_22kHz = 0x40,
			MONO_16bit_44kHz = 0x400,
			MONO_16bit_48kHz = 0x4000,
			MONO_16bit_96kHz = 0x40000,
			STEREO_16bit_11kHz = 0x8,
			STEREO_16bit_22kHz = 0x80,
			STEREO_16bit_44kHz = 0x800,
			STEREO_16bit_48kHz = 0x8000,
			STEREO_16bit_96kHz = 0x80000
		}

		[StructLayout(LayoutKind.Sequential)]
		protected struct WAVEFORMATEX {
			public ushort wFormatTag;
			public ushort nChannels;
			public uint nSamplesPerSec;
			public uint nAvgBytesPerSec;
			public ushort nBlockAlign;
			public ushort wBitsPerSample;
			public ushort cbSize;
		}
		[StructLayout(LayoutKind.Sequential)]
		protected struct WAVEHDR {
			public IntPtr lpData;
			public uint dwBufferLength;
			public uint dwBytesRecorded;
			public uint dwUser;
			public WAVEHDR_FLAG dwFlags;
			public uint dwLoops;
			public IntPtr lpNext;
			public uint reserved;
		}

		public enum BUFFER_TYPE {
			INTEGER = 0,
			I16 = INTEGER | 16,
			I24 = INTEGER | 24,
			I32 = INTEGER | 32,
			FLOAT = 0x100,
			F32 = FLOAT | 32
		}

		protected readonly BUFFER_TYPE BufferType;
		protected readonly ushort BitCount;
		protected readonly ushort Channels;
		protected readonly int SampleRate;
		protected readonly int BufferSamples;

		protected const uint WAVE_MAPPER = unchecked((uint)-1);

		#region dynamic variable
		protected IntPtr mHandle;
		protected WAVEFORMATEX mWaveFormatEx;
		protected IntPtr[] mpWaveHeader;
		protected int mBufferCount;
		protected bool mDoStop = false;
		protected bool mStopped = true;
		#endregion

		#region property
		public bool Enabled { get; protected set; }
		public uint DeviceId { get; private set; } = WAVE_MAPPER;
		#endregion

		protected WaveLib(int sampleRate, int channels, BUFFER_TYPE bufferType, int bufferSamples, int bufferCount) {
			BufferType = bufferType;
			BitCount = (ushort)((int)bufferType & 0xFF);
			Channels = (ushort)channels;
			SampleRate = sampleRate;
			BufferSamples = bufferSamples;
			mBufferCount = bufferCount;
			mWaveFormatEx = new WAVEFORMATEX() {
				wFormatTag = (ushort)(((int)bufferType & 0x100) > 0 ? 3 : 1),
				nChannels = (ushort)channels,
				nSamplesPerSec = (uint)sampleRate,
				nAvgBytesPerSec = (uint)(sampleRate * channels * BitCount >> 3),
				nBlockAlign = (ushort)(channels * BitCount >> 3),
				wBitsPerSample = BitCount,
				cbSize = 0
			};
		}

		protected void AllocHeader() {
			var bufferBytes = BufferSamples * Channels * BitCount >> 3;
			var defaultValue = new byte[bufferBytes];
			mpWaveHeader = new IntPtr[mBufferCount];
			for (int i = 0; i < mBufferCount; ++i) {
				var hdr = new WAVEHDR() {
					dwFlags = WAVEHDR_FLAG.WHDR_NONE,
					dwBufferLength = (uint)bufferBytes
				};
				hdr.lpData = Marshal.AllocHGlobal((int)hdr.dwBufferLength);
				Marshal.Copy(defaultValue, 0, hdr.lpData, bufferBytes);
				mpWaveHeader[i] = Marshal.AllocHGlobal(Marshal.SizeOf<WAVEHDR>());
				Marshal.StructureToPtr(hdr, mpWaveHeader[i], true);
			}
		}

		protected void DisposeHeader() {
			for (int i = 0; i < mBufferCount; ++i) {
				if (mpWaveHeader[i] == IntPtr.Zero) {
					continue;
				}
				var hdr = Marshal.PtrToStructure<WAVEHDR>(mpWaveHeader[i]);
				if (hdr.lpData != IntPtr.Zero) {
					Marshal.FreeHGlobal(hdr.lpData);
				}
				Marshal.FreeHGlobal(mpWaveHeader[i]);
				mpWaveHeader[i] = IntPtr.Zero;
			}
		}

		public void Dispose() {
			WaveClose();
		}

		public void SetDevice(uint deviceId) {
			var enable = Enabled;
			WaveClose();
			DeviceId = deviceId;
			if (enable) {
				WaveOpen();
			}
		}

		protected abstract void WaveOpen();

		protected abstract void WaveClose();
	}

	public abstract class WaveIn : WaveLib {
		enum MM_WIM {
			OPEN = 0x3BE,
			CLOSE = 0x3BF,
			DATA = 0x3C0
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct WAVEINCAPS {
			public ushort wMid;
			public ushort wPid;
			public uint vDriverVersion;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string szPname;
			private uint dwFormats;
			public ushort wChannels;
			public ushort wReserved1;
			public List<WAVE_FORMAT> Formats {
				get {
					var list = new List<WAVE_FORMAT>();
					for (int i = 0, s = 1; i < 32; i++, s <<= 1) {
						if (0 < (dwFormats & s)) {
							list.Add((WAVE_FORMAT)s);
						}
					}
					return list;
				}
			}
		}

		delegate void DCallback(IntPtr hwi, MM_WIM uMsg, int dwUser, IntPtr lpWaveHdr, int dwParam2);
		DCallback mCallback;

		#region dll
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern uint waveInGetNumDevs();
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveInGetDevCaps(uint uDeviceID, ref WAVEINCAPS pwic, int size);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveInOpen(ref IntPtr hwi, uint uDeviceID, ref WAVEFORMATEX lpFormat, DCallback dwCallback, IntPtr dwInstance, uint dwFlags = 0x00030000);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveInClose(IntPtr hwi);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveInPrepareHeader(IntPtr hwi, IntPtr lpWaveHdr, int size);
		[DllImport("winmm.dll")]
		static extern MMRESULT waveInUnprepareHeader(IntPtr hwi, IntPtr lpWaveHdr, int size);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveInReset(IntPtr hwi);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveInAddBuffer(IntPtr hwi, IntPtr lpWaveHdr, int size);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveInStart(IntPtr hwi);
		#endregion

		public static List<string> GetDeviceList() {
			var list = new List<string>();
			var deviceCount = waveInGetNumDevs();
			for (uint i = 0; i < deviceCount; i++) {
				var caps = new WAVEINCAPS();
				var ret = waveInGetDevCaps(i, ref caps, Marshal.SizeOf(caps));
				if (MMRESULT.MMSYSERR_NOERROR == ret) {
					list.Add(caps.szPname);
				} else {
					list.Add(ret.ToString());
				}
			}
			return list;
		}

		public WaveIn(int sampleRate, int channels, BUFFER_TYPE bufferType, int bufferSamples, int bufferCount) :
			base(sampleRate, channels, bufferType, bufferSamples, bufferCount) {
			mCallback = Callback;
		}

		protected override void WaveOpen() {
			WaveClose();
			AllocHeader();
			var mr = waveInOpen(ref mHandle, DeviceId, ref mWaveFormatEx, mCallback, IntPtr.Zero);
			if (MMRESULT.MMSYSERR_NOERROR != mr) {
				return;
			}
			for (int i = 0; i < mBufferCount; ++i) {
				waveInPrepareHeader(mHandle, mpWaveHeader[i], Marshal.SizeOf<WAVEHDR>());
				waveInAddBuffer(mHandle, mpWaveHeader[i], Marshal.SizeOf<WAVEHDR>());
			}
			waveInStart(mHandle);
		}

		protected override void WaveClose() {
			if (IntPtr.Zero == mHandle) {
				return;
			}
			mDoStop = true;
			for (int i = 0; i < 20 && !mStopped; i++) {
				Thread.Sleep(100);
			}
			for (int i = 0; i < mBufferCount; ++i) {
				waveInUnprepareHeader(mpWaveHeader[i], mHandle, Marshal.SizeOf<WAVEHDR>());
			}
			var mr = waveInReset(mHandle);
			if (MMRESULT.MMSYSERR_NOERROR != mr) {
				throw new Exception(mr.ToString());
			}
			mr = waveInClose(mHandle);
			if (MMRESULT.MMSYSERR_NOERROR != mr) {
				throw new Exception(mr.ToString());
			}
			mHandle = IntPtr.Zero;
			DisposeHeader();
		}

		void Callback(IntPtr hwi, MM_WIM uMsg, int dwUser, IntPtr lpWaveHdr, int dwParam2) {
			switch (uMsg) {
			case MM_WIM.OPEN:
				mStopped = false;
				Enabled = true;
				break;
			case MM_WIM.CLOSE:
				mDoStop = false;
				Enabled = false;
				break;
			case MM_WIM.DATA:
				if (mDoStop) {
					mStopped = true;
					break;
				}
				var hdr = Marshal.PtrToStructure<WAVEHDR>(lpWaveHdr);
				ReadBuffer(hdr.lpData);
				waveInAddBuffer(mHandle, lpWaveHdr, Marshal.SizeOf<WAVEHDR>());
				break;
			}
		}

		protected abstract void ReadBuffer(IntPtr pBuffer);
	}

	public abstract class WaveOut : WaveLib {
		enum MM_WOM {
			OPEN = 0x3BB,
			CLOSE = 0x3BC,
			DONE = 0x3BD
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		struct WAVEOUTCAPS {
			public ushort wMid;
			public ushort wPid;
			public uint vDriverVersion;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32)]
			public string szPname;
			private uint dwFormats;
			public ushort wChannels;
			public ushort wReserved1;
			public uint dwSupport;
			public List<WAVE_FORMAT> Formats {
				get {
					var list = new List<WAVE_FORMAT>();
					for (int i = 0, s = 1; i < 32; i++, s <<= 1) {
						if (0 < (dwFormats & s)) {
							list.Add((WAVE_FORMAT)s);
						}
					}
					return list;
				}
			}
		}

		delegate void DCallback(IntPtr hwo, MM_WOM uMsg, int dwUser, IntPtr lpWaveHdr, int dwParam2);
		DCallback mCallback;

		#region dll
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern uint waveOutGetNumDevs();
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveOutGetDevCaps(uint uDeviceID, ref WAVEOUTCAPS pwoc, int size);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveOutOpen(ref IntPtr hwo, uint uDeviceID, ref WAVEFORMATEX lpFormat, DCallback dwCallback, IntPtr dwInstance, uint dwFlags);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveOutClose(IntPtr hwo);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveOutPrepareHeader(IntPtr hwo, IntPtr lpWaveHdr, int size);
		[DllImport("winmm.dll")]
		static extern MMRESULT waveOutUnprepareHeader(IntPtr hwo, IntPtr lpWaveHdr, int size);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveOutReset(IntPtr hwo);
		[DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern MMRESULT waveOutWrite(IntPtr hwo, IntPtr lpWaveHdr, int size);
		#endregion

		#region dynamic variable
		Thread mBufferThread;
		object mLockBuffer = new object();
		int mWriteCount;
		int mWriteIndex;
		int mReadIndex;
		#endregion

		public static List<string> GetDeviceList() {
			var list = new List<string>();
			var deviceCount = waveOutGetNumDevs();
			for (uint i = 0; i < deviceCount; i++) {
				var caps = new WAVEOUTCAPS();
				var ret = waveOutGetDevCaps(i, ref caps, Marshal.SizeOf(caps));
				if (MMRESULT.MMSYSERR_NOERROR == ret) {
					list.Add(caps.szPname);
				} else {
					list.Add(ret.ToString());
				}
			}
			return list;
		}

		public WaveOut(int sampleRate, int channels, BUFFER_TYPE bufferType, int bufferSamples, int bufferCount) :
			base(sampleRate, channels, bufferType, bufferSamples, bufferCount) {
			mCallback = Callback;
		}

		protected override void WaveOpen() {
			WaveClose();
			AllocHeader();
			var ret = waveOutOpen(ref mHandle, DeviceId, ref mWaveFormatEx, mCallback, IntPtr.Zero, 0x00030000);
			if (MMRESULT.MMSYSERR_NOERROR != ret) {
				return;
			}
			for (int i = 0; i < mBufferCount; ++i) {
				waveOutPrepareHeader(mHandle, mpWaveHeader[i], Marshal.SizeOf<WAVEHDR>());
				waveOutWrite(mHandle, mpWaveHeader[i], Marshal.SizeOf<WAVEHDR>());
			}
			mBufferThread = new Thread(BufferTask) {
				Priority = ThreadPriority.Highest
			};
			mBufferThread.Start();
		}

		protected override void WaveClose() {
			if (IntPtr.Zero == mHandle) {
				return;
			}
			mDoStop = true;
			mBufferThread.Join();
			for (int i = 0; i < 20 && !mStopped; i++) {
				Thread.Sleep(100);
			}
			for (int i = 0; i < mBufferCount; ++i) {
				waveOutUnprepareHeader(mHandle, mpWaveHeader[i], Marshal.SizeOf<WAVEHDR>());
			}
			var ret = waveOutReset(mHandle);
			if (MMRESULT.MMSYSERR_NOERROR != ret) {
				throw new Exception(ret.ToString());
			}
			ret = waveOutClose(mHandle);
			if (MMRESULT.MMSYSERR_NOERROR != ret) {
				throw new Exception(ret.ToString());
			}
			mHandle = IntPtr.Zero;
			DisposeHeader();
		}

		void Callback(IntPtr hwo, MM_WOM uMsg, int dwUser, IntPtr lpWaveHdr, int dwParam2) {
			switch (uMsg) {
			case MM_WOM.OPEN:
				mStopped = false;
				Enabled = true;
				break;
			case MM_WOM.CLOSE:
				mDoStop = false;
				Enabled = false;
				break;
			case MM_WOM.DONE: {
				if (mDoStop) {
					mStopped = true;
					break;
				}
				lock (mLockBuffer) {
					waveOutWrite(mHandle, mpWaveHeader[mReadIndex], Marshal.SizeOf<WAVEHDR>());
					if (0 < mWriteCount) {
						mReadIndex = (mReadIndex + 1) % mBufferCount;
						mWriteCount--;
					}
				}
				break;
			}
			}
		}

		void BufferTask() {
			mWriteCount = 0;
			mWriteIndex = 0;
			mReadIndex = 0;
			while (!mDoStop) {
				var sleep = false;
				lock (mLockBuffer) {
					if (mBufferCount <= mWriteCount + 1) {
						sleep = true;
					} else {
						var pHdr = Marshal.PtrToStructure<WAVEHDR>(mpWaveHeader[mWriteIndex]);
						WriteBuffer(pHdr.lpData);
						mWriteIndex = (mWriteIndex + 1) % mBufferCount;
						mWriteCount++;
					}
				}
				if (sleep) {
					Thread.Sleep(1);
				}
			}
		}

		protected abstract void WriteBuffer(IntPtr pBuffer);
	}
}
