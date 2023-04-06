// from https://github.com/dotnet/runtime/blob/main/src/native/libs/System.Native/pal_console.c

#include <assert.h>
#include <errno.h>
#include <fcntl.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/ioctl.h>
#include <termios.h>
#include <unistd.h>
#include <poll.h>
#include <pthread.h>
#include <signal.h>

#define HAVE_IOCTL (1)
#define HAVE_TIOCGWINSZ (1)
#define HAVE_TIOCSWINSZ (1)
#define HAVE_IOCTL_WITH_INT_REQUEST (1)

#define __DLLEXPORT __attribute__ ((__visibility__ ("default")))

#define STANDARD_FILE_IN (0)
#define STANDARD_FILE_OUT (1)
#define STANDARD_FILE_ERR (2)

typedef struct
{
    uint16_t Row;
    uint16_t Col;
    uint16_t XPixel;
    uint16_t YPixel;
} WinSize;

__DLLEXPORT int32_t PalmtreeNative_GetStandardFileNo(int32_t standardFileType)
{
    switch (standardFileType)
    {
    case STANDARD_FILE_IN:
        return STDIN_FILENO;
    case STANDARD_FILE_OUT:
        return STDOUT_FILENO;
    case STANDARD_FILE_ERR:
        return STDERR_FILENO;
    default:
        errno = EINVAL;
        return -1;
    }
}

__DLLEXPORT int32_t PalmtreeNative_GetWindowSize(int32_t consoleFileNo, WinSize* windowSize, int32_t* errnoBuffer)
{
    assert(windowSize != NULL);

#if HAVE_IOCTL && HAVE_TIOCGWINSZ
    int32_t result = ioctl(consoleFileNo, TIOCGWINSZ, windowSize);
    *errnoBuffer = errno;
    if (result != 0)
        memset(windowSize, 0, sizeof(WinSize)); // managed out param must be initialized
    return result;
#else
    memset(windowSize, 0, sizeof(WinSize)); // managed out param must be initialized
    *errnoBuffer = errno = ENOTSUP;
    return -1;
#endif
}

__DLLEXPORT int32_t PalmtreeNative_SetWindowSize(int32_t consoleFileNo, WinSize* windowSize, int32_t* errnoBuffer)
{
    assert(windowSize != NULL);

#if HAVE_IOCTL_WITH_INT_REQUEST && HAVE_TIOCSWINSZ
    int32_t result = ioctl(consoleFileNo, (int)TIOCSWINSZ, windowSize);
    *errnoBuffer = errno;
    return result;
#elif HAVE_IOCTL && HAVE_TIOCSWINSZ
    int32_t result = ioctl(consoleFileNo, TIOCSWINSZ, windowSize);
    *errnoBuffer = errno;
    return result;
#else
    // Not supported on e.g. Android. Also, prevent a compiler error because windowSize is unused
    (void)windowSize;
    *errnoBuffer = errno = ENOTSUP;
    return -1;
#endif
}
