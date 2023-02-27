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

#if false   // -tag:v hvc1 オプションを追加しないように修正。以下はその理由。
                    // hvc1 タグが必要なのは H.265 のビデオトラックを持つ mp4 ファイルを apple 系のプレイヤーで再生する場合に必要となるから。
                    // つまり、出力先が Matoroska である場合は、実は hvc1 タグの追加は不要。
                    // その辺りの仕様が影響しているせいなのか、ffmpeg は出力先コンテナが Matoroska である場合は、H.265 ストリームのタグに hvc1 が指定されてもそれを出力先ファイルに反映しない。
                    // エンコード中のログにはエンコード方法が H.265 の hvc1 であることが表示されるのだが、出力された動画ファイルを ffprobe で調べると、ビデオストリームの codec_tag_string が "[0][0][0][0]" の値にクリアされてしまっている。
                    // 一方、同じ方法でエンコードして出力先コンテナが mp4 の場合は、codec_tag_string が "hvc1" となっている。
                    options.Add($"-tag:v:{outputStreamIndexWithinVideoStreams} hvc1");
#endif

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
            else if (string.Equals(videoEncoderTypeText, _libx264EncoderName, StringComparison.Ordinal))
                return VideoEncoderType.Libx264;
            else if (string.Equals(videoEncoderTypeText, _libx265EncoderName, StringComparison.Ordinal))
                return VideoEncoderType.Libx265;
            else if (string.Equals(videoEncoderTypeText, _libaomAV1EncoderName, StringComparison.Ordinal))
                return VideoEncoderType.LibaomAV1;
            else
                return null;
        }
    }
}
