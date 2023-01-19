using System;
using System.IO;

namespace MatroskaBatchToolBox
{
    internal static class VideoEncoderTypeExtensions
    {
        private const string _libx264EncoderName = "libx264";
        private const string _libx265EncoderName = "libx265";
        private const string _libaomAV1EncoderName = "libaom-av1";

        public static string ToFormatName(this VideoEncoderType videoEncoderType)
        {
            switch (videoEncoderType)
            {
                case VideoEncoderType.Libx264:
                    return "H.264";
                case VideoEncoderType.Libx265:
                    return "H.265";
                case VideoEncoderType.LibaomAV1:
                    return "AV1";
                default:
                    throw new Exception($"Unsupported video codec.: \"{videoEncoderType}\"");
            }
        }

        public static string ToCodecSpec(this VideoEncoderType videoEncoderType)
        {
            switch (videoEncoderType)
            {
                case VideoEncoderType.Libx264:
                    return _libx264EncoderName;
                case VideoEncoderType.Libx265:
                    return _libx265EncoderName;
                case VideoEncoderType.LibaomAV1:
                    return _libaomAV1EncoderName;
                default:
                    throw new Exception($"Unsupported video codec.: \"{videoEncoderType}\"");
            }
        }

        public static string GetEncodingOption(this Settings localSettings, VideoEncoderType videoEncoderType)
        {
            switch (videoEncoderType)
            {
                case VideoEncoderType.Libx264:
                    return localSettings.Libx264EncoderOptionOnComplexConversion;
                case VideoEncoderType.Libx265:
                    return localSettings.Libx265EncoderOptionOnComplexConversion;
                case VideoEncoderType.LibaomAV1:
                    return localSettings.LibaomAV1EncoderOptionOnComplexConversion;
                default:
                    throw new Exception($"Unsupported video codec.: \"{videoEncoderType}\"");
            }
        }

        public static VideoEncoderType? TryParseAsVideoEncoderType(this string? videoEncoderTypeText)
        {
            if (string.IsNullOrEmpty(videoEncoderTypeText))
                return null;
            else if (string.Equals(videoEncoderTypeText, _libx264EncoderName, StringComparison.InvariantCulture))
                return VideoEncoderType.Libx264;
            else if (string.Equals(videoEncoderTypeText, _libx265EncoderName, StringComparison.InvariantCulture))
                return VideoEncoderType.Libx265;
            else if (string.Equals(videoEncoderTypeText, _libaomAV1EncoderName, StringComparison.InvariantCulture))
                return VideoEncoderType.LibaomAV1;
            else
                return null;
        }
    }
}
