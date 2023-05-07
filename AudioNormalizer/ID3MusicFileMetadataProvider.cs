using System;
using System.Collections.Generic;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;

namespace AudioNormalizer
{
    internal class ID3MusicFileMetadataProvider
        : MusicFileMetadataProvider
    {
        private readonly TransferDirection _direction;
        private readonly string? _fileFormat;

        public ID3MusicFileMetadataProvider(TransferDirection direction, string? fileFormat, string? fileExtension)
        {
            _direction = direction;
            if (fileFormat is not null)
            {
                if (fileExtension is not null)
                {
                    if (fileFormat == "wav" && fileExtension == ".wav")
                        _fileFormat = "wav";
                    else if (fileFormat == "mp3" && fileExtension == ".mp3")
                        _fileFormat = "mp3";
                    else
                        _fileFormat = null;
                }
                else
                {
                    if (fileFormat == "wav")
                        _fileFormat = "wav";
                    else if (fileFormat == "mp3")
                        _fileFormat = "mp3";
                    else
                        _fileFormat = null;
                }
            }
            else
            {
                if (fileExtension is not null)
                {
                    if (fileExtension == ".wav")
                        _fileFormat = "wav";
                    else if (fileExtension == ".mp3")
                        _fileFormat = "mp3";
                    else
                        _fileFormat = null;
                }
                else
                {
                    _fileFormat = null;
                }
            }
        }

        public override bool Supported
            => _fileFormat is not null;

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
                Lyricist = sourceMusicFileInfo.Format.Tags["text"],
                Title = sourceMusicFileInfo.Format.Tags["title"],
                Track = sourceMusicFileInfo.Format.Tags["track"],
            };
        }

        public override string? FormatMetadataFile(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            var metadataTexts = new List<string>
            {
                ";FFMETADATA1"
            };
            if (metadata.Album is not null)
                metadataTexts.Add($"album={EncodeFfmetadataValue(metadata.Album)}");
            if (metadata.AlbumArtist is not null)
                metadataTexts.Add($"album_artist={EncodeFfmetadataValue(metadata.AlbumArtist)}");
            if (metadata.Artist is not null)
                metadataTexts.Add($"artist={EncodeFfmetadataValue(metadata.Artist)}");
            if (metadata.Comment is not null)
                metadataTexts.Add($"comment={EncodeFfmetadataValue(metadata.Comment)}");
            if (metadata.Composer is not null)
                metadataTexts.Add($"composer={EncodeFfmetadataValue(metadata.Composer)}");
            if (metadata.Copyright is not null)
                metadataTexts.Add($"copyright={EncodeFfmetadataValue(metadata.Copyright)}");
            if (metadata.Date is not null)
                metadataTexts.Add($"date={EncodeFfmetadataValue(metadata.Date)}");
            if (metadata.Disc is not null)
                metadataTexts.Add($"disc={EncodeFfmetadataValue(metadata.Disc)}");
            if (metadata.Genre is not null)
                metadataTexts.Add($"genre={EncodeFfmetadataValue(metadata.Genre)}");
            if (metadata.Lyricist is not null)
                metadataTexts.Add($"text={EncodeFfmetadataValue(metadata.Lyricist)}");
            if (metadata.Title is not null)
                metadataTexts.Add($"title={EncodeFfmetadataValue(metadata.Title)}");
            if (metadata.Track is not null)
                metadataTexts.Add($"track={EncodeFfmetadataValue(metadata.Track)}");
            return $"{string.Join("\n", metadataTexts)}\n";
        }

        public override IEnumerable<(string metadataName, string metadatavalue)> GetStreamMetadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            return Array.Empty<(string metadataName, string metadatavalue)>();
        }

        public override (string encoder, IEnumerable<string> encoderOptions) GuessDefaultEncoderSpec(AudioStreamInfo sourceAudioStream)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            return _fileFormat switch
            {
                null => throw new InvalidOperationException(),
                "wav" => ("pcm_s16le", Array.Empty<string>()),
                "mp3" => ("libmp3lame", new[] { "-q:a 0" }),
                _ => throw Validation.GetFailErrorException($"_format == \"{_fileFormat}\""),
            };
        }

        public override string GuessFileFormat()
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            return _fileFormat ?? throw new InvalidOperationException();
        }
    }
}
