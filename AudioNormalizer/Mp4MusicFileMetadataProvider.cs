using System;
using System.Collections.Generic;
using System.Linq;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;

namespace AudioNormalizer
{
    internal sealed class Mp4MusicFileMetadataProvider
        : MusicFileMetadataProvider
    {
        private readonly TransferDirection _direction;
        private readonly string? _fileFormat;

        public Mp4MusicFileMetadataProvider(TransferDirection direction, string? fileFormat, string? fileExtension)
        {
            _direction = direction;
            _fileFormat =
                fileFormat is null && fileExtension is null
                ? null
                : (fileFormat is null || fileFormat.IsAnyOf("mov", "mp4", "m4a", "3gp", "3g2", "mj2")) && (fileExtension is null || fileExtension.IsAnyOf(".m4a", ".mp4"))
                ? "mp4"
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
                        "mp4" => ".m4a",
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
                Lyricist = sourceMusicFileInfo.Format.Tags["LYRICIST"],
                Title = sourceMusicFileInfo.Format.Tags["title"],
                Track = sourceMusicFileInfo.Format.Tags["track"],
            };
        }

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateFormatMetadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            yield return ("major_brand", "M4A");
            yield return ("minor_version", "512");
            yield return ("compatible_brands", "M4A isomiso2");

            if (metadata.Album is not null)
                yield return ("album", metadata.Album);
            if (metadata.AlbumArtist is not null)
                yield return ("album_artist", metadata.AlbumArtist);
            if (metadata.Artist is not null)
                yield return ("artist", metadata.Artist);
            if (metadata.Comment is not null)
                yield return ("comment", metadata.Comment);
            if (metadata.Composer is not null)
                yield return ("composer", metadata.Composer);
            if (metadata.Copyright is not null)
                yield return ("copyright", metadata.Copyright);
            if (metadata.Date is not null)
                yield return ("date", metadata.Date);
            if (metadata.Disc is not null)
                yield return ("disc", metadata.Disc);
            if (metadata.Genre is not null)
                yield return ("genre", metadata.Genre);
            if (metadata.Lyricist is not null)
                yield return ("lyricist", metadata.Lyricist);
            if (metadata.Title is not null)
                yield return ("title", metadata.Title);
            if (metadata.Track is not null)
                yield return ("track", metadata.Track);
        }

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateStreamMetadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            yield break;
        }

        public override (string encoder, IEnumerable<string> encoderOptions) GuessDefaultEncoderSpec(AudioStreamInfo sourceAudioStream)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            return _fileFormat switch
            {
                null => throw new InvalidOperationException(),
                "mp4" => ("aac", new[] { $"-aac_coder twoloop -ab {CalculateBitRateForAac(sourceAudioStream.Channels) / 1000:F0}k" }.Append(MapAacSampleFormatOptions(sourceAudioStream.IndexWithinAudioStream, sourceAudioStream.SampleFormat))),
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
