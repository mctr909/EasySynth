using System.Runtime.InteropServices;

namespace Synth.Instruments {
	struct ENV_AMP {
		public double Attack;
		public double Decay;
		public double Release;
		public double Hold;
		public double Sustain;
	}

	struct ENV_LPF {
		public double Attack;
		public double Decay;
		public double Rise;
		public double Level;
		public double Sustain;
		public double Fall;
		public double Resonance;
	}

	struct ENV_PITCH {
		public double Attack;
		public double Decay;
		public double Rise;
		public double Level;
		public double Fall;
	}

	struct WAVE_INFO {
		public uint Offset;
		public uint SampleRate;
		public uint LoopBegin;
		public uint LoopLength;
		public bool LoopEnable;
		public byte UnityNote;
		public double Gain;
		public double Pitch;
	}

	struct WAVE_TYPE {
		public const uint PCM_MAX = 0xFFFFFEFF;
		public const uint FM = 0xFFFFFF00;
		public const uint SAW = 0xFFFFFF80;
		public const uint PWM = 0xFFFFFF81;
	}

	struct REGION {
		public byte KeyLow;
		public byte KeyHigh;
		public byte VeloLow;
		public byte VeloHigh;
		public uint WaveIndex;
		public double Gain;
		public double Pitch;
		public double PanL;
		public double PanR;
		public ENV_AMP EnvAmp;
		public ENV_LPF EnvLpf;
		public ENV_PITCH EnvPitch;
	}

	struct INST {
		public byte IsDrum;
		public byte BankMsb;
		public byte BankLsb;
		public byte ProgNum;
		public uint RegionIndex;
		public uint RegionCount;
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 36)]
		public byte[] Name;
	}
}
