using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Utility.Models.Json
{
    public class MovieInformationContainer
    {
        public MovieInformationContainer()
        {
            Streams = new List<MovieStreamInfoContainer>();
            Chapters = new List<MovieChapterContainer>();
        }

        [JsonPropertyName("streams")]
        public IList<MovieStreamInfoContainer> Streams { get; set; }

        [JsonPropertyName("chapters")]
        public IList<MovieChapterContainer> Chapters { get; set; }

        internal IEnumerable<ChapterInfo> EnumerateChapters()
        {
            return
                Chapters
                .Select(chapter => new ChapterInfo(chapter));
        }

        internal IEnumerable<VideoStreamInfo> EnumerateVideoStreams() =>
            Streams
            .Where(stream => string.Equals(stream.CodecType, "video", StringComparison.Ordinal))
            .Select((stream, index) => new VideoStreamInfo(stream, index));

        internal IEnumerable<AudioStreamInfo> EnumerateAudioStreams() =>
            Streams
            .Where(stream => string.Equals(stream.CodecType, "audio", StringComparison.Ordinal))
            .Select((stream, index) => new AudioStreamInfo(stream, index));

        internal IEnumerable<SubtitleStreamInfo> EnumerateSubtitleStreams() =>
            Streams
            .Where(stream => string.Equals(stream.CodecType, "subtitle", StringComparison.Ordinal))
            .Select((stream, index) => new SubtitleStreamInfo(stream, index));

        internal IEnumerable<DataStreamInfo> EnumerateDataStreams() =>
            Streams
            .Where(stream => string.Equals(stream.CodecType, "data", StringComparison.Ordinal))
            .Select((stream, index) => new DataStreamInfo(stream, index));

        internal IEnumerable<AttachmentStreamInfo> EnumerateAttachmentStreams() =>
            Streams
            .Where(stream => string.Equals(stream.CodecType, "attachment", StringComparison.Ordinal))
            .Select((stream, index) => new AttachmentStreamInfo(stream, index));
    }
}
