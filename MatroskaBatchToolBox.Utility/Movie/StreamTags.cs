using System;
using MatroskaBatchToolBox.Utility.Models.Json;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class StreamTags
    {
        internal StreamTags(MovieStreamTagsContainer? tags)
        {
            var durationText = tags?.Duration;
            Duration =
                durationText is null || !durationText.TryParse(false, out TimeSpan duration)
                ? null
                : duration;
            Title = tags?.Title;
            Language = NormalizeLanguageCode(tags?.Language);
        }

        /// <summary>
        /// ストリームのエンコーダーを表すメタデータ。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>ffmpeg 固有のメタデータなので、必ず存在するとは限らない。</item>
        /// </list>
        /// </remarks>
        public string? Encoder { get; }

        /// <summary>
        /// ストリームの長さを表す時間を表すメタデータ。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>ffmpeg 固有のメタデータなので、必ず存在するとは限らない。</item>
        /// <item>もし、ffmpeg の "-map_metadata -1" オプションによってメタデータが削除された場合でも、このメタデータは削除されない。</item>
        /// </list>
        /// </remarks>
        public TimeSpan? Duration { get; }

        public string? Title { get; }

        public string? Language { get; }

        private static string? NormalizeLanguageCode(string? language)
            // 言語コード "und" は未定義 (null) とみなす。
            => language == "und" ? null : language;

    }
}
