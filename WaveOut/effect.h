#ifndef __HEADER_EFFECT__
#define __HEADER_EFFECT__

/******************************************************************************/
typedef struct SYSVALUE SYSVALUE;

/******************************************************************************/
#pragma pack(push, 8)
typedef struct EFFECT_PARAM {
    double delay_send;
    double delay_time;
    double delay_cross;
    double chorus_send;
    double chorus_rate;
    double chorus_depth;
} EFFECT_PARAM;
#pragma pack(pop)

#pragma pack(push, 4)
typedef struct EFFECT {
    EFFECT_PARAM param;
    SYSVALUE* pSysValue;
    double* p_tap_l;
    double* p_tap_r;
    int write_index;
    double cho_lfo_u;
    double cho_lfo_v;
    double cho_lfo_w;
    double cho_pan_ul;
    double cho_pan_ur;
    double cho_pan_vl;
    double cho_pan_vr;
    double cho_pan_wl;
    double cho_pan_wr;
} EFFECT;
#pragma pack(pop)

/******************************************************************************/
void effect_create(SYSVALUE* pSysValue);
void effect_dispose(SYSVALUE* pSysValue);
extern inline void effect(EFFECT* pEffect, double* pInputL, double* pInputR, double* pOutputL, double* pOutputR);

#endif /* __HEADER_EFFECT__ */
