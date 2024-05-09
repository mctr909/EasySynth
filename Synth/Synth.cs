using System;

namespace Synth {
	public enum BUFFER_TYPE {
		I16,
		I24,
		I32,
		F32
	}

	public class Synth {
		public const int CHANNEL_COUNT = 256;
		public const int SAMPLER_COUNT = 128;

		public Spectrum Spectrum;

		Channel[] mChannels = new Channel[CHANNEL_COUNT];
		Sampler[] mSamplers = new Sampler[SAMPLER_COUNT];

		double mDeltaTime;
		int mBufferSamples;
		double[] mOutputBuffer;
		double[] mSpecBuffer;

		delegate void DWrite(IntPtr pBuffer);
		DWrite mfpWrite;

		struct Int24 {
			public byte lsb;
			public ushort msb;
		}

		public Synth() {
			mfpWrite = WriteNone;
		}

		public void Setup(int sampleRate, int bufferSamples, BUFFER_TYPE bufferType) {
			switch (bufferType) {
			case BUFFER_TYPE.I16:
				mfpWrite = Write16i;
				break;
			case BUFFER_TYPE.I24:
				mfpWrite = Write24i;
				break;
			case BUFFER_TYPE.I32:
				mfpWrite = Write32i;
				break;
			case BUFFER_TYPE.F32:
				mfpWrite = Write32f;
				break;
			default:
				mfpWrite = WriteNone;
				return;
			}
			mDeltaTime = 1.0 / sampleRate;
			mBufferSamples = bufferSamples;
			mOutputBuffer = new double[bufferSamples * 2];
			mSpecBuffer = new double[bufferSamples * 2];
			for (int i = 0; i < CHANNEL_COUNT; i++) {
				if (null == mChannels[i]) {
					mChannels[i] = new Channel(i);
				}
				mChannels[i].Setup(sampleRate, bufferSamples);
			}
			for (int i = 0; i < SAMPLER_COUNT; i++) {
				mSamplers[i] = new Sampler();
			}
			Spectrum = new Spectrum(sampleRate, 114, 12, 27.5);
		}

		public void SetWaveTable(int channelNum, IntPtr pWaveTable) {
			mChannels[channelNum].SetWaveTable(pWaveTable);
		}

		public Sampler.ID GetSamplerId(int index) {
			return mSamplers[index].Id;
		}

		public Channel GetChannel(int index) {
			return mChannels[index];
		}

		public void WriteBuffer(IntPtr pBuffer) {
			for (int i = 0; i < SAMPLER_COUNT; i++) {
				if (Sampler.STATE.STANDBY != mSamplers[i].Id.State) {
					mSamplers[i].WriteBuffer();
				}
			}
			for (int i = 0; i < CHANNEL_COUNT; i++) {
				if (Channel.STATE.STANDBY != mChannels[i].State) {
					mChannels[i].WriteBuffer(mOutputBuffer, mSpecBuffer);
				}
			}
			Spectrum.Update(mSpecBuffer);
			Array.Clear(mSpecBuffer, 0, mSpecBuffer.Length);
			mfpWrite(pBuffer);
		}

		public void SendMessage(Event message) {
			if (message.Type == Event.TYPE.META) {
				return;
			}
			var channel = mChannels[message.Channel];
			switch (message.Type) {
			case Event.TYPE.NOTE_OFF:
				for (int i = 0; i < SAMPLER_COUNT; i++) {
					mSamplers[i].NoteOff(channel, message.Tone);
				}
				break;
			case Event.TYPE.NOTE_ON:
				if (channel.Mute) {
					break;
				}
				for (int r = 0; r < channel.RegionCount; r++) {
					var region = channel.Regions[r];
					if (message.Tone < region.KeyLow || region.KeyHigh < message.Tone ||
						message.Value < region.VeloLow || region.VeloHigh < message.Value
					) {
						continue;
					}
					for (int i = 0; i < SAMPLER_COUNT; i++) {
						if (mSamplers[i].NoteOn(channel, region, message.Tone, (int)message.Value)) {
							channel.NoteOn();
							break;
						}
					}
				}
				break;
			case Event.TYPE.CTRL_CHG:
				channel.CtrlChange(message.CtrlType, message.Value);
				switch (message.CtrlType) {
				case Event.CTRL.HOLD:
					if (message.Value == 0) {
						for (int i = 0; i < SAMPLER_COUNT; i++) {
							mSamplers[i].HoldOff(channel);
						}
					}
					break;
				case Event.CTRL.ALL_SOUND_OFF:
				case Event.CTRL.ALL_NOTE_OFF:
					for (int i = 0; i < SAMPLER_COUNT; i++) {
						mSamplers[i].PurgeChannelNotes(channel);
					}
					break;
				case Event.CTRL.MUTE:
					if (message.Value > 0) {
						for (int i = 0; i < SAMPLER_COUNT; i++) {
							mSamplers[i].PurgeChannelNotes(channel);
						}
					}
					break;
				}
				break;
			default:
				break;
			}
		}

		void WriteNone(IntPtr pBuffer) { }

		unsafe void Write16i(IntPtr pBuffer) {
			var buffer = (short*)pBuffer;
			for (int i = 0, j = 0; i < mBufferSamples; i++, j += 2) {
				var l = mOutputBuffer[j];
				mOutputBuffer[j] = 0;
				var r = mOutputBuffer[j + 1];
				mOutputBuffer[j + 1] = 0;
				l = (l < -1) ? -1 : (l > 1) ? 1 : l;
				r = (r < -1) ? -1 : (r > 1) ? 1 : r;
				buffer[j] = (short)(0x7FFF * l);
				buffer[j + 1] = (short)(0x7FFF * r);
			}
		}

		unsafe void Write24i(IntPtr pBuffer) {
			var buffer = (Int24*)pBuffer;
			for (int i = 0, j = 0; i < mBufferSamples; i++, j += 2) {
				var l = mOutputBuffer[j];
				mOutputBuffer[j] = 0;
				var r = mOutputBuffer[j + 1];
				mOutputBuffer[j + 1] = 0;
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
				var l = mOutputBuffer[j];
				mOutputBuffer[j] = 0;
				var r = mOutputBuffer[j + 1];
				mOutputBuffer[j + 1] = 0;
				l = (l < -1) ? -1 : (l > 1) ? 1 : l;
				r = (r < -1) ? -1 : (r > 1) ? 1 : r;
				buffer[j] = (int)(0x7FFFFFFF * l);
				buffer[j + 1] = (int)(0x7FFFFFFF * r);
			}
		}

		unsafe void Write32f(IntPtr pBuffer) {
			var buffer = (float*)pBuffer;
			for (int i = 0, j = 0; i < mBufferSamples; i++, j += 2) {
				buffer[j] = (float)mOutputBuffer[j];
				buffer[j + 1] = (float)mOutputBuffer[j + 1];
				mOutputBuffer[j + 1] = 0;
				mOutputBuffer[j + 1] = 0;
			}
		}
	}
}
