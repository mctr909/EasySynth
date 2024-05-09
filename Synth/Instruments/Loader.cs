using System;

namespace Synth.Instruments {
	public class Loader {
		IntPtr mpPreset = IntPtr.Zero;
		IntPtr mpLayer = IntPtr.Zero;
		IntPtr mpArt = IntPtr.Zero;
		IntPtr mpWaveInfo = IntPtr.Zero;
		IntPtr mpWaveData = IntPtr.Zero;

		Loader() { }

		public Loader(string path) { }

		public unsafe void GetPreset(ref CK_PRESET preset, int progNum, int bankMSB=0, int bankLSB=0, bool isTonal=false) {
		}

		public unsafe void GetLayer(ref CK_LAYER layer, CK_PRESET preset, int keyLow, int keyHigh, int veloLow=0, int veloHigh=127) {
		}

		public unsafe void GetArt(ref CK_ART art, CK_LAYER layer, int index) {
		}

		public unsafe void GetWave(ref CK_WAVE waveInfo, short[] waveData, int index) {
		}
	}
}
