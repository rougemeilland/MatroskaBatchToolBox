using System;
using System.Collections.Generic;
using System.Linq;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;

namespace AudioNormalizer
{
    internal class FlacMusicFileMetadataProvider
        : MusicFileMetadataProvider
    {
        private readonly TransferDirection _direction;
        private readonly string? _fileFormat;

        public FlacMusicFileMetadataProvider(TransferDirection direction, string? fileFormat, string? fileExtension)
        {
            _direction = direction;
            _fileFormat =
                fileFormat is null && fileExtension is null
                ? null
                : fileFormat is null or "flac" && fileExtension is null or ".flac"
                ? "flac"
                : null;
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
                Lyricist = sourceMusicFileInfo.Format.Tags["lyricist"],
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
                metadataTexts.Add($"lyricist={EncodeFfmetadataValue(metadata.Lyricist)}");
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
                "flac" => ("flac", new[] { $"-compression_level:a:{sourceAudioStream.IndexWithinAudioStream} 12" }.Concat(MapEncoderOptions(sourceAudioStream.IndexWithinAudioStream, sourceAudioStream.SampleFormat, sourceAudioStream.BitsPerRawSample))),
                _ => throw Validation.GetFailErrorException($"_format == \"{_fileFormat}\""),
            };
        }

        public override string GuessFileFormat()
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            return _fileFormat ?? throw new InvalidOperationException();
        }

        private static IEnumerable<string> MapEncoderOptions(int index, AudioSampleFormat sampleFormat, int? bitsPerRawSample)
        {
            if (sampleFormat is AudioSampleFormat.S64 or AudioSampleFormat.S64P or AudioSampleFormat.DBL or AudioSampleFormat.DBLP ||
                bitsPerRawSample is not null and > 24)
            {
                yield return $"-strict:a:{index} experimental";
            }

            switch (sampleFormat)
            {
                case AudioSampleFormat.U8:
                case AudioSampleFormat.U8P:
                case AudioSampleFormat.S16:
                case AudioSampleFormat.S16P:
                    yield return $"-sample_fmt:a:{index} s16";
                    break;
                case AudioSampleFormat.S32:
                case AudioSampleFormat.S32P:
                case AudioSampleFormat.S64:
                case AudioSampleFormat.S64P:
                case AudioSampleFormat.FLT:
                case AudioSampleFormat.FLTP:
                case AudioSampleFormat.DBL:
                case AudioSampleFormat.DBLP:
                    yield return $"-sample_fmt:a:{index} s32";
                    break;
                default:
                    if (bitsPerRawSample is null or > 16)
                        yield return $"-sample_fmt:a:{index} s32";
                    else
                        yield return $"-sample_fmt:a:{index} s16";
                    break;
            }

            if (sampleFormat is AudioSampleFormat.FLT or AudioSampleFormat.FLTP)
                yield return $" -bits_per_raw_sample:a:{index} 24";
            else if (bitsPerRawSample != null)
                yield return $" -bits_per_raw_sample:a:{index} {bitsPerRawSample}";
        }
    }
}
