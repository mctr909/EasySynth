#ifndef __HEADER_TYPE__
#define __HEADER_TYPE__

/******************************************************************************/
#define CHANNEL_COUNT 16
#define SAMPLER_COUNT 128
#define NULL 0

/******************************************************************************/
typedef unsigned char  byte;
typedef unsigned short ushort;
typedef unsigned int   uint;

/******************************************************************************/
static int    g_buffer_length = 0;
static int    g_sample_rate = 44100;
static double g_delta_time = 1.0 / 44100;

#endif /* __HEADER_TYPE__ */
