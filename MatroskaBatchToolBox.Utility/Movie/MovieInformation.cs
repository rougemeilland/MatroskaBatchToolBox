using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using MatroskaBatchToolBox.Utility.Models.Json;

namespace MatroskaBatchToolBox.Utility.Movie
{
    public class MovieInformation
    {
        private readonly MovieFormat? _format;
        private readonly IEnumerable<ChapterInfo>? _chapters;
        private readonly IEnumerable<VideoStreamInfo>? _videoStreams;
        private readonly IEnumerable<AudioStreamInfo>? _audioStreams;
        private readonly IEnumerable<SubtitleStreamInfo>? _subtitleStreams;
        private readonly IEnumerable<DataStreamInfo>? _dataStreams;
        private readonly IEnumerable<AttachmentStreamInfo>? _attachmentStreams;

        private MovieInformation(MovieInformationContainer info)
        {
            _format = info.Format is null ? null : new MovieFormat(info.Format);
            _chapters = info.Chapters?.Select(chapter => new ChapterInfo(chapter));
            _videoStreams = info.Streams?.Where(stream => stream.CodecType == "video").Select((stream, index) => new VideoStreamInfo(stream, index)).ToList();
            _audioStreams = info.Streams?.Where(stream => stream.CodecType == "audio").Select((stream, index) => new AudioStreamInfo(stream, index)).ToList();
            _subtitleStreams = info.Streams?.Where(stream => stream.CodecType == "subtitle").Select((stream, index) => new SubtitleStreamInfo(stream, index)).ToList();
            _dataStreams = info.Streams?.Where(stream => stream.CodecType == "data").Select((stream, index) => new DataStreamInfo(stream, index)).ToList();
            _attachmentStreams = info.Streams?.Where(stream => stream.CodecType == "attachment").Select((stream, index) => new AttachmentStreamInfo(stream, index)).ToList();
        }

        public MovieFormat Format => _format ?? throw new Exception("ffprobe returned no format information.");
        public IEnumerable<ChapterInfo> Chapters => _chapters ?? throw new Exception("\"chapters\" property does not exist.");
        public IEnumerable<VideoStreamInfo> VideoStreams => _videoStreams ?? throw new Exception("\"streams\" property does not exist.");
        public IEnumerable<AudioStreamInfo> AudioStreams => _audioStreams ?? throw new Exception("\"streams\" property does not exist.");
        public IEnumerable<SubtitleStreamInfo> SubtitleStreams => _subtitleStreams ?? throw new Exception("\"streams\" property does not exist.");
        public IEnumerable<DataStreamInfo> DataStreams => _dataStreams ?? throw new Exception("\"streams\" property does not exist.");
        public IEnumerable<AttachmentStreamInfo> AttachmentStreams => _attachmentStreams ?? throw new Exception("\"streams\" property does not exist.");

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
