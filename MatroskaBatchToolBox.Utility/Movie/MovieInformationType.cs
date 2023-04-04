using System;

namespace MatroskaBatchToolBox.Utility.Movie
{
    [Flags]
    public enum MovieInformationType
    {
        None = 0,
        Streams = 1 << 0,
        Chapters = 1 << 1,
    }
}
