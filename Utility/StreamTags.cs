using System;
using Utility.Models.Json;

namespace Utility
{
    public class StreamTags
    {
        internal StreamTags(MovieStreamTagsContainer? tags)
        {
            var durationText = tags?.Duration;
            Duration = durationText is not null ? Time.ParseTime(durationText, false) : null;
            Title = tags?.Title;
            Language = NormalizeLanguageCode(tags?.Language);
        }

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
        {
            // 言語コード "und" は未定義 (null) とみなす。
            return
                string.Equals(language, "und", StringComparison.Ordinal)
                ? null
                : language;
        }
    }
}
