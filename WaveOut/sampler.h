#ifndef __HEADER_SAMPLER__
#define __HEADER_SAMPLER__

#include "type.h"
#include "effect.h"

enum struct E_SAMPLER_STATE : byte {
    FREE,
    RESERVED,
    PRESS,
    RELEASE,
    HOLD,
    PURGE
};

typedef struct OSC {
    double output;
    double width;
    double count;
    double delta;
    double gain;
} OSC;

typedef struct ENVELOPE_AMP {
    double attack;
    double decay;
    double release;
    double sustain;
} ENVELOPE_AMP;

typedef struct ENVELOPE_PITCH {
    double attack;
    double release;
    double rise;
    double fall;
} ENVELOPE_PITCH;

typedef struct ENVELOPE_CUTOFF {
    double attack;
    double decay;
    double release;
    double rise;
    double top;
    double sustain;
    double fall;
} ENVELOPE_CUTOFF;

typedef struct ENVELOPE {
    ENVELOPE_AMP amp;
    ENVELOPE_PITCH pitch;
    ENVELOPE_CUTOFF cutoff;
} ENVELOPE;

typedef struct SAMPLER {
    short *pWave;
    ENVELOPE *pEnv;
    SYSVALUE *pSysValue;
    uint loop_begin;
    uint loop_length;
    byte loop_enable;
    E_SAMPLER_STATE status;
    ushort reserved;
    double time;
    double index;
    double pitch;
    double gain;
    double env_amp;
    double env_cutoff;
    double env_pitch;
} SAMPLER;

#endif /* __HEADER_SAMPLER__ */