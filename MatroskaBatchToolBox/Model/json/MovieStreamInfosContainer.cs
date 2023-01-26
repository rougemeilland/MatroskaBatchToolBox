using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace MatroskaBatchToolBox.Model.Json
{
    public class MovieStreamInfosContainer
    {
        public MovieStreamInfosContainer()
        {
            Streams = new List<MovieStreamInfoContainer>();
        }

        [JsonPropertyName("streams")]
        public IList<MovieStreamInfoContainer> Streams { get; set; }

        internal IEnumerable<VideoStreamInfo> EnumerateVideoStreams() =>
            Streams
            .Where(stream => string.Equals(stream.CodecType, "video"))
            .Select((stream, index) => new VideoStreamInfo(stream, index));

        internal IEnumerable<AudioStreamInfo> EnumerateAudioStreams() =>
            Streams
            .Where(stream => string.Equals(stream.CodecType, "audio"))
            .Select((stream, index) => new AudioStreamInfo(stream, index));

        internal IEnumerable<SubtitleStreamInfo> EnumerateSubtitleStreams() =>
            Streams
            .Where(stream => string.Equals(stream.CodecType, "subtitle"))
            .Select((stream, index) => new SubtitleStreamInfo(stream, index));
    }
}
