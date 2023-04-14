using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MatroskaBatchToolBox.Utility.Models.Json;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class MovieInformation
    {
        private readonly MovieInformationContainer _info;

        private MovieInformation(MovieInformationContainer info)
            => _info = info;

        public MovieFormat Format => new(_info.Format ?? throw new Exception("ffprobe returned no format information."));
        public IEnumerable<ChapterInfo> Chapters => _info.EnumerateChapters().Select(chapter => new ChapterInfo(chapter));
        public IEnumerable<VideoStreamInfo> VideoStreams => _info.EnumerateVideoStreams().Select((stream, index) => new VideoStreamInfo(stream, index));
        public IEnumerable<AudioStreamInfo> AudioStreams => _info.EnumerateAudioStreams().Select((stream, index) => new AudioStreamInfo(stream, index));
        public IEnumerable<SubtitleStreamInfo> SubtitleStreams => _info.EnumerateSubtitleStreams().Select((stream, index) => new SubtitleStreamInfo(stream, index));
        public IEnumerable<DataStreamInfo> DataStreams => _info.EnumerateDataStreams().Select((stream, index) => new DataStreamInfo(stream, index));
        public IEnumerable<AttachmentStreamInfo> AttachmentStreams => _info.EnumerateAttachmentStreams().Select((stream, index) => new AttachmentStreamInfo(stream, index));

        public static MovieInformation ParseFromJson(string jsonText)
        {
            var movieInformationContainer =
                JsonSerializer.Deserialize<MovieInformationContainer>(
                    jsonText,
                    new JsonSerializerOptions { AllowTrailingCommas = true, PropertyNameCaseInsensitive = true })
                ?? throw new Exception("ffprobe returned no information.");
            return new MovieInformation(movieInformationContainer);
        }
    }
}
