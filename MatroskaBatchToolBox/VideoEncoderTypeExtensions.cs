using System;
using System.Collections.Generic;

namespace MatroskaBatchToolBox
{
    internal static class VideoEncoderTypeExtensions
    {
        private const string _libx264EncoderName = "libx264";
        private const string _libx265EncoderName = "libx265";
        private const string _libaomAv1EncoderName = "libaom-av1";

        public static string ToFormatName(this VideoEncoderType videoEncoderType)
            => videoEncoderType switch
            {
                VideoEncoderType.Libx264 => "H.264",
                VideoEncoderType.Libx265 => "H.265",
                VideoEncoderType.LibaomAv1 => "AV1",
                _ => throw new Exception($"Unsupported video codec.: \"{videoEncoderType}\""),
            };

        public static IEnumerable<string> GetEncoderOptions(this VideoEncoderType videoEncoderType, Settings localSettings, int outputStreamIndexWithinVideoStreams)
        {
            var options = new List<string>();
            switch (videoEncoderType)
            {
                case VideoEncoderType.Copy:
                    options.Add($"-c:v:{outputStreamIndexWithinVideoStreams} copy");
                    break;
                case VideoEncoderType.LibaomAv1:
                    options.Add($"-c:v:{outputStreamIndexWithinVideoStreams} {_libaomAv1EncoderName}");
                    if (!string.IsNullOrEmpty(localSettings.FfmpegLibaomAv1EncoderOption))
                        options.Add(localSettings.FfmpegLibaomAv1EncoderOption);
                    break;
                case VideoEncoderType.Libx264:
                    options.Add($"-c:v:{outputStreamIndexWithinVideoStreams} {_libx264EncoderName}");
                    if (!string.IsNullOrEmpty(localSettings.FfmpegLibx264EncoderOption))
                        options.Add(localSettings.FfmpegLibx264EncoderOption);
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

                    if (!string.IsNullOrEmpty(localSettings.FfmpegLibx265EncoderOption))
                        options.Add(localSettings.FfmpegLibx265EncoderOption);
                    break;
                default:
                    throw new Exception($"Unsupported video codec.: \"{videoEncoderType}\"");
            }

            return options;
        }

        public static VideoEncoderType? TryParseAsVideoEncoderType(this string? videoEncoderTypeText)
            => videoEncoderTypeText switch
            {
                null => null,
                "" => null,
                _libx264EncoderName => VideoEncoderType.Libx264,
                _libx265EncoderName => VideoEncoderType.Libx265,
                _libaomAv1EncoderName => VideoEncoderType.LibaomAv1,
                _ => null,
            };
    }
}
