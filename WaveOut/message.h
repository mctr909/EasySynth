#ifndef __HEADER_MESSAGE__
#define __HEADER_MESSAGE__

#include "type.h"
#include <windows.h>

/******************************************************************************/
enum struct E_MESSSAGE_TYPE : byte {
    NOTE_OFF = 0x80,
    NOTE_ON = 0x90,
    KEY_PRESS = 0xA0,
    CTRL_CHG = 0xB0,
    PROG_CHG = 0xC0,
    CH_PRESS = 0xD0,
    PITCH = 0xE0,
    SYSTEM = 0xF0
};

enum struct E_CTRL_TYPE : byte {
    BANK_MSB = 0x00,
    BANK_LSB = 0x20,

    VOLUME = 0x07,
    PAN = 0x0A,
    EXP = 0x0B,

    MODURATION = 0x01,
    VIB_RATE = 0x4C,
    VIB_DEPTH = 0x4D,
    VIB_DELAY = 0x4E,

    PORT_TIME = 0x05,
    PORT_SWITCH = 0x41,

    DAMPER = 0x40,
    RELEASE = 0x48,
    ATTACK = 0x49,

    CUTOFF = 0x46,
    RESONANCE = 0x4A,

    REVERB = 0x5B,
    CHORUS = 0x5D,
    DELAY = 0x5E,

    DATA_MSB = 0x06,
    DATA_LSB = 0x26,
    RPN_LSB = 0x64,
    RPN_MSB = 0x65,

    ALL_SOUND_OFF = 0x78,
    RESET_CTRL = 0x79,
    ALL_NOTE_OFF = 0x7B,
};

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
__declspec(dllexport) void WINAPI message_send(byte* pMsg);
#ifdef __cplusplus
}
#endif

#endif /* __HEADER_MESSAGE__ */
