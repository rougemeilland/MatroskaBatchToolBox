#include <stdio.h>
#include "windows.h"

int main()
{
    int values[] =
    {
        -1,
        0,
        1,
        12,
        123,
        123456,
    };

    const char* formats[] =
    {
        "%c",
        "%8c",
        "%08c",
        "%.4c",
        "%8.4c",
        "%08.4c",
        "%d",
        "%8d",
        "%08d",
        "%.4d",
        "%8.4d",
        "%08.4d",
        "%o",
        "%8o",
        "%08o",
        "%.4o",
        "%8.4o",
        "%08.4o",
        "%u",
        "%8u",
        "%08u",
        "%.4u",
        "%8.4u",
        "%08.4u",
        "%x",
        "%8x",
        "%08x",
        "%.4x",
        "%8.4x",
        "%08.4x",
        "%X",
        "%8X",
        "%08X",
        "%.4X",
        "%8.4X",
        "%08.4X",
    };

    for (int index1 = 0; index1 < sizeof(values) / sizeof(values[0]); ++index1)
    {
        int value = values[index1];
        for (int index2 = 0; index2 < sizeof(formats) / sizeof(formats[0]); ++index2)
        {
            const char* format = formats[index2];
            char buffer[256];
            sprintf_s(buffer, format, value);
            printf("new { value = %d, format = \"%s\", result = \"", value, format);
            const char* source = buffer;
            while (*source != '\0')
            {
                if (*source >= 0x20 && *source <= 0x7e)
                    putchar(*source);
                else
                    printf("\\x%02x", *source & 0xff);
                ++source;
            }
            printf("\" },\n");
        }
    }
    return 0;
}
