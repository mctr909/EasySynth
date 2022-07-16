#include "type.h"
#include "effect.h"
#include <math.h>
#include <string.h>
#include <stdlib.h>

/******************************************************************************/
#define TAP_LENGTH pSysValue->buffer_length
#define PI        3.14159265
#define PI2       6.28318531
#define INV_SQRT3 0.577350269

/******************************************************************************/
void effect_create(SYSVALUE* pSysValue) {
    pSysValue->pp_effect = (EFFECT**)calloc(CHANNEL_COUNT, sizeof(EFFECT*));
    for (int i = 0; i < CHANNEL_COUNT; i++) {
        pSysValue->pp_effect[i] = (EFFECT*)calloc(1, sizeof(EFFECT));
        auto pEffect = pSysValue->pp_effect[i];
        pEffect->pSysValue = pSysValue;

        // allocate delay taps
        pEffect->write_index = 0;
        pEffect->p_tap_l = (double*)malloc(sizeof(double) * TAP_LENGTH);
        pEffect->p_tap_r = (double*)malloc(sizeof(double) * TAP_LENGTH);
        memset(pEffect->p_tap_l, 0, sizeof(double) * TAP_LENGTH);
        memset(pEffect->p_tap_r, 0, sizeof(double) * TAP_LENGTH);

        // initialize chorus
        pEffect->cho_lfo_u = 0.505 + 0.495;
        pEffect->cho_lfo_v = 0.505 + 0.495 * -0.5;
        pEffect->cho_lfo_w = 0.505 + 0.495 * -0.5;
        pEffect->cho_pan_ul = cos(0.5 * PI * 0 / 3.0);
        pEffect->cho_pan_ur = sin(0.5 * PI * 0 / 3.0);
        pEffect->cho_pan_vl = cos(0.5 * PI * 1 / 3.0);
        pEffect->cho_pan_vr = sin(0.5 * PI * 1 / 3.0);
        pEffect->cho_pan_wl = cos(0.5 * PI * 2 / 3.0);
        pEffect->cho_pan_wr = sin(0.5 * PI * 2 / 3.0);
    }
}

void effect_dispose(SYSVALUE* pSysValue) {
    if (NULL == pSysValue->pp_effect) {
        return;
    }
    for (int i = 0; i < CHANNEL_COUNT; i++) {
        free(pSysValue->pp_effect[i]->p_tap_l);
        free(pSysValue->pp_effect[i]->p_tap_r);
        free(pSysValue->pp_effect[i]);
    }
    free(pSysValue->pp_effect);
    pSysValue->pp_effect = NULL;
}

inline void effect(EFFECT* pEffect, double *pInputL, double *pInputR, double* pOutputL, double* pOutputR) {
    auto pSysValue = pEffect->pSysValue;
    auto pParam = &pEffect->param;
    auto pDelayTapL = pEffect->p_tap_l;
    auto pDelayTapR = pEffect->p_tap_r;

    *pOutputL = *pInputL;
    *pOutputR = *pInputR;

    pEffect->write_index++;
    if (TAP_LENGTH <= pEffect->write_index) {
        pEffect->write_index = 0;
    }

    /*** output delay ***/
    {
        int delayIndex = pEffect->write_index - (int)(pSysValue->sample_rate * pParam->delay_time);
        if (delayIndex < 0) {
            delayIndex += TAP_LENGTH;
        }
        double delayL = pParam->delay_send * pDelayTapL[delayIndex];
        double delayR = pParam->delay_send * pDelayTapR[delayIndex];
        *pOutputL += (delayL * (1.0 - pParam->delay_cross) + delayR * pParam->delay_cross);
        *pOutputR += (delayR * (1.0 - pParam->delay_cross) + delayL * pParam->delay_cross);
        pDelayTapL[pEffect->write_index] = *pOutputL;
        pDelayTapR[pEffect->write_index] = *pOutputR;
    }

    /*** output chorus ***/
    {
        double depth = pSysValue->sample_rate * pParam->chorus_depth;
        double posU = pEffect->write_index - pEffect->cho_lfo_u * depth;
        double posV = pEffect->write_index - pEffect->cho_lfo_v * depth;
        double posW = pEffect->write_index - pEffect->cho_lfo_w * depth;
        int idxU = (int)posU;
        int idxV = (int)posV;
        int idxW = (int)posW;
        double dtU = posU - idxU;
        double dtV = posV - idxV;
        double dtW = posW - idxW;
        if (idxU < 0) {
            idxU += TAP_LENGTH;
        }
        if (TAP_LENGTH <= idxU) {
            idxU -= TAP_LENGTH;
        }
        if (idxV < 0) {
            idxV += TAP_LENGTH;
        }
        if (TAP_LENGTH <= idxV) {
            idxV -= TAP_LENGTH;
        }
        if (idxW < 0) {
            idxW += TAP_LENGTH;
        }
        if (TAP_LENGTH <= idxW) {
            idxW -= TAP_LENGTH;
        }
        double chorusL = 0.0;
        double chorusR = 0.0;
        if (idxU == 0) {
            chorusL += pEffect->cho_pan_ul * (pDelayTapL[TAP_LENGTH - 1] * (1.0 - dtU) + pDelayTapL[idxU] * dtU);
            chorusR += pEffect->cho_pan_ur * (pDelayTapR[TAP_LENGTH - 1] * (1.0 - dtU) + pDelayTapR[idxU] * dtU);
        } else {
            chorusL += pEffect->cho_pan_ul * (pDelayTapL[idxU - 1] * (1.0 - dtU) + pDelayTapL[idxU] * dtU);
            chorusR += pEffect->cho_pan_ur * (pDelayTapR[idxU - 1] * (1.0 - dtU) + pDelayTapR[idxU] * dtU);
        }
        if (idxV == 0) {
            chorusL += pEffect->cho_pan_vl * (pDelayTapL[TAP_LENGTH - 1] * (1.0 - dtV) + pDelayTapL[idxV] * dtV);
            chorusR += pEffect->cho_pan_vr * (pDelayTapR[TAP_LENGTH - 1] * (1.0 - dtV) + pDelayTapR[idxV] * dtV);
        } else {
            chorusL += pEffect->cho_pan_vl * (pDelayTapL[idxV - 1] * (1.0 - dtV) + pDelayTapL[idxV] * dtV);
            chorusR += pEffect->cho_pan_vr * (pDelayTapR[idxV - 1] * (1.0 - dtV) + pDelayTapR[idxV] * dtV);
        }
        if (idxW == 0) {
            chorusL += pEffect->cho_pan_wl * (pDelayTapL[TAP_LENGTH - 1] * (1.0 - dtW) + pDelayTapL[idxW] * dtW);
            chorusR += pEffect->cho_pan_wr * (pDelayTapR[TAP_LENGTH - 1] * (1.0 - dtW) + pDelayTapR[idxW] * dtW);
        } else {
            chorusL += pEffect->cho_pan_wl * (pDelayTapL[idxW - 1] * (1.0 - dtW) + pDelayTapL[idxW] * dtW);
            chorusR += pEffect->cho_pan_wr * (pDelayTapR[idxW - 1] * (1.0 - dtW) + pDelayTapR[idxW] * dtW);
        }
        *pOutputL += chorusL * pParam->chorus_send / 3.0;
        *pOutputR += chorusR * pParam->chorus_send / 3.0;

        /*** update lfo ***/
        double lfoDelta = PI2 * INV_SQRT3 * pParam->chorus_rate * pSysValue->delta_time;
        pEffect->cho_lfo_u += (pEffect->cho_lfo_v - pEffect->cho_lfo_w) * lfoDelta;
        pEffect->cho_lfo_v += (pEffect->cho_lfo_w - pEffect->cho_lfo_u) * lfoDelta;
        pEffect->cho_lfo_w += (pEffect->cho_lfo_u - pEffect->cho_lfo_v) * lfoDelta;
    }
}
