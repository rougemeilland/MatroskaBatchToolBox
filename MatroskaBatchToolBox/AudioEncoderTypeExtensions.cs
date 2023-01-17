using System;

namespace MatroskaBatchToolBox
{
    internal static class AudioEncoderTypeExtensions
    {
        public static string ToFormatName(this AudioEncoderType audioEncoderType)
        {
            switch (audioEncoderType)
            {
                case AudioEncoderType.Libopus:
                    return "Opus";
                case AudioEncoderType.Libvorbis:
                    return "Vorbis";
                default:
                    throw new Exception($"Unsupported audio codec.: \"{audioEncoderType}\"");
            }
        }

        public static string ToCodecSpec(this AudioEncoderType audioEncoderType)
        {
            switch (audioEncoderType)
            {
                case AudioEncoderType.Libopus:
                    return "libopus";
                case AudioEncoderType.Libvorbis:
                    return "libvorbis";
                default:
                    throw new Exception($"Unsupported audio codec.: \"{audioEncoderType}\"");
            }
        }
    }
}
