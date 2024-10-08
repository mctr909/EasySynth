namespace Synth.Instruments {
	public enum EART : uint {
		GAIN,
		PAN,
		PITCH,

		VIBRATO = 0x10,
		VIBRATO_DEPTH,
		VIBRATO_RATE,
		VIBRATO_DELAY,

		MODURATION = 0x20,
		MODURATION_DEPTH,
		MODURATION_RATE,
		MODURATION_DELAY,
		MODURATION_OFFSET,

		EG_AMP = 0x40,
		EG_AMP_ATTACK,
		EG_AMP_DECAY,
		EG_AMP_RELEACE,
		EG_AMP_SUSTAIN,

		EG_PITCH = 0x80,
		EG_PITCH_ATTACK,
		EG_PITCH_DECAY,
		EG_PITCH_RELEACE,
		EG_PITCH_RISE,
		EG_PITCH_LEVEL,
		EG_PITCH_FALL,

		EG_LPF = 0x100,
		EG_LPF_ATTACK,
		EG_LPF_DECAY,
		EG_LPF_RELEACE,
		EG_LPF_RISE,
		EG_LPF_LEVEL,
		EG_LPF_SUSTAIN,
		EG_LPF_FALL,
		EG_LPF_RESONANCE,

		FM = 0xF00,
		FM_SRC_INDEX,
		FM_SRC_GAIN,
		FM_SRC_OFFSET,

		FM_GAIN = 0xF10,
		FM_GAIN_ATTACK,
		FM_GAIN_DECAY,
		FM_GAIN_HOLD,
		FM_GAIN_RELEACE,
		FM_GAIN_RISE,
		FM_GAIN_LEVEL,
		FM_GAIN_SUSTAIN,
		FM_GAIN_FALL,

		FM_RATE = 0xF20,
		FM_RATE_ATTACK,
		FM_RATE_DECAY,
		FM_RATE_HOLD,
		FM_RATE_RELEACE,
		FM_RATE_RISE,
		FM_RATE_LEVEL,
		FM_RATE_SUSTAIN,
		FM_RATE_FALL,

		INST = 0x8000,
		INST_GAIN  = INST | GAIN,
		INST_PAN   = INST | PAN,
		INST_PITCH = INST | PITCH,

		INST_VIBRATO_DEPTH = INST | VIBRATO_DEPTH,
		INST_VIBRATO_RATE  = INST | VIBRATO_RATE,
		INST_VIBRATO_DELAY = INST | VIBRATO_DELAY,

		INST_MODURATION_DEPTH  = INST | MODURATION_DEPTH,
		INST_MODURATION_RATE   = INST | MODURATION_RATE,
		INST_MODURATION_DELAY  = INST | MODURATION_DELAY,
		INST_MODURATION_OFFSET = INST | MODURATION_OFFSET,

		INST_EG_AMP_ATTACK  = INST | EG_AMP_ATTACK,
		INST_EG_AMP_DECAY   = INST | EG_AMP_DECAY,
		INST_EG_AMP_RELEACE = INST | EG_AMP_RELEACE,
		INST_EG_AMP_SUSTAIN = INST | EG_AMP_SUSTAIN,

		INST_EG_PITCH_ATTACK  = INST | EG_PITCH_ATTACK,
		INST_EG_PITCH_DECAY   = INST | EG_PITCH_DECAY,
		INST_EG_PITCH_RELEACE = INST | EG_PITCH_RELEACE,
		INST_EG_PITCH_RISE    = INST | EG_PITCH_RISE,
		INST_EG_PITCH_LEVEL   = INST | EG_PITCH_LEVEL,
		INST_EG_PITCH_FALL    = INST | EG_PITCH_FALL,

		INST_EG_LPF_ATTACK    = INST | EG_LPF_ATTACK,
		INST_EG_LPF_DECAY     = INST | EG_LPF_DECAY,
		INST_EG_LPF_RELEACE   = INST | EG_LPF_RELEACE,
		INST_EG_LPF_RISE      = INST | EG_LPF_RISE,
		INST_EG_LPF_LEVEL     = INST | EG_LPF_LEVEL,
		INST_EG_LPF_SUSTAIN   = INST | EG_LPF_SUSTAIN,
		INST_EG_LPF_FALL      = INST | EG_LPF_FALL,
		INST_EG_LPF_RESONANCE = INST | EG_LPF_RESONANCE
	}

	public struct CK_ART {
		public EART Type;
		public float Value;
	}
}
