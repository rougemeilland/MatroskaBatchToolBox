using System;
using System.Collections.Generic;
using System.Linq;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;

namespace AudioNormalizer
{
    internal sealed class AacMusicFileMetadataProvider
        : MusicFileMetadataProvider
    {
        private readonly TransferDirection _direction;
        private readonly string? _fileFormat;

        public AacMusicFileMetadataProvider(TransferDirection direction, string? fileFormat, string? fileExtension)
        {
            _direction = direction;
            _fileFormat =
                fileFormat is null && fileExtension is null
                ? null
                : (fileFormat is null || fileFormat == "aac") && (fileExtension is null || fileExtension == ".aac") && direction == TransferDirection.Input
                ? "aac"
                : (fileFormat is null || fileFormat == "adts") && (fileExtension is null || fileExtension == ".aac") && direction == TransferDirection.Output
                ? "adts"
                : null;
        }

        public override bool Supported => _fileFormat is not null;

        public override string DefaultExtension
        {
            get
            {
                if (_fileFormat is null)
                    throw Validation.GetFailErrorException();
                return
                    _fileFormat switch
                    {
                        "aac" => ".aac",
                        "adts" => ".aac",
                        _ => throw Validation.GetFailErrorException(),
                    };
            }
        }

        public override string Format => _fileFormat ?? throw new InvalidOperationException();

        public override MusicFileMetadata GetMetadata(MovieInformation sourceMusicFileInfo)
        {
            if (_direction != TransferDirection.Input)
                throw new InvalidOperationException();

            return new()
            {
                Album = sourceMusicFileInfo.Format.Tags["album"],
                AlbumArtist = sourceMusicFileInfo.Format.Tags["album_artist"],
                Artist = sourceMusicFileInfo.Format.Tags["artist"],
                Comment = sourceMusicFileInfo.Format.Tags["comment"],
                Composer = sourceMusicFileInfo.Format.Tags["composer"],
                Copyright = sourceMusicFileInfo.Format.Tags["copyright"],
                Date = sourceMusicFileInfo.Format.Tags["date"],
                Disc = sourceMusicFileInfo.Format.Tags["disc"],
                Genre = sourceMusicFileInfo.Format.Tags["genre"],
                Lyricist = sourceMusicFileInfo.Format.Tags["TEXT"],
                Title = sourceMusicFileInfo.Format.Tags["title"],
                Track = sourceMusicFileInfo.Format.Tags["track"],
            };
        }

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateFormatMetadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            yield break;
        }

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateStreamMetadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            yield break;
        }

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateKid3Metadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            if (metadata.Album is not null)
                yield return ("album", metadata.Album);
            if (metadata.AlbumArtist is not null)
                yield return ("albumartist", metadata.AlbumArtist);
            if (metadata.Artist is not null)
                yield return ("artist", metadata.Artist);
            if (metadata.Comment is not null)
                yield return ("Comment", metadata.Comment);
            if (metadata.Composer is not null)
                yield return ("composer", metadata.Composer);
            if (metadata.Copyright is not null)
                yield return ("copyright", metadata.Copyright);
            if (metadata.Date is not null)
                yield return ("date", metadata.Date);
            if (metadata.Disc is not null)
                yield return ("discnumber", metadata.Disc);
            if (metadata.Genre is not null)
                yield return ("genre", metadata.Genre);
            if (metadata.Lyricist is not null)
                yield return ("lyricist", metadata.Lyricist);
            if (metadata.Title is not null)
                yield return ("title", metadata.Title);
            if (metadata.Track is not null)
                yield return ("track", metadata.Track);
        }

        public override (string encoder, IEnumerable<string> encoderOptions) GuessDefaultEncoderSpec(AudioStreamInfo sourceAudioStream)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            return _fileFormat switch
            {
                null => throw new InvalidOperationException(),
                "adts" => ("aac", new[] { $"-aac_coder twoloop -ab {CalculateBitRateForAac(sourceAudioStream.Channels) / 1000:F0}k" }.Append(MapAacSampleFormatOptions(sourceAudioStream.IndexWithinAudioStream, sourceAudioStream.SampleFormat))),
                _ => throw Validation.GetFailErrorException(),
            };
        }

        private static double CalculateBitRateForAac(int channels)
        {
            if (channels <= 1)
                return 128000;
            else if (channels <= 2)
                return 384000;
            else
                return 512000;
        }

        private static string MapAacSampleFormatOptions(int index, AudioSampleFormat sampleFormat)
            => sampleFormat switch
            {
                _ => $"-sample_fmt:a:{index} fltp",
            };
    }
}
