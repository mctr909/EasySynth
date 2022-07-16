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
typedef struct EFFECT EFFECT;

/******************************************************************************/
typedef struct SYSVALUE {
    int      buffer_length;
    int      sample_rate;
    double   delta_time;
    EFFECT** pp_effect;
} SYSVALUE;

#endif /* __HEADER_TYPE__ */
