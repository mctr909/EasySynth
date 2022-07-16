#include "dll_main.h"
#include "effect.h"
#include <mmsystem.h>

#pragma comment (lib, "winmm.lib")

/******************************************************************************/
SYSVALUE     mSysValue = { 0 };
HWAVEOUT     mhWaveOut = NULL;
WAVEFORMATEX mWaveFmt = { 0 };
WAVEHDR**    mppWaveHdr = NULL;
void (*mfpWriteBuffer)(LPSTR) = NULL;

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

/******************************************************************************/
BOOL waveOutOpen(void(*fpWriteBufferProc)(LPSTR));
void CALLBACK waveOutProc(HWAVEOUT hwo, UINT uMsg, DWORD_PTR dwInstance, DWORD dwParam1, DWORD dwParam);
DWORD writeBufferTask(LPVOID* param);
void doStop();
void writeShort(LPSTR pData);
void writeFloat(LPSTR pData);

/******************************************************************************/
void WINAPI waveout_open(
    int sample_rate,
    int buffer_length,
    int buffer_count,
    BOOL enable_float
) {
    waveout_close();
    //
    mSysValue.sample_rate = sample_rate;
    mSysValue.delta_time = 1.0 / sample_rate;
    mSysValue.buffer_length = buffer_length;
    mBufferCount = buffer_count;
    //
    effect_create(&mSysValue);
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
        waveOutUnprepareHeader(mhWaveOut, mppWaveHdr[n], sizeof(WAVEHDR));
    }
    waveOutReset(mhWaveOut);
    waveOutClose(mhWaveOut);
    //
    effect_dispose(&mSysValue);
    return TRUE;
}

void WINAPI message_send(byte* pMsg) {
}

byte* WINAPI dls_load(LPWSTR file_path) {
    doStop();
    mDoStop = FALSE;
    return NULL;
}

/******************************************************************************/
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
    auto hThread = CreateThread(NULL, 0, (LPTHREAD_START_ROUTINE)writeBufferTask, NULL, 0, &mThreadId);
    if (NULL == hThread) {
        waveout_close();
        return FALSE;
    }
    SetThreadPriority(hThread, THREAD_PRIORITY_HIGHEST);
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
        if (10 < elapsedTime) {
            return;
        }
    }
}

void writeShort(LPSTR pData) {
    auto pOutput = (short*)pData;
    for (int i = 0; i < mSysValue.buffer_length; i++, pOutput += 2) {
        double ia = 0, ib = 0;
        double oa = 0, ob = 0;
        effect(mSysValue.pp_effect[0], &ia, &ib, &oa, &ob);

        pOutput[0] = (short)(32000 * oa);
        pOutput[1] = (short)(32000 * ob);
    }
}

void writeFloat(LPSTR pData) {
    auto pOutput = (float*)pData;
    for (int i = 0; i < mSysValue.buffer_length; i++, pOutput += 2) {
        double ia = 0, ib = 0;
        double oa = 0, ob = 0;
        effect(mSysValue.pp_effect[0], &ia, &ib, &oa, &ob);

        pOutput[0] = (float)(32000 / 32767.0 * oa);
        pOutput[1] = (float)(32000 / 32767.0 * ob);
    }
}
