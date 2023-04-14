using System;

namespace MatroskaBatchToolBox.Utility.Movie
{
    [Flags]
    public enum MovieInformationType
    {
        None = 0,
        Format = 1 << 0,
        Streams = 1 << 1,
        Chapters = 1 << 2,
    }
}
