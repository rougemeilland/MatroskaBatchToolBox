using System.Collections.Generic;
using Utility.Models.Json;

namespace Utility
{
    public class MovieInformation
    {
        public MovieInformation(MovieInformationContainer info)
        {
            Chapters = info.EnumerateChapters();
            VideoStreams = info.EnumerateVideoStreams();
            AudioStreams = info.EnumerateAudioStreams();
            SubtitleStreams = info.EnumerateSubtitleStreams();
        }

        public IEnumerable<ChapterInfo> Chapters { get; set; }
        public IEnumerable<VideoStreamInfo> VideoStreams { get; }
        public IEnumerable<AudioStreamInfo> AudioStreams { get; }
        public IEnumerable<SubtitleStreamInfo> SubtitleStreams { get; }
    }
}
