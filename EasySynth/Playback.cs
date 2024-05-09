using System;
using Synth;
using WinMM;

namespace EasySynth {
	class Playback : WaveOut {
		static Playback mInstance = null;
		static Synth.Synth mSynth = new Synth.Synth();

		Playback(int sampleRate, int bufferSamples) :
			base(sampleRate, 2, BUFFER_TYPE.F32, bufferSamples, 8192 / bufferSamples) { }

		public static void Open(int sampleRate, int bufferSamples) {
			mInstance = new Playback(sampleRate, bufferSamples);
			mSynth.Setup(sampleRate, bufferSamples, Synth.BUFFER_TYPE.F32);
			mInstance.WaveOpen();
		}

		public static void Close() {
			mInstance.WaveClose();
		}

		public static void SendMessage(Synth.Event message) {
			mSynth.SendMessage(message);
		}

		public static Spectrum GetSpectrum() {
			return mSynth.Spectrum;
		}

		protected override void WriteBuffer(IntPtr pBuffer) {
			mSynth.WriteBuffer(pBuffer);
		}
	}
}
