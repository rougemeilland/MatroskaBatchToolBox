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
            return
                videoEncoderType switch
                {
                    VideoEncoderType.Libx264 => "H.264",
                    VideoEncoderType.Libx265 => "H.265",
                    VideoEncoderType.LibaomAV1 => "AV1",
                    _ => throw new Exception($"Unsupported video codec.: \"{videoEncoderType}\""),
                };
        }

        public static string ToCodecSpec(this VideoEncoderType videoEncoderType)
        {
            return
                videoEncoderType switch
                {
                    VideoEncoderType.Libx264 => _libx264EncoderName,
                    VideoEncoderType.Libx265 => _libx265EncoderName,
                    VideoEncoderType.LibaomAV1 => _libaomAV1EncoderName,
                    _ => throw new Exception($"Unsupported video codec.: \"{videoEncoderType}\""),
                };
        }

        public static string GetEncodingOption(this Settings localSettings, VideoEncoderType videoEncoderType)
        {
            return
                videoEncoderType switch
                {
                    VideoEncoderType.Libx264 => localSettings.FFmpegLibx264EncoderOption,
                    VideoEncoderType.Libx265 => localSettings.FFmpegLibx265EncoderOption,
                    VideoEncoderType.LibaomAV1 => localSettings.FFmpegLibaomAV1EncoderOption,
                    _ => throw new Exception($"Unsupported video codec.: \"{videoEncoderType}\""),
                };
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
