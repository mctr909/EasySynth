#include "dll_main.h"
#include "effect.h"
#include "message.h"
#include <mmsystem.h>

#pragma comment (lib, "winmm.lib")

/******************************************************************************/
SYSVALUE     mSysValue = { 0 };
HWAVEOUT     mhWaveOut = NULL;
WAVEFORMATEX mWaveFmt = { 0 };
WAVEHDR**    mppWaveHdr = NULL;
void (*mfpWriteBuffer)(LPSTR) = NULL;

HANDLE           mhThread = NULL;
DWORD            mThreadId = 0;
CRITICAL_SECTION mcsBufferLock = { 0 };

BOOL mDoStop = TRUE;
BOOL mWaveOutStopped = TRUE;
BOOL mThreadStopped = TRUE;

int mActiveCount = 0;
int mBufferCount = 0;
int mWriteCount = 0;
int mWriteIndex = 0;
int mReadIndex = 0;

double *mpWaveBufferL = NULL;
double *mpWaveBufferR = NULL;

/******************************************************************************/
BOOL waveOutOpen(void(*fpWriteBufferProc)(LPSTR));
void CALLBACK waveOutProc(HWAVEOUT hwo, UINT uMsg, DWORD_PTR dwInstance, DWORD dwParam1, DWORD dwParam);
DWORD writeBufferTask(LPVOID* param);
void doStop();
void write();
void writeShort(LPSTR pData);
void writeFloat(LPSTR pData);

/******************************************************************************/
void note_off(byte ch, byte note_num);
void note_on(byte ch, byte note_num, byte velo);
void ctrl_chg(byte ch, E_CTRL_TYPE type, byte value);
void prog_chg(byte ch, byte num);
void pitch(byte ch, byte lsb, byte msb);

/******************************************************************************/
void WINAPI waveout_open(int sample_rate, int buffer_length, int buffer_count, BOOL enable_float) {
    waveout_close();
    //
    mSysValue.sample_rate = sample_rate;
    mSysValue.delta_time = 1.0 / sample_rate;
    mSysValue.buffer_length = buffer_length;
    mBufferCount = buffer_count;
    // Create
    effect_create(&mSysValue);
    // Allocate Wave buffer
    mpWaveBufferL = (double*)malloc(sizeof(double) * buffer_length);
    mpWaveBufferR = (double*)malloc(sizeof(double) * buffer_length);
    //
    if (enable_float) {
        waveOutOpen(writeFloat);
    } else {
        waveOutOpen(writeShort);
    }
}

BOOL waveout_close() {
    if (NULL == mhWaveOut) {
        return FALSE;
    }
    doStop();
    // Unprepare Wave header
    for (int n = 0; n < mBufferCount; ++n) {
        if (NULL == mppWaveHdr) {
            break;
        }
        waveOutUnprepareHeader(mhWaveOut, mppWaveHdr[n], sizeof(WAVEHDR));
    }
    waveOutReset(mhWaveOut);
    waveOutClose(mhWaveOut);
    // Dispose
    effect_dispose(&mSysValue);
    // Release Wave buffer
    free(mpWaveBufferL);
    mpWaveBufferL = NULL;
    free(mpWaveBufferR);
    mpWaveBufferR = NULL;
    return TRUE;
}

BOOL waveOutOpen(void(*fpWriteBufferProc)(LPSTR)) {
    if (NULL != mhWaveOut) {
        if (!waveout_close()) {
            return FALSE;
        }
    }
    if (NULL == fpWriteBufferProc) {
        return FALSE;
    }
    //
    mWriteCount = 0;
    mWriteIndex = 0;
    mReadIndex = 0;
    mfpWriteBuffer = fpWriteBufferProc;
    // Setting WaveFmt
    mWaveFmt.wFormatTag = fpWriteBufferProc == writeFloat ? 3 : 1;
    mWaveFmt.nChannels = 2;
    mWaveFmt.wBitsPerSample = (WORD)(fpWriteBufferProc == writeFloat ? 32 : 16);
    mWaveFmt.nSamplesPerSec = (DWORD)mSysValue.sample_rate;
    mWaveFmt.nBlockAlign = mWaveFmt.nChannels * mWaveFmt.wBitsPerSample / 8;
    mWaveFmt.nAvgBytesPerSec = mWaveFmt.nSamplesPerSec * mWaveFmt.nBlockAlign;
    // WaveOut Open
    if (MMSYSERR_NOERROR != waveOutOpen(
        &mhWaveOut,
        WAVE_MAPPER,
        &mWaveFmt,
        (DWORD_PTR)waveOutProc,
        (DWORD_PTR)mppWaveHdr,
        CALLBACK_FUNCTION
    )) {
        return FALSE;
    }
    // Create Wave write Thread
    InitializeCriticalSection((LPCRITICAL_SECTION)&mcsBufferLock);
    if (NULL == mhThread) {
        mhThread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)writeBufferTask, NULL, 0, &mThreadId);
    }
    if (NULL == mhThread) {
        waveout_close();
        return FALSE;
    }
    SetThreadPriority(mhThread, THREAD_PRIORITY_HIGHEST);
    // Allocate Wave header
    mppWaveHdr = (WAVEHDR**)calloc(mBufferCount, sizeof(WAVEHDR*));
    for (int n = 0; n < mBufferCount; ++n) {
        mppWaveHdr[n] = (WAVEHDR*)calloc(1, sizeof(WAVEHDR));
        auto pWaveHdr = mppWaveHdr[n];
        pWaveHdr->dwBufferLength = (DWORD)(mSysValue.buffer_length * mWaveFmt.nBlockAlign);
        pWaveHdr->dwFlags = WHDR_BEGINLOOP | WHDR_ENDLOOP;
        pWaveHdr->dwLoops = 0;
        pWaveHdr->dwUser = 0;
        pWaveHdr->lpData = (LPSTR)calloc(mSysValue.buffer_length, mWaveFmt.nBlockAlign);
    }
    // Prepare Wave header
    for (int n = 0; n < mBufferCount; ++n) {
        waveOutPrepareHeader(mhWaveOut, mppWaveHdr[n], sizeof(WAVEHDR));
        waveOutWrite(mhWaveOut, mppWaveHdr[n], sizeof(WAVEHDR));
    }
    return TRUE;
}

void CALLBACK waveOutProc(HWAVEOUT hwo, UINT uMsg, DWORD_PTR dwInstance, DWORD dwParam1, DWORD dwParam) {
    switch (uMsg) {
    case MM_WOM_OPEN:
        mDoStop = FALSE;
        break;
    case MM_WOM_CLOSE:
        // Release Wave header
        for (int b = 0; b < mBufferCount; ++b) {
            free(mppWaveHdr[b]->lpData);
            free(mppWaveHdr[b]);
        }
        free(mppWaveHdr);
        mppWaveHdr = NULL;
        break;
    case MM_WOM_DONE:
        if (mDoStop) {
            mWaveOutStopped = TRUE;
            break;
        }
        mWaveOutStopped = FALSE;
        EnterCriticalSection((LPCRITICAL_SECTION)&mcsBufferLock);
        {
            // Buffer empty
            if (mWriteCount < 1) {
                waveOutWrite(mhWaveOut, mppWaveHdr[mReadIndex], sizeof(WAVEHDR));
                LeaveCriticalSection((LPCRITICAL_SECTION)&mcsBufferLock);
                break;
            }
            // Output wave
            waveOutWrite(mhWaveOut, mppWaveHdr[mReadIndex], sizeof(WAVEHDR));
            mReadIndex = (mReadIndex + 1) % mBufferCount;
            mWriteCount--;
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)&mcsBufferLock);
        break;
    default:
        break;
    }
}

DWORD writeBufferTask(LPVOID* param) {
    while (TRUE) {
        if (mDoStop) {
            mThreadStopped = TRUE;
            break;
        }
        mThreadStopped = FALSE;
        if (NULL == mppWaveHdr || NULL == mppWaveHdr[0] || NULL == mppWaveHdr[0]->lpData) {
            Sleep(100);
            continue;
        }
        EnterCriticalSection((LPCRITICAL_SECTION)&mcsBufferLock);
        {
            // Buffer full
            if (mBufferCount <= mWriteCount + 1) {
                LeaveCriticalSection((LPCRITICAL_SECTION)&mcsBufferLock);
                Sleep(1);
                continue;
            }
            // Write Buffer
            auto pBuff = mppWaveHdr[mWriteIndex]->lpData;
            memset(pBuff, 0, mWaveFmt.nBlockAlign * mSysValue.buffer_length);
            mfpWriteBuffer(pBuff);
            mWriteIndex = (mWriteIndex + 1) % mBufferCount;
            mWriteCount++;
        }
        LeaveCriticalSection((LPCRITICAL_SECTION)&mcsBufferLock);
    }
    return 0;
}

void doStop() {
    mDoStop = TRUE;
    long elapsedTime = 0;
    while (!mWaveOutStopped || !mThreadStopped) {
        Sleep(100);
        elapsedTime++;
    }
}

void write() {
    for (int s = 0; s < SAMPLER_COUNT; s++) {
    }
    memset(mpWaveBufferL, 0, sizeof(double) * mSysValue.buffer_length);
    memset(mpWaveBufferR, 0, sizeof(double) * mSysValue.buffer_length);
    for (int c = 0; c < CHANNEL_COUNT; c++) {
        auto pEffect = mSysValue.pp_effect[c];
        auto pInputL = pEffect->value.p_output_l;
        auto pInputR = pEffect->value.p_output_r;
        for (int i = 0; i < mSysValue.buffer_length; i++) {
            double tempL, tempR;
            effect(pEffect, pInputL[i], pInputR[i], &tempL, &tempR);
            mpWaveBufferL[i] += tempL;
            mpWaveBufferR[i] += tempR;
        }
    }
}

void writeShort(LPSTR pData) {
    write();
    auto pOutput = (short*)pData;
    for (int i = 0; i < mSysValue.buffer_length; i++) {
        auto l = mpWaveBufferL[i] * 32767;
        auto r = mpWaveBufferR[i] * 32767;
        if (32767 < l) l = 32767;
        if (l < -32767) l = -32767;
        if (32767 < r) r = 32767;
        if (r < -32767) r = -32767;
        pOutput[0] = (short)l;
        pOutput[1] = (short)r;
        pOutput += 2;
    }
}

void writeFloat(LPSTR pData) {
    write();
    auto pOutput = (float*)pData;
    for (int i = 0; i < mSysValue.buffer_length; i++) {
        pOutput[0] = (float)mpWaveBufferL[i];
        pOutput[1] = (float)mpWaveBufferR[i];
        pOutput += 2;
    }
}

/******************************************************************************/
byte* WINAPI dls_load(char* file_path) {
    waveout_close();
    waveout_open(mSysValue.sample_rate, mSysValue.buffer_length, mBufferCount, mfpWriteBuffer == writeFloat);
    return NULL;
}

/******************************************************************************/
void WINAPI message_send(byte* pMsg) {
    auto type = (E_MESSSAGE_TYPE)(*pMsg & 0xF0);
    auto ch = (byte)(*pMsg & 0x0F);

    switch (type) {
    case E_MESSSAGE_TYPE::NOTE_OFF:
        note_off(ch, pMsg[1]);
        break;
    case E_MESSSAGE_TYPE::NOTE_ON:
        note_on(ch, pMsg[1], pMsg[2]);
        break;
    case E_MESSSAGE_TYPE::CTRL_CHG:
        ctrl_chg(ch, (E_CTRL_TYPE)pMsg[1], pMsg[2]);
        break;
    case E_MESSSAGE_TYPE::PROG_CHG:
        prog_chg(ch, pMsg[1]);
        break;
    case E_MESSSAGE_TYPE::PITCH:
        pitch(ch, pMsg[1], pMsg[2]);
        break;
    case E_MESSSAGE_TYPE::KEY_PRESS:
    case E_MESSSAGE_TYPE::CH_PRESS:
    case E_MESSSAGE_TYPE::SYSTEM:
        break;
    default:
        break;
    }
}

void note_off(byte ch, byte note_num) {
    for (int i = 0; i < SAMPLER_COUNT; i++) {

    }
}

void note_on(byte ch, byte note_num, byte velo) {
    if (0 == velo) {
        note_off(ch, note_num);
        return;
    }
    for (int i = 0; i < SAMPLER_COUNT; i++) {

    }
    for (int i = 0; i < SAMPLER_COUNT; i++) {

    }
}

void ctrl_chg(byte ch, E_CTRL_TYPE type, byte value) {
}

void prog_chg(byte ch, byte num) {
}

void pitch(byte ch, byte lsb, byte msb) {
}
