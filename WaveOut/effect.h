#ifndef __HEADER_EFFECT__
#define __HEADER_EFFECT__

/******************************************************************************/
typedef struct SYSVALUE SYSVALUE;

/******************************************************************************/
typedef struct EFFECT_PARAM {
    double delay_send;
    double delay_time;
    double delay_cross;
    double chorus_send;
    double chorus_rate;
    double chorus_depth;
    double amp;
    double pan_l;
    double pan_r;
    double pitch;
    double cutoff;
    double resonance;
} EFFECT_PARAM;

typedef struct EFFECT_VALUE {
    int write_index;
    double* p_output_l;
    double* p_output_r;
    double* p_tap_l;
    double* p_tap_r;
    double cho_lfo_u;
    double cho_lfo_v;
    double cho_lfo_w;
    double cho_pan_ul;
    double cho_pan_ur;
    double cho_pan_vl;
    double cho_pan_vr;
    double cho_pan_wl;
    double cho_pan_wr;
} EFFECT_VALUE;

typedef struct EFFECT {
    EFFECT_PARAM param;
    EFFECT_VALUE value;
    SYSVALUE* pSysValue;
} EFFECT;

/******************************************************************************/
void effect_create(SYSVALUE* pSysValue);
void effect_dispose(SYSVALUE* pSysValue);
extern inline void effect(EFFECT* pEffect, double inputL, double inputR, double* pOutputL, double* pOutputR);

#endif /* __HEADER_EFFECT__ */
