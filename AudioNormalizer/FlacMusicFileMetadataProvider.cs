using System;
using System.Collections.Generic;
using System.Linq;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;

namespace AudioNormalizer
{
    internal sealed class FlacMusicFileMetadataProvider
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

        public override bool Supported => _fileFormat is not null;
        public override string DefaultExtension => ".flac";
        public override string Format => _fileFormat ?? throw Validation.GetFailErrorException();

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

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateFormatMetadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            if (metadata.Album is not null)
                yield return ("album", metadata.Album);
            if (metadata.AlbumArtist is not null)
                yield return ("album_artist", metadata.AlbumArtist);
            if (metadata.Artist is not null)
                yield return ("artist", metadata.Artist);
#if false // flac では ffmpeg によって comment タグを正しく設定できないため、この行は無効化
            if (metadata.Comment is not null)
                yield return ("comment", metadata.Comment);
#endif
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

        public override IEnumerable<(string metadataName, string metadataValue)> EnumerateKid3Metadata(MusicFileMetadata metadata)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            if (metadata.Comment is not null)
                yield return ("comment", metadata.Comment);
        }

        public override (string encoder, IEnumerable<string> encoderOptions) GuessDefaultEncoderSpec(AudioStreamInfo sourceAudioStream)
        {
            if (_direction != TransferDirection.Output)
                throw new InvalidOperationException();

            return _fileFormat switch
            {
                null => throw new InvalidOperationException(),
                "flac" => ("flac", new[] { $"-compression_level:a:{sourceAudioStream.IndexWithinAudioStream} 12" }.Concat(MapEncoderOptions(sourceAudioStream.IndexWithinAudioStream, sourceAudioStream.SampleFormat, sourceAudioStream.BitsPerRawSample))),
                _ => throw Validation.GetFailErrorException(),
            };
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
