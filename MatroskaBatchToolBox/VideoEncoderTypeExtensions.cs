using System;
using System.Collections.Generic;

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

        public static IEnumerable<string> GetEncoderOptions(this VideoEncoderType videoEncoderType, Settings localSettings, int outputStreamIndexWithinVideoStreams)
        {
            var options = new List<string>();
            switch (videoEncoderType)
            {
                case VideoEncoderType.Copy:
                    options.Add($"-c:v:{outputStreamIndexWithinVideoStreams} copy");
                    break;
                case VideoEncoderType.LibaomAV1:
                    options.Add($"-c:v:{outputStreamIndexWithinVideoStreams} {_libaomAV1EncoderName}");
                    if (!string.IsNullOrEmpty(localSettings.FFmpegLibaomAV1EncoderOption))
                        options.Add(localSettings.FFmpegLibaomAV1EncoderOption);
                    break;
                case VideoEncoderType.Libx264:
                    options.Add($"-c:v:{outputStreamIndexWithinVideoStreams} {_libx264EncoderName}");
                    if (!string.IsNullOrEmpty(localSettings.FFmpegLibx264EncoderOption))
                        options.Add(localSettings.FFmpegLibx264EncoderOption);
                    break;
                case VideoEncoderType.Libx265:
                    options.Add($"-c:v:{outputStreamIndexWithinVideoStreams} {_libx265EncoderName}");
                    options.Add($"-tag:v:{outputStreamIndexWithinVideoStreams} hvc1");
                    if (!string.IsNullOrEmpty(localSettings.FFmpegLibx265EncoderOption))
                        options.Add(localSettings.FFmpegLibx265EncoderOption);
                    break;
                default:
                    throw new Exception($"Unsupported video codec.: \"{videoEncoderType}\"");
            }
            return options;
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
