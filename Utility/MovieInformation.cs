using System.Collections.Generic;
using Utility.Models.Json;

namespace Utility
{
    public class MovieInformation
    {
        public MovieInformation(MovieInformationContainer info)
        {
            VideoStreams = info.EnumerateVideoStreams();
            AudioStreams = info.EnumerateAudioStreams();
            SubtitleStreams = info.EnumerateSubtitleStreams();
        }

        public IEnumerable<VideoStreamInfo> VideoStreams { get; }
        public IEnumerable<AudioStreamInfo> AudioStreams { get; }
        public IEnumerable<SubtitleStreamInfo> SubtitleStreams { get; }
    }
}
