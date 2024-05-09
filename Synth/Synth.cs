using System;
using System.Runtime.InteropServices;
using SMF;
using static SMF.SMF;

namespace Synth {
	public enum OUTPUT_TYPE {
		I16,
		I24,
		F32
	}

	public class Synth : IDisposable {
		const int CHANNEL_COUNT = 256;
		const int SAMPLER_COUNT = 128;
		const int SPEC_BANK_COUNT = 114;
		const int SPEC_OCT_DIV = 12;
		const double SPEC_BASE_FREQ = 27.5;
		const double SPEC_AMP_MIN = 1e-4; /* -80db */

		class Bank {
			public readonly double KB0;
			public readonly double KA1;
			public readonly double KA2;
			public readonly double FREQ;

			public double Sigma;

			public double Lb1;
			public double Lb2;
			public double La1;
			public double La2;
			public double LPower;

			public double Rb1;
			public double Rb2;
			public double Ra1;
			public double Ra2;
			public double RPower;

			public Bank(int sampleRate, double freq, double width) {
				var omega = 2 * Math.PI * freq / sampleRate;
				var s = Math.Sin(omega);
				var alpha = s * Math.Sinh(Math.Log(2) / 2 * width * omega / s);
				var ka0 = alpha + 1;
				KB0 = alpha / ka0;
				KA1 = 2 * Math.Cos(omega) / ka0;
				KA2 = (1 - alpha) / ka0;
				FREQ = freq;
			}
		}
		Bank[] mBank = new Bank[SPEC_BANK_COUNT];
		Channel[] mChannels = new Channel[CHANNEL_COUNT];
		Sampler[] mSamplers = new Sampler[SAMPLER_COUNT];

		int mSampleRate;
		double mDeltaTime;
		int mBufferSamples;
		IntPtr mOutputBuffer;
		IntPtr mSpecBuffer;
		double[] mMuteData;

		delegate void DWrite(IntPtr output);
		DWrite mfpWrite = (p) => { };

		public double[] SpecL { get; private set; }
		public double[] SpecR { get; private set; }

		struct Int24 {
			public byte lsb;
			public ushort msb;
		}

		public void Dispose() {
			Marshal.FreeHGlobal(mOutputBuffer);
			Marshal.FreeHGlobal(mSpecBuffer);
		}

		public void Setup(int sampleRate, int bufferSamples, OUTPUT_TYPE outputType) {
			switch (outputType) {
			case OUTPUT_TYPE.I16:
				mfpWrite = WriteI16;
				break;
			case OUTPUT_TYPE.I24:
				mfpWrite = WriteI24;
				break;
			case OUTPUT_TYPE.F32:
				mfpWrite = WriteF32;
				break;
			default:
				mfpWrite = (p) => { };
				return;
			}
			mSampleRate = sampleRate;
			mDeltaTime = 1.0 / sampleRate;
			mBufferSamples = bufferSamples;

			for (int i = 0; i < CHANNEL_COUNT; i++) {
				if (null == mChannels[i]) {
					mChannels[i] = new Channel(i);
				}
				mChannels[i].Setup(sampleRate, bufferSamples);
			}
			for (int i = 0; i < SAMPLER_COUNT; i++) {
				mSamplers[i] = new Sampler();
			}

			Marshal.FreeHGlobal(mOutputBuffer);
			Marshal.FreeHGlobal(mSpecBuffer);
			mOutputBuffer = Marshal.AllocHGlobal(sizeof(double) * bufferSamples * 2);
			mSpecBuffer = Marshal.AllocHGlobal(sizeof(double) * bufferSamples * 2);
			mMuteData = new double[bufferSamples * 2];

			SpecL = new double[SPEC_BANK_COUNT];
			SpecR = new double[SPEC_BANK_COUNT];
			for (int b = 0; b < SPEC_BANK_COUNT; ++b) {
				var freq = SPEC_BASE_FREQ * Math.Pow(2, (double)b / SPEC_OCT_DIV);
				mBank[b] = new Bank(sampleRate, freq, 1.0 / SPEC_OCT_DIV);
			}
			SetSpectrumSpeed(20);
		}

		public Sampler.ID GetSamplerId(int index) {
			return mSamplers[index].Id;
		}

		public Channel GetChannel(int index) {
			return mChannels[index];
		}

		public void SetSpectrumSpeed(double speed) {
			for (int b = 0; b < SPEC_BANK_COUNT; ++b) {
				var bank = mBank[b];
				if (bank.FREQ < speed) {
					bank.Sigma = bank.FREQ / mSampleRate;
				}
				else {
					bank.Sigma = speed / mSampleRate;
				}
			}
		}

		public void WriteBuffer(IntPtr output) {
			for (int i = 0; i < SAMPLER_COUNT; i++) {
				if (Sampler.STATE.STANDBY != mSamplers[i].Id.State) {
					mSamplers[i].WriteBuffer();
				}
			}
			Marshal.Copy(mMuteData, 0, mSpecBuffer, mMuteData.Length);
			for (int i = 0; i < CHANNEL_COUNT; i++) {
				if (Channel.STATE.STANDBY != mChannels[i].State) {
					mChannels[i].WriteBuffer(mOutputBuffer, mSpecBuffer);
				}
			}
			CalcSpectrum();
			mfpWrite(output);
		}

		public void SendMessage(Event ev) {
			if (ev is Note note) {
				var channel = mChannels[ev.Channel];
				if (note.IsNoteOn) {
					if (!channel.Enable) {
						return;
					}
					for (int r = 0; r < channel.RegionCount; r++) {
						var region = channel.Regions[r];
						if (note.Tone < region.KeyLow || region.KeyHigh < note.Tone ||
							note.Velocity < region.VeloLow || region.VeloHigh < note.Velocity
						) {
							continue;
						}
						for (int i = 0; i < SAMPLER_COUNT; i++) {
							if (mSamplers[i].NoteOn(channel, region, note.Tone, note.Velocity)) {
								channel.NoteOn();
								break;
							}
						}
					}
				} else {
					for (int i = 0; i < SAMPLER_COUNT; i++) {
						mSamplers[i].NoteOff(channel, note.Tone);
					}
				}
			}
			if (ev is CustomCtrl ctrl) {
				var channel = mChannels[ev.Channel];
				channel.CtrlChange(ctrl.CtrlType, ctrl.Value);
				switch (ctrl.CtrlType) {
				case CTRL.HOLD1:
					if (ctrl.Value == 0) {
						for (int i = 0; i < SAMPLER_COUNT; i++) {
							mSamplers[i].HoldOff(channel);
						}
					}
					break;
				case CTRL.ALL_SOUND_OFF:
				case CTRL.ALL_NOTE_OFF:
					for (int i = 0; i < SAMPLER_COUNT; i++) {
						mSamplers[i].PurgeChannelNotes(channel);
					}
					break;
				case CTRL.LOCAL_CTRL:
					if (ctrl.Value == 0) {
						for (int i = 0; i < SAMPLER_COUNT; i++) {
							mSamplers[i].PurgeChannelNotes(channel);
						}
					}
					break;
				}
			}
			if (ev is Prog prog) {
				mChannels[ev.Channel].ProgChange(prog.Number);
			}
			if (ev is Pitch pitch) {
				mChannels[ev.Channel].PitchBend(pitch.Value);
			}
		}

		unsafe void WriteI16(IntPtr output) {
			var pInput = (double*)mOutputBuffer;
			var pOutput = (short*)output;
			for (int s = 0; s < mBufferSamples; ++s) {
				var l = *pInput;
				*pInput++ = 0;
				var r = *pInput;
				*pInput++ = 0;
				l = (l < -1) ? -1 : (l > 1) ? 1 : l;
				r = (r < -1) ? -1 : (r > 1) ? 1 : r;
				*pOutput++ = (short)(0x7FFF * l);
				*pOutput++ = (short)(0x7FFF * r);
			}
		}

		unsafe void WriteI24(IntPtr output) {
			var pInput = (double*)mOutputBuffer;
			var pOutput = (Int24*)output;
			for (int s = 0; s < mBufferSamples; ++s) {
				var l = *pInput;
				*pInput++ = 0;
				var r = *pInput;
				*pInput++ = 0;
				l = (l < -1) ? -1 : (l > 1) ? 1 : l;
				r = (r < -1) ? -1 : (r > 1) ? 1 : r;
				var ul = (uint)(0x7FFFFFFF * l);
				var ur = (uint)(0x7FFFFFFF * r);
				pOutput->lsb = (byte)((ul >> 8) & 0xFF);
				pOutput->msb = (ushort)(ul >> 16);
				pOutput++;
				pOutput->lsb = (byte)((ur >> 8) & 0xFF);
				pOutput->msb = (ushort)(ur >> 16);
				pOutput++;
			}
		}

		unsafe void WriteF32(IntPtr output) {
			var pInput = (double*)mOutputBuffer;
			var pOutput = (float*)output;
			for (int s = 0; s < mBufferSamples; ++s) {
				*pOutput++ = (float)*pInput;
				*pInput++ = 0;
				*pOutput++ = (float)*pInput;
				*pInput++ = 0;
			}
		}

		unsafe void CalcSpectrum() {
			for (int b = 0; b < SPEC_BANK_COUNT; ++b) {
				var pInput = (double*)mSpecBuffer;
				var bank = mBank[b];
				for (int s = 0; s < mBufferSamples; ++s) {
					var lb0 = *pInput++;
					var la0
						= bank.KB0 * lb0
						- bank.KB0 * bank.Lb2
						- bank.KA1 * bank.La1
						- bank.KA2 * bank.La2;
					bank.Lb2 = bank.Lb1;
					bank.Lb1 = lb0;
					bank.La2 = bank.La1;
					bank.La1 = la0;
					bank.LPower += (la0 * la0 - bank.LPower) * bank.Sigma;
					var rb0 = *pInput++;
					var ra0
						= bank.KB0 * rb0
						- bank.KB0 * bank.Rb2
						- bank.KA1 * bank.Ra1
						- bank.KA2 * bank.Ra2;
					bank.Rb2 = bank.Lb1;
					bank.Rb1 = rb0;
					bank.Ra2 = bank.La1;
					bank.Ra1 = ra0;
					bank.RPower += (ra0 * ra0 - bank.RPower) * bank.Sigma;
				}
				var lAmp = Math.Sqrt(mBank[b].LPower);
				var rAmp = Math.Sqrt(mBank[b].RPower);
				SpecL[b] = 20 * Math.Log10((lAmp < SPEC_AMP_MIN) ? SPEC_AMP_MIN : lAmp);
				SpecR[b] = 20 * Math.Log10((rAmp < SPEC_AMP_MIN) ? SPEC_AMP_MIN : rAmp);
			}
		}
	}
}
