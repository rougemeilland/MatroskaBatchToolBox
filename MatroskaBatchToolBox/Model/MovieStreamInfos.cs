using System.Collections.Generic;
using System.Linq;

namespace MatroskaBatchToolBox.Model
{
    public class MovieStreamInfos
    {
        public MovieStreamInfos()
        {
            streams = new List<MovieStreamInfo>();
        }

        public IList<MovieStreamInfo> streams { get; set; }

        internal IEnumerable<VideoStreamInfo> EnumerateVideoStreams() =>
            streams
            .Where(stream => string.Equals(stream.codec_type, "video"))
            .Select(stream => new VideoStreamInfo(stream));

        internal IEnumerable<AudioStreamInfo> EnumerateAudioStreams() =>
            streams
            .Where(stream => string.Equals(stream.codec_type, "audio"))
            .Select(stream => new AudioStreamInfo(stream));

        internal IEnumerable<SubtitleStreamInfo> EnumerateSubtitleStreams() =>
            streams
            .Where(stream => string.Equals(stream.codec_type, "subtitle"))
            .Select(stream => new SubtitleStreamInfo(stream));
    }
}
