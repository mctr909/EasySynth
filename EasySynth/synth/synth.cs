using System;
using SMF;
using WINMM;

namespace Synth {
	public enum NRPN_TYPE : ushort {
		NULL = 0x7F7F
	}

	public class Synth {
		public const int CHANNEL_COUNT = 256;
		public const int SAMPLER_COUNT = 128;

		public Spectrum Spectrum;

		Channel[] mChannels = new Channel[CHANNEL_COUNT];
		Sampler[] mSamplers = new Sampler[SAMPLER_COUNT];

		double mDeltaTime;
		int mBufferSamples;
		double[] mBuffer;
		double[] mSpecInput;

		delegate void DWrite(IntPtr pBuffer);
		DWrite mfpWrite;

		struct int24 {
			public byte lsb;
			public ushort msb;
		}

		public Synth() {
			mfpWrite = WriteNone;
		}

		public void Setup(int sampleRate, int bufferSamples, WaveLib.BUFFER_TYPE waveoutType) {
			mDeltaTime = 1.0 / sampleRate;
			mBufferSamples = bufferSamples;
			switch (waveoutType) {
			case WaveLib.BUFFER_TYPE.I16:
				mfpWrite = Write16i;
				break;
			case WaveLib.BUFFER_TYPE.I24:
				mfpWrite = Write24i;
				break;
			case WaveLib.BUFFER_TYPE.I32:
				mfpWrite = Write32i;
				break;
			case WaveLib.BUFFER_TYPE.F32:
				mfpWrite = Write32f;
				break;
			default:
				mfpWrite = WriteNone;
				return;
			}
			mBuffer = new double[bufferSamples * 2];
			mSpecInput = new double[bufferSamples * 2];
			for (int i = 0; i < CHANNEL_COUNT; i++) {
				if (null == mChannels[i]) {
					mChannels[i] = new Channel(i);
				}
				mChannels[i].Setup(sampleRate, bufferSamples);
			}
			for (int i = 0; i < SAMPLER_COUNT; i++) {
				mSamplers[i] = new Sampler();
			}
			Spectrum = new Spectrum(sampleRate, 114, 12, 27.5, 1);
		}

		public void SetWaveTable(int channelNum, IntPtr pWaveTable) {
			mChannels[channelNum].SetWaveTable(pWaveTable);
		}

		public SamplerId GetSamplerId(int index) {
			return mSamplers[index].Id;
		}

		public Channel GetChannel(int index) {
			return mChannels[index];
		}

		public void WriteBuffer(IntPtr pBuffer) {
			for (int i = 0; i < SAMPLER_COUNT; i++) {
				if (SamplerState.STANDBY != mSamplers[i].Id.State) {
					mSamplers[i].WriteBuffer();
				}
			}
			for (int i = 0; i < CHANNEL_COUNT; i++) {
				if (Channel.STATE.STANDBY != mChannels[i].State) {
					mChannels[i].WriteBuffer(mBuffer, mSpecInput);
				}
			}
			Spectrum.Update(mSpecInput);
			Array.Clear(mSpecInput, 0, mSpecInput.Length);
			mfpWrite(pBuffer);
		}

		public void SendMessage(byte[] pData) {
			var messageType = (MSG_TYPE)pData[0];
			if (messageType >= MSG_TYPE.SYS_EX) {
				return;
			}
			byte chNum = pData[1];
			byte note, velo;
			byte value;
			var ch = mChannels[chNum];
			CTRL_TYPE ctrlType;
			switch (messageType) {
			case MSG_TYPE.NOTE_OFF:
				note = pData[2];
				for (int i = 0; i < SAMPLER_COUNT; i++) {
					mSamplers[i].NoteOff(ch, note);
				}
				break;
			case MSG_TYPE.NOTE_ON:
				if (ch.Mute) {
					break;
				}
				note = pData[2];
				velo = pData[3];
				if (velo == 0) {
					for (int i = 0; i < SAMPLER_COUNT; i++) {
						mSamplers[i].NoteOff(ch, note);
					}
					break;
				}
				for (int r = 0; r < ch.RegionCount; r++) {
					var region = ch.Regions[r];
					if (note < region.KeyLow || region.KeyHigh < note ||
						velo < region.VeloLow || region.VeloHigh < velo
					) {
						continue;
					}
					for (int i = 0; i < SAMPLER_COUNT; i++) {
						if (mSamplers[i].NoteOn(ch, region, note, velo)) {
							ch.NoteOn();
							break;
						}
					}
				}
				break;
			case MSG_TYPE.CTRL_CHG:
				ctrlType = (CTRL_TYPE)pData[2];
				value = pData[3];
				ch.CtrlChange(ctrlType, value);
				switch (ctrlType) {
				case CTRL_TYPE.HOLD:
					if (value < 64) {
						for (int i = 0; i < SAMPLER_COUNT; i++) {
							mSamplers[i].HoldOff(ch);
						}
					}
					break;
				case CTRL_TYPE.ALL_SOUND_OFF:
				case CTRL_TYPE.ALL_NOTE_OFF:
				case CTRL_TYPE.DISABLE: {
					for (int i = 0; i < SAMPLER_COUNT; i++) {
						mSamplers[i].ChannelNoteOff(ch);
					}
					break;
				}
				}
				break;
			case MSG_TYPE.PROG_CHG:
				mChannels[chNum].ProgChange(pData[2]);
				break;
			case MSG_TYPE.PITCH:
				mChannels[chNum].PitchBend(pData[2], pData[3]);
				break;
			default:
				break;
			}
		}

		void WriteNone(IntPtr pBuffer) { }

		unsafe void Write16i(IntPtr pBuffer) {
			var buffer = (short*)pBuffer;
			for (int i = 0, j = 0; i < mBufferSamples; i++, j += 2) {
				var l = mBuffer[j];
				mBuffer[j] = 0;
				var r = mBuffer[j + 1];
				mBuffer[j + 1] = 0;
				l = (l < -1) ? -1 : (l > 1) ? 1 : l;
				r = (r < -1) ? -1 : (r > 1) ? 1 : r;
				buffer[j] = (short)(0x7FFF * l);
				buffer[j + 1] = (short)(0x7FFF * r);
			}
		}

		unsafe void Write24i(IntPtr pBuffer) {
			var buffer = (int24*)pBuffer;
			for (int i = 0, j = 0; i < mBufferSamples; i++, j += 2) {
				var l = mBuffer[j];
				mBuffer[j] = 0;
				var r = mBuffer[j + 1];
				mBuffer[j + 1] = 0;
				l = (l < -1) ? -1 : (l > 1) ? 1 : l;
				r = (r < -1) ? -1 : (r > 1) ? 1 : r;
				var ul = (uint)(0x7FFFFFFF * l);
				var ur = (uint)(0x7FFFFFFF * r);
				buffer[j].lsb = (byte)((ul >> 8) & 0xFF);
				buffer[j].msb = (ushort)(ul >> 16);
				buffer[j + 1].lsb = (byte)((ur >> 8) & 0xFF);
				buffer[j + 1].msb = (ushort)(ur >> 16);
			}
		}

		unsafe void Write32i(IntPtr pBuffer) {
			var buffer = (int*)pBuffer;
			for (int i = 0, j = 0; i < mBufferSamples; i++, j += 2) {
				var l = mBuffer[j];
				mBuffer[j] = 0;
				var r = mBuffer[j + 1];
				mBuffer[j + 1] = 0;
				l = (l < -1) ? -1 : (l > 1) ? 1 : l;
				r = (r < -1) ? -1 : (r > 1) ? 1 : r;
				buffer[j] = (int)(0x7FFFFFFF * l);
				buffer[j + 1] = (int)(0x7FFFFFFF * r);
			}
		}

		unsafe void Write32f(IntPtr pBuffer) {
			var buffer = (float*)pBuffer;
			for (int i = 0, j = 0; i < mBufferSamples; i++, j += 2) {
				buffer[j] = (float)mBuffer[j];
				buffer[j + 1] = (float)mBuffer[j + 1];
				mBuffer[j + 1] = 0;
				mBuffer[j + 1] = 0;
			}
		}
	}
}
