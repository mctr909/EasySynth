#include "sampler.h"

/******************************************************************************/
#define OVER_SAMPLING 4
#define PI2 6.283185307179586476925286766559

/******************************************************************************/
inline void osc_sin(OSC* param);
inline void osc_pwm(OSC* param);
inline void osc_saw(OSC* param);
inline void osc_tri(OSC* param);
inline void genenv(SAMPLER *pSmpl);

/******************************************************************************/
inline void osc_sin(OSC* param) {
    param->output = param->width * param->gain;
    param->width -= param->count * param->delta * PI2;
    param->count += param->width * param->delta * PI2;
}

inline void osc_pwm(OSC* param) {
    if (param->count < param->width) {
        param->output = param->gain;
    } else {
        param->output = -param->gain;
    }
    param->count += param->delta;
    if (1.0 <= param->count) {
        param->count -= 1.0;
    }
}

inline void osc_saw(OSC* param) {
    if (param->count < 0.5) {
        param->output = param->count * param->gain * 2;
    } else {
        param->output = (param->count - 1.0) * param->gain * 2;
    }
    param->count += param->delta;
    if (1.0 <= param->count) {
        param->count -= 1.0;
    }
}

inline void osc_tri(OSC* param) {
    if (param->count < 0.5) {
        param->output = (param->count - 0.25) * param->gain * 4;
    } else {
        param->output = (0.75 - param->count) * param->gain * 4;
    }
    param->count += param->delta;
    if (1.0 <= param->count) {
        param->count -= 1.0;
    }
}

inline void genenv(SAMPLER *pSmpl) {
    auto pEnv = pSmpl->pEnv;
    auto delta_time = pSmpl->pSysValue->delta_time;
    switch (pSmpl->status) {
    case E_SAMPLER_STATE::PRESS:
        if (pSmpl->time < pEnv->amp.attack) {
            pSmpl->env_amp += (1.0 - pSmpl->env_amp) * delta_time / pEnv->amp.attack;
        } else {
            pSmpl->env_amp += (pEnv->amp.sustain - pSmpl->env_amp) * delta_time / pEnv->amp.decay;
        }
        pSmpl->env_pitch += (1.0 - pSmpl->env_pitch) * delta_time / pEnv->pitch.attack;
        if (pSmpl->time < pEnv->cutoff.attack) {
            pSmpl->env_cutoff += (1.0 - pSmpl->env_cutoff) * delta_time / pEnv->cutoff.attack;
        } else {
            pSmpl->env_cutoff += (pEnv->cutoff.sustain - pSmpl->env_cutoff) * delta_time / pEnv->cutoff.decay;
        }
        break;
    case E_SAMPLER_STATE::RELEASE:
        pSmpl->env_amp -= pSmpl->env_amp * delta_time / pEnv->amp.release;
        pSmpl->env_pitch += (pEnv->pitch.fall - pSmpl->env_pitch) * delta_time / pEnv->pitch.release;
        pSmpl->env_cutoff += (pEnv->cutoff.fall - pSmpl->env_cutoff) * delta_time / pEnv->cutoff.release;
        break;
    case E_SAMPLER_STATE::HOLD:
        pSmpl->env_amp -= pSmpl->env_amp * delta_time;
        pSmpl->env_pitch += (pEnv->pitch.fall - pSmpl->env_pitch) * delta_time / pEnv->pitch.release;
        pSmpl->env_cutoff += (pEnv->cutoff.fall - pSmpl->env_cutoff) * delta_time / pEnv->cutoff.release;
        break;
    case E_SAMPLER_STATE::PURGE:
        pSmpl->env_amp -= pSmpl->env_amp * 500 * delta_time;
        break;
    }
}
