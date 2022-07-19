#ifndef __HEADER_DLL_MAIN__
#define __HEADER_DLL_MAIN__

#include <windows.h>
#include "type.h"

/******************************************************************************/
#ifdef __cplusplus
extern "C" {
#endif
    __declspec(dllexport) void WINAPI waveout_open(int sample_rate, int buffer_length, int buffer_count, BOOL enable_float);
    __declspec(dllexport) BOOL WINAPI waveout_close();
    __declspec(dllexport) byte* WINAPI dls_load(char* file_path);
#ifdef __cplusplus
}
#endif
#endif /* __HEADER_DLL_MAIN__ */
