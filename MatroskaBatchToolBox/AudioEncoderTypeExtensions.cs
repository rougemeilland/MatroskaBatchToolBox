using System;

namespace MatroskaBatchToolBox
{
    internal static class AudioEncoderTypeExtensions
    {
        public static string ToFormatName(this AudioEncoderType audioEncoderType)
            => audioEncoderType switch
            {
                AudioEncoderType.Libopus => "Opus",
                AudioEncoderType.Libvorbis => "Vorbis",
                _ => throw new ApplicationException($"Unsupported audio codec.: \"{audioEncoderType}\""),
            };

        public static string ToCodecSpec(this AudioEncoderType audioEncoderType)
            => audioEncoderType switch
            {
                AudioEncoderType.Libopus => "libopus",
                AudioEncoderType.Libvorbis => "libvorbis",
                _ => throw new ApplicationException($"Unsupported audio codec.: \"{audioEncoderType}\""),
            };
    }
}
