using System;

namespace MatroskaBatchToolBox
{
    internal static class AudioEncoderTypeExtensions
    {
        public static string ToFormatName(this AudioEncoderType audioEncoderType)
        {
            return
                audioEncoderType switch
                {
                    AudioEncoderType.Libopus => "Opus",
                    AudioEncoderType.Libvorbis => "Vorbis",
                    _ => throw new Exception($"Unsupported audio codec.: \"{audioEncoderType}\""),
                };
        }

        public static string ToCodecSpec(this AudioEncoderType audioEncoderType)
        {
            return
                audioEncoderType switch
                {
                    AudioEncoderType.Libopus => "libopus",
                    AudioEncoderType.Libvorbis => "libvorbis",
                    _ => throw new Exception($"Unsupported audio codec.: \"{audioEncoderType}\""),
                };
        }
    }
}
