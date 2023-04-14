using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Utility.Models.Json
{
    public class MovieInformationContainer
    {
        [JsonPropertyName("format")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public MovieFormatContainer? Format { get; set; }

        [JsonPropertyName("streams")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<MovieStreamInfoContainer>? Streams { get; set; }

        [JsonPropertyName("chapters")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IList<MovieChapterContainer>? Chapters { get; set; }

        internal IEnumerable<MovieChapterContainer> EnumerateChapters()
            => Chapters ?? throw new Exception("\"chapters\" property does not exist.");

        internal IEnumerable<MovieStreamInfoContainer> EnumerateVideoStreams()
            => (Streams ?? throw new Exception("\"streams\" property does not exist."))
            .Where(stream => stream.CodecType == "video");

        internal IEnumerable<MovieStreamInfoContainer> EnumerateAudioStreams()
            => (Streams ?? throw new Exception("\"streams\" property does not exist."))
            .Where(stream => stream.CodecType == "audio");

        internal IEnumerable<MovieStreamInfoContainer> EnumerateSubtitleStreams()
            => (Streams ?? throw new Exception("\"streams\" property does not exist."))
            .Where(stream => stream.CodecType == "subtitle");

        internal IEnumerable<MovieStreamInfoContainer> EnumerateDataStreams()
            => (Streams ?? throw new Exception("\"streams\" property does not exist."))
            .Where(stream => stream.CodecType == "data");

        internal IEnumerable<MovieStreamInfoContainer> EnumerateAttachmentStreams()
            => (Streams ?? throw new Exception("\"streams\" property does not exist."))
            .Where(stream => stream.CodecType == "attachment");
    }
}
