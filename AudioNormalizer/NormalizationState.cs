using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using MatroskaBatchToolBox.Utility.Interprocess;
using MatroskaBatchToolBox.Utility.Movie;
using Palmtree;
using Palmtree.IO;
using Palmtree.Linq;
using Palmtree.Text;

namespace AudioNormalizer
{
    internal sealed class NormalizationState
    {
        private const string _ffmpegPathEnvironmentVariableName = "FFMPEG_PATH";
        private static readonly FilePath _ffmpegCommandFile;
        private static readonly FilePath _metaeditCommandFile;
        private static readonly Lazy<FilePath?> _kid3ConsoleCommandFile =
                new(() =>
                    {
                        var kid3ConsoleCommandFilePath = ProcessUtility.WhereIs("kid3-cli");
                        return kid3ConsoleCommandFilePath is not null ? new FilePath(kid3ConsoleCommandFilePath) : null;
                    });
        private static readonly Dictionary<string, int> _coverArtPictureTypes =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { "Other", 0 },
                { "32x32 pixels 'file icon'", 1 },
                { "Other file icon", 2 },
                { "Cover (front)", 3 },
                { "Cover (back)", 4 },
                { "Leaflet page", 5 },
                { "Media (e.g. label side of CD)", 6 },
                { "Lead artist/lead performer/soloist", 7 },
                { "Artist/performer", 8 },
                { "Conductor", 9 },
                { "Band/Orchestra", 10 },
                { "Composer", 11 },
                { "Lyricist/text writer", 12 },
                { "Recording Location", 13 },
                { "During recording", 14 },
                { "During performance", 15 },
                { "Movie/video screen capture", 16 },
                { "A bright coloured fish", 17 },
                { "Illustration", 18 },
                { "Band/artist logotype", 19 },
                { "Publisher/Studio logotype", 20 }
            };

        private readonly IMusicFileMetadataProvider? _inputFlleProvider;
        private readonly IMusicFileMetadataProvider? _outputFlleProvider;
        private readonly string? _audioEncoder;
        private readonly string? _audioEncoderOption;
        private readonly bool _doOverwrite;
        private readonly bool _verbose;
        private readonly bool _disableVideoStream;
        private readonly bool _allowDuplicateCoverArtPictureType;
        private readonly Action<ISequentialInputByteStream, ISequentialOutputByteStream, ulong> _streamCopier;
        private readonly Action<LogCategory, string> _messageReporter;
        private readonly Action<string, LogCategory, string> _messageReporter2;

        static NormalizationState()
        {
            _ffmpegCommandFile = new FilePath(ProcessUtility.WhereIs("ffmpeg") ?? throw new FileNotFoundException($"ffmpeg is not installed."));
            var metaeditCommandName = "metaedit";
            _metaeditCommandFile = new FilePath(ProcessUtility.WhereIs(metaeditCommandName) ?? throw new FileNotFoundException($"{metaeditCommandName} is not installed."));
        }

        public NormalizationState(
            IMusicFileMetadataProvider? inputFlleProvider,
            IMusicFileMetadataProvider? outputFlleProvider,
            string? audioEncoder,
            string? audioEncoderOption,
            bool doOverwrite,
            bool verbose,
            bool disableVideoStream,
            bool allowDuplicateCoverArtPictureType,
            Action<ISequentialInputByteStream, ISequentialOutputByteStream, ulong> streamCopier,
            Action<LogCategory, string> messageReporter,
            Action<string, LogCategory, string> messageReporter2)
        {
            _inputFlleProvider = inputFlleProvider;
            _outputFlleProvider = outputFlleProvider;
            _doOverwrite = doOverwrite;
            _verbose = verbose;
            _disableVideoStream = disableVideoStream;
            _allowDuplicateCoverArtPictureType = allowDuplicateCoverArtPictureType;
            _streamCopier = streamCopier;
            _messageReporter = messageReporter;
            _messageReporter2 = messageReporter2;
            _audioEncoder = audioEncoder;
            _audioEncoderOption = audioEncoderOption;
        }

        public void NormalizeFile(string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile)
        {
            // 入力動画ファイルの情報を取得する
            var movieInformation = GetMovieInformation(inputFormat, inputFile);

            if (movieInformation.AudioStreams.None())
            {
                // 入力ファイルに音声ストリームが存在しない場合

                CopyFile(inputFormat, inputFile, outputFormat, outputFile);
            }
            else if (movieInformation.VideoStreams.Any(stream => stream.CodecName.IsNoneOf("png", "mjpeg") && !stream.Disposition.AttachedPic))
            {
                // 入力ファイルにオーディオストリームが存在し、かつサムネイル以外のビデオストリームが含まれている場合

                NormalizeMovieFile(movieInformation, inputFormat, inputFile, outputFormat, outputFile);
            }
            else if (movieInformation.AudioStreams.Skip(1).None())
            {
                // 入力ファイルにオーディオストリームが1つだけ存在し、かつサムネイル以外のビデオストリームが含まれてていない場合

                NormalizeMusicFile(movieInformation, inputFile, outputFile);
            }
            else
            {
                // 上記以外の場合

                throw new NotSupportedException("An input file whose audio cannot be normalized.");
            }
        }

        private void CopyFile(string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile)
        {
            // 出力先に単純コピーする

            _messageReporter(LogCategory.Information, "Since the video file does not contain an audio track, simply copy it.");
            if (!string.IsNullOrEmpty(_audioEncoder))
                _messageReporter(LogCategory.Warning, "The \"--encoder\" option is ignored for files that do not have an audio stream.");
            if (!string.IsNullOrEmpty(_audioEncoderOption))
                _messageReporter(LogCategory.Warning, "The \"--encoder_option\" option is ignored for files that do not have an audio stream.");

            if (_verbose)
            {
                if (inputFormat is not null)
                    _messageReporter(LogCategory.Information, $"  Input file format: {inputFormat}");
                _messageReporter(LogCategory.Information, $"  Input file format: \"{inputFile.FullName}\"");
                if (outputFormat is not null)
                    _messageReporter(LogCategory.Information, $"  Output file format: {outputFormat}");
                _messageReporter(LogCategory.Information, $"  Output file format: \"{outputFile.FullName}\"");
            }

            using var instream = inputFile.OpenRead();
            using var outstream = outputFile.Create();
            _streamCopier(instream, outstream, inputFile.Length);

            if (_verbose)
                _messageReporter(LogCategory.Information, "Copy finished.");
        }

        private void NormalizeMovieFile(MovieInformation movieInformation, string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile)
        {
            // 入力ファイルが動画ファイルであると判断し、オーディオストリームの正規化と共に以下の処理を行う。
            // - オーディオストリームのコーデックを opus/vorbis に変更する
            // - 以下の情報を維持する。
            //   * チャプターのメタデータ (title)
            //   * ストリームのメタデータ (language/title)
            //   * ストリームの disposition (default/force/title/attached_pic)
            // - 上記以外のメタデータを消去する。

            if (_disableVideoStream)
                _messageReporter(LogCategory.Warning, "The \"--video_disable\" option is ignored for movie file normalization.");

            // 中間一時ファイルを作成する
            var temporaryIntermediateFile1 = FilePath.CreateTemporaryFile(suffix: ".mkv");
            var temporaryIntermediateFileFormat1 = (string?)null;
            var temporaryIntermediateFile2 = FilePath.CreateTemporaryFile(suffix: ".mkv");
            var temporaryIntermediateFileFormat2 = (string?)null;
            if (_verbose)
            {
                _messageReporter(LogCategory.Information, $"Temprary file is created.: \"{temporaryIntermediateFile2.FullName}\"");
            }

            try
            {
                // 正規化の前に、動画ファイルのオーディオストリームの encoder タグを消去する
                // ※正規化の際にオーディオストリームの encoder タグが上書きされないので、最終的に encoder タグの値が不適切な値になってしまうのを防ぐため。
                if (_verbose)
                {
                    _messageReporter(LogCategory.Information, "Remove audio stream tags.");
                    if (inputFormat is not null)
                        _messageReporter(LogCategory.Information, $"  Input file format: {inputFormat}");
                    _messageReporter(LogCategory.Information, $"  Input file format: \"{inputFile.FullName}\"");
                    if (temporaryIntermediateFileFormat2 is not null)
                        _messageReporter(LogCategory.Information, $"  Output file format: {temporaryIntermediateFileFormat1}");
                    _messageReporter(LogCategory.Information, $"  Output file format: \"{temporaryIntermediateFile1.FullName}\"");
                }

                RemoveMetadataOfMovieFile(
                    movieInformation,
                    inputFormat,
                    inputFile,
                    temporaryIntermediateFileFormat1,
                    temporaryIntermediateFile1);

                // 動画ファイルの音声を正規化して中間一時ファイルに保存する
                if (_verbose)
                {
                    _messageReporter(LogCategory.Information, "Start audio normalization.");
                    if (temporaryIntermediateFileFormat1 is not null)
                        _messageReporter(LogCategory.Information, $"  Input file format: {temporaryIntermediateFileFormat1}");
                    _messageReporter(LogCategory.Information, $"  Input file format: \"{temporaryIntermediateFile1.FullName}\"");
                    if (temporaryIntermediateFileFormat2 is not null)
                        _messageReporter(LogCategory.Information, $"  Output file format: {temporaryIntermediateFileFormat2}");
                    _messageReporter(LogCategory.Information, $"  Output file format: \"{temporaryIntermediateFile2.FullName}\"");
                }

                NormalizeAudioStreamOfMovieFile(
                    movieInformation,
                    temporaryIntermediateFileFormat1,
                    temporaryIntermediateFile1,
                    temporaryIntermediateFileFormat2,
                    temporaryIntermediateFile2);

                if (_verbose)
                    _messageReporter(LogCategory.Information, "Audio normalization finished.");

                // 動画ファイルの余分なメタデータを消去する
                if (_verbose)
                {
                    _messageReporter(LogCategory.Information, "Start restoring metadata.");
                    if (temporaryIntermediateFileFormat2 is not null)
                        _messageReporter(LogCategory.Information, $"  Input file format: {temporaryIntermediateFileFormat2}");
                    _messageReporter(LogCategory.Information, $"  Input file format: \"{temporaryIntermediateFile2.FullName}\"");
                    if (outputFormat is not null)
                        _messageReporter(LogCategory.Information, $"  Output file format: {outputFormat}");
                    _messageReporter(LogCategory.Information, $"  Output file format: \"{outputFile.FullName}\"");
                }

                SetMetadataOfMovieFile(
                    movieInformation,
                    temporaryIntermediateFileFormat2,
                    temporaryIntermediateFile2,
                    outputFormat,
                    outputFile);

                if (_verbose)
                    _messageReporter(LogCategory.Information, "Audio normalization finished.");
            }
            finally
            {
                foreach (var temporaryIntermediateFile in new[] { temporaryIntermediateFile1, temporaryIntermediateFile2 })
                {
                    if (temporaryIntermediateFile is not null)
                    {
                        temporaryIntermediateFile.SafetyDelete();
                        if (_verbose)
                            _messageReporter(LogCategory.Information, $"Temporary file is deleted.: \"{temporaryIntermediateFile.FullName}\"");
                    }
                }
            }
        }

        private void NormalizeMusicFile(MovieInformation musicFileInfo, FilePath inputFile, FilePath outputFile)
        {
            Validation.Assert(inputFile.Extension != ".tmp");
            Validation.Assert(outputFile.Extension != ".tmp");

            // 入力ファイルが音楽ファイルであると判断し、オーディオストリームの正規化と共に以下の処理を行う。
            // - オーディオストリームのコーデックを出力先フォーマットに応じて変更する
            // - 以下の情報を維持する維持する。
            //   * チャプターのメタデータ (title)
            //   * ストリームのメタデータ (language/title)
            //   * ストリームの disposition (default/force/title/attached_pic)
            // - 上記以外のメタデータを消去する。

            if (_inputFlleProvider is null)
                throw new ApplicationException("Input file format is unknown.");

            if (_outputFlleProvider is null)
                throw new ApplicationException("Output file format is unknown.");

            // 中間一時ファイルを作成する
            var temporaryIntermediateFile = FilePath.CreateTemporaryFile(suffix: _outputFlleProvider.DefaultExtension);
            if (_verbose)
            {
                _messageReporter(LogCategory.Information, $"Temprary file is created.: \"{temporaryIntermediateFile.FullName}\"");
            }

            try
            {
                var metadata = _inputFlleProvider.GetMetadata(musicFileInfo);

                // 音楽ファイルを正規化して中間一時ファイルに保存する
                if (_verbose)
                {
                    _messageReporter(LogCategory.Information, "Start audio normalization.");
                    _messageReporter(LogCategory.Information, $"  Input file format: {_inputFlleProvider.Format}");
                    _messageReporter(LogCategory.Information, $"  Input file format: \"{inputFile.FullName}\"");
                    _messageReporter(LogCategory.Information, $"  Output file format: {_outputFlleProvider.Format}");
                    _messageReporter(LogCategory.Information, $"  Output file format: \"{temporaryIntermediateFile.FullName}\"");
                }

                NormalizeAudioStreamOfMusicFile(
                    musicFileInfo,
                    null,
                    inputFile,
                    null,
                    temporaryIntermediateFile);

                if (_verbose)
                    _messageReporter(LogCategory.Information, "Audio normalization finished.");

                // 音楽ファイルのメタデータを復元する
                if (_verbose)
                {
                    _messageReporter(LogCategory.Information, "Start restoring metadata.");
                    _messageReporter(LogCategory.Information, $"  Input file format: {AlternativeFormatForInput(_outputFlleProvider.Format)}");
                    _messageReporter(LogCategory.Information, $"  Input file format: \"{temporaryIntermediateFile.FullName}\"");
                    _messageReporter(LogCategory.Information, $"  Output file format: {_outputFlleProvider.Format}");
                    _messageReporter(LogCategory.Information, $"  Output file format: \"{outputFile.FullName}\"");
                }

                var normalizedMusicFileInfo =
                    GetMovieInformation(
                        null,
                        temporaryIntermediateFile);

                SetMetadataOfMusicFile(
                    musicFileInfo,
                    normalizedMusicFileInfo,
                    null,
                    temporaryIntermediateFile,
                    null,
                    outputFile,
                    metadata);

                if (_verbose)
                    _messageReporter(LogCategory.Information, "Audio normalization finished.");
            }
            finally
            {
                if (temporaryIntermediateFile is not null)
                {
                    temporaryIntermediateFile.SafetyDelete();
                    if (_verbose)
                        _messageReporter(LogCategory.Information, $"Temporary file is deleted.: \"{temporaryIntermediateFile.FullName}\"");
                }
            }
        }

        private MovieInformation GetMovieInformation(string? inputFormat, FilePath inputFile)
        {
            if (_verbose)
                _messageReporter(LogCategory.Information, $"Probe movie information.: \"{inputFile.FullName}\"");
            try
            {
                return
                    Command.GetMovieInformation(
                        inputFormat,
                        inputFile,
                        MovieInformationType.Chapters | MovieInformationType.Streams | MovieInformationType.Format,
                        (level, message) =>
                        {
                            if (level != LogCategory.Information || _verbose)
                                _messageReporter2("ffprobe", level, message);
                        });
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Failed to get movie information.", ex);
            }
        }

        private void RemoveMetadataOfMovieFile(MovieInformation movieFileInformation, string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile)
        {
            var metaeditCommandParameters = new List<string>();

            if (_verbose)
                metaeditCommandParameters.Add("-v");
            metaeditCommandParameters.Add("-f");

            // 入力方法の指定
            if (inputFormat is not null)
                metaeditCommandParameters.Add($"-if {inputFormat.EncodeCommandLineArgument()}");
            metaeditCommandParameters.Add($"-i {inputFile.FullName.EncodeCommandLineArgument()}");

            // ストリームのメタデータの指定
            foreach (var stream in movieFileInformation.AudioStreams)
            {
                // ストリームのメタデータの指定 (オーディオの正規化により変化するため、オーディオストリームの "encoder" メタデータは除く)
                foreach (var metadataName in new[] { "encoder", "duration" })
                {
                    var metadataValue = stream.Tags[metadataName];
                    if (metadataValue is not null)
                        metaeditCommandParameters.Add($"-s:a:{stream.IndexWithinAudioStream} {metadataName.EncodeCommandLineArgument()}=");
                }
            }

            // 出力方法の指定
            if (outputFormat is not null)
                metaeditCommandParameters.Add($"-of {outputFormat.EncodeCommandLineArgument()}");
            metaeditCommandParameters.Add($"-o {outputFile.FullName.EncodeCommandLineArgument()}");

            var metaeditCommandParameterText = string.Join(" ", metaeditCommandParameters);
            var exitCode =
                Command.ExecuteCommand(
                    _metaeditCommandFile,
                    metaeditCommandParameterText,
                    null,
                    null,
                    null,
                    null,
                    null,
                    (level, message) =>
                    {
                        if (level != LogCategory.Information)
                            _messageReporter(level, message);
                    },
                    null);
            if (exitCode != 0)
                throw new ApplicationException($"An error occurred in the \"{Path.GetFileNameWithoutExtension(_metaeditCommandFile.Name)}\" command. : exit-code={exitCode}");
        }

        private void NormalizeAudioStreamOfMovieFile(MovieInformation movieFileInformation, string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile)
        {
            var (audioEncoder, defaultAudioEncoderOptions) = SelectAudioEncoder(movieFileInformation, _audioEncoder, _verbose, _messageReporter);
            if (_verbose)
                _messageReporter(LogCategory.Information, $"\"{audioEncoder}\" is selected as the audio encoder.");

            ExecuteFfpegNormaize(
                inputFormat,
                inputFile,
                outputFormat,
                outputFile,
                audioEncoder,
                string.IsNullOrEmpty(_audioEncoderOption)
                    ? defaultAudioEncoderOptions
                    : new[] { _audioEncoderOption },
                false);

            static (string audioEncoder, IEnumerable<string> audioEncoderDefaultOptions) SelectAudioEncoder(MovieInformation movieFileInformation, string? specifiedEncoder, bool verbose, Action<LogCategory, string> messageReporter)
            {
                if (specifiedEncoder == "aac")
                {
                    var provider = new AacMusicFileMetadataProvider(TransferDirection.Output, "aac", null);
                    return ("adts", movieFileInformation.AudioStreams.SelectMany(stream => provider.GuessDefaultEncoderSpec(stream).encoderOptions).ToArray());
                }
                else if (specifiedEncoder == "flac")
                {
                    var provider = new FlacMusicFileMetadataProvider(TransferDirection.Output, "flac", null);
                    return ("flac", movieFileInformation.AudioStreams.SelectMany(stream => provider.GuessDefaultEncoderSpec(stream).encoderOptions).ToArray());
                }
                else if (specifiedEncoder == "vorbis")
                {
                    var provider = new OggMusicFileMetadataProvider(TransferDirection.Output, "ogg", null);
                    return ("libvorbis", movieFileInformation.AudioStreams.SelectMany(stream => provider.GuessDefaultEncoderSpec(stream).encoderOptions).ToArray());
                }
                else if (specifiedEncoder is "opus" or null)
                {
                    // チャンネルレイアウトが libopus によってサポートされていないオーディオストリームを抽出する
                    var audioStreamsNotSupportedByLibOpus =
                        movieFileInformation.AudioStreams
                        .Where(stream => stream.ChannelLayout is not null && stream.ChannelLayout.IsAnyOf("5.0(side)", "5.1(side)"))
                        .ToList();

                    if (audioStreamsNotSupportedByLibOpus.Count > 0)
                    {
                        if (verbose)
                            messageReporter(LogCategory.Warning, "Some audio streams contain a channel layout that is not supported by \"opus\", so \"vorbis\" will be used instead.");
                        var provider = new OggMusicFileMetadataProvider(TransferDirection.Output, "ogg", null);
                        return ("libvorbis", movieFileInformation.AudioStreams.SelectMany(stream => provider.GuessDefaultEncoderSpec(stream).encoderOptions).ToArray());
                    }
                    else
                    {
                        var provider = new OggMusicFileMetadataProvider(TransferDirection.Output, "opus", null);
                        return ("libopus", movieFileInformation.AudioStreams.SelectMany(stream => provider.GuessDefaultEncoderSpec(stream).encoderOptions).ToArray());
                    }
                }
                else
                {
                    throw new ApplicationException($"Audio encoder \"{specifiedEncoder}\" is not supported.");
                }
            }
        }

        private void NormalizeAudioStreamOfMusicFile(MovieInformation musicFileInformation, string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile)
        {
            Validation.Assert(_inputFlleProvider is not null);
            Validation.Assert(_outputFlleProvider is not null);
            Validation.Assert(musicFileInformation.AudioStreams.Take(2).Count() == 1);

            var sourceAudioStream = musicFileInformation.AudioStreams.First();
            var defaultEncoderSpec = _outputFlleProvider.GuessDefaultEncoderSpec(sourceAudioStream);
            var encoder = _audioEncoder ?? defaultEncoderSpec.encoder;
            var encoderOptions = _audioEncoderOption is not null ? new[] { _audioEncoderOption } : defaultEncoderSpec.encoderOptions;
            ExecuteFfpegNormaize(inputFormat, inputFile, outputFormat, outputFile, encoder, encoderOptions, true);
        }

        private void ExecuteFfpegNormaize(string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile, string audioEncoder, IEnumerable<string> audioEncoderOptions, bool exceptVideoStream)
        {
            var ffmpegCommandFilePath =
                ProcessUtility.WhereIs("ffmpeg")
                ?? throw new ApplicationException("ffmpeg command not found.");
            Environment.SetEnvironmentVariable(_ffmpegPathEnvironmentVariableName, ffmpegCommandFilePath);
            var normalizeCommandFilePath =
                ProcessUtility.WhereIs("ffmpeg-normalize")
                ?? throw new ApplicationException("ffmpeg-normalize command not found.");
            var normalizerCommandParameters = new List<string>();
            if (_verbose)
                normalizerCommandParameters.Add("-v");
            normalizerCommandParameters.Add("-f");
            normalizerCommandParameters.Add("-pr");
            normalizerCommandParameters.Add("--keep-loudness-range-target");
            if (inputFormat is not null)
                normalizerCommandParameters.Add($"-ei={string.Join(" ", ["-f", $"{inputFormat.EncodeCommandLineArgument()}"]).EncodeCommandLineArgument()}");
            normalizerCommandParameters.Add(inputFile.FullName.EncodeCommandLineArgument());
            if (exceptVideoStream)
                normalizerCommandParameters.Add("-vn");
            normalizerCommandParameters.Add($"-c:a {audioEncoder.EncodeCommandLineArgument()}");
            if (audioEncoderOptions.Any())
                normalizerCommandParameters.Add($"-e={string.Join(" ", audioEncoderOptions).EncodeCommandLineArgument()}");
            if (outputFormat is not null)
                normalizerCommandParameters.Add($"-ofmt {outputFormat.EncodeCommandLineArgument()}");
            normalizerCommandParameters.Add($"-o {outputFile.FullName.EncodeCommandLineArgument()}");
            var startInfo = new ProcessStartInfo
            {
                Arguments = string.Join(" ", normalizerCommandParameters),
                FileName = normalizeCommandFilePath,
                UseShellExecute = false,
            };

            if (_verbose)
                _messageReporter(LogCategory.Information, $"Execute: {startInfo.FileName} {startInfo.Arguments}");
            var process =
                Process.Start(startInfo)
                ?? throw new ApplicationException($"Could not start process. :\"{normalizeCommandFilePath}\"");
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new ApplicationException($"An error occurred in the ffmpeg-normalize command.: exit-code={process.ExitCode}");

            if (_verbose)
                _messageReporter(LogCategory.Information, $"Audio streams normalization is complete.");
        }

        private void SetMetadataOfMovieFile(MovieInformation movieFileInformation, string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile)
        {
            var streams =
                movieFileInformation.VideoStreams
                .Select(stream => new
                {
                    streamTypeSymbol = "v",
                    index = stream.IndexWithinVideoStream,
                    streamDispositions = stream.Disposition,
                    streamTags = stream.Tags,
                })
                .Concat(
                    movieFileInformation.AudioStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "a",
                        index = stream.IndexWithinAudioStream,
                        streamDispositions = stream.Disposition,
                        streamTags = stream.Tags,
                    }))
                .Concat(
                    movieFileInformation.SubtitleStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "s",
                        index = stream.IndexWithinSubtitleStream,
                        streamDispositions = stream.Disposition,
                        streamTags = stream.Tags,
                    }))
                .Concat(
                    movieFileInformation.DataStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "d",
                        index = stream.IndexWithinDataStream,
                        streamDispositions = stream.Disposition,
                        streamTags = stream.Tags,
                    }))
                .Concat(
                    movieFileInformation.AttachmentStreams
                    .Select(stream => new
                    {
                        streamTypeSymbol = "t",
                        index = stream.IndexWithinAttachmentStream,
                        streamDispositions = stream.Disposition,
                        streamTags = stream.Tags,
                    }))
                .Select(stream =>
                {
                    // audio ストリームの encoder および duration タグはオーディオの正規化によって変化するため、復元対象から除外する。
                    var newStreamTags = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
                    foreach (var tagName in stream.streamTags.EnumerateTagNames())
                    {
                        if (!(stream.streamTypeSymbol == "a" && (string.Equals(tagName, "encoder", StringComparison.OrdinalIgnoreCase) || string.Equals(tagName, "duration", StringComparison.OrdinalIgnoreCase))))
                            newStreamTags.Add(tagName, stream.streamTags[tagName]);
                    }

                    return new
                    {
                        stream.streamTypeSymbol,
                        stream.index,
                        stream.streamDispositions,
                        streamTags = newStreamTags,
                    };
                })
                .ToList();

            var metaeditCommandParameters = new List<string>();

            if (_verbose)
                metaeditCommandParameters.Add("-v");
            if (_doOverwrite)
                metaeditCommandParameters.Add("-f");

            // 入力方法の指定
            if (inputFormat is not null)
                metaeditCommandParameters.Add($"-if {inputFormat.EncodeCommandLineArgument()}");
            metaeditCommandParameters.Add($"-i {inputFile.FullName.EncodeCommandLineArgument()}");

            // 0秒間または非常に短いチャプターの削除を抑止する指定
            // これらを指定しないとチャプタータイトルがずれてしまう可能性がある
            metaeditCommandParameters.Add("--keep_empty_chapter");
            metaeditCommandParameters.Add("--minimum_duration 0");

            // 消去対象の設定
            metaeditCommandParameters.Add("-c");
            metaeditCommandParameters.Add("-ccm");

            // チャプタータイトルの指定
            foreach (var chapter in movieFileInformation.Chapters.Select((chapter, index) => (index, title: chapter.Title)))
            {
                if (!string.IsNullOrEmpty(chapter.title))
                    metaeditCommandParameters.Add($"-tt:{chapter.index} {chapter.title.EncodeCommandLineArgument()}");
            }

            // ストリームのメタデータ/dispositionの指定
            var streamMetadataNames = new[] { "title", "language", "encoder" };
            foreach (var stream in streams)
            {
                // ストリームのメタデータの指定 (オーディオの正規化により変化するため、オーディオストリームの "encoder" メタデータは除く)
                foreach (var metadataName in streamMetadataNames)
                {
                    if (stream.streamTags.TryGetValue(metadataName, out var metadataValue) &&
                        !(stream.streamTypeSymbol == "a" && metadataName == "encoder"))
                    {
                        metaeditCommandParameters.Add($"-s:{stream.streamTypeSymbol}:{stream.index} {metadataName.EncodeCommandLineArgument()}={(metadataValue ?? "").EncodeCommandLineArgument()}");
                    }
                }

                // ストリームの disposition の指定
                var dispositionsSpec =
                    stream.streamDispositions.EnumerateDispositions()
                    .Select(disposition => $"{(disposition.dispositionValue ? "+" : "-")}{disposition.dispositionName}");
                metaeditCommandParameters.Add($"-d:{stream.streamTypeSymbol}:{stream.index} {string.Concat(dispositionsSpec).EncodeCommandLineArgument()}");
            }

            // 出力方法の指定
            if (outputFormat is not null)
                metaeditCommandParameters.Add($"-of {outputFormat.EncodeCommandLineArgument()}");
            metaeditCommandParameters.Add($"-o {outputFile.FullName.EncodeCommandLineArgument()}");

            var metaeditCommandParameterText = string.Join(" ", metaeditCommandParameters);

            if (_verbose)
                _messageReporter(LogCategory.Information, $"Execute: {_metaeditCommandFile} {metaeditCommandParameterText}");

            var exitCode =
                Command.ExecuteCommand(
                    _metaeditCommandFile,
                    metaeditCommandParameterText,
                    null,
                    null,
                    null,
                    null,
                    null,
                    (level, message) =>
                    {
                        if (level != LogCategory.Information)
                            _messageReporter(level, message);
                    },
                    null);
            if (exitCode != 0)
                throw new ApplicationException($"An error occurred in the \"{Path.GetFileNameWithoutExtension(_metaeditCommandFile.Name)}\" command. : exit-code={exitCode}");

            if (_verbose)
                _messageReporter(LogCategory.Information, $"\"metaedit\" finished successfully.");
        }

        private void SetMetadataOfMusicFile(MovieInformation musicFileInfo, MovieInformation normalizedMusicFileInfo, string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile, MusicFileMetadata metadata)
        {
            Validation.Assert(_inputFlleProvider is not null);
            Validation.Assert(_outputFlleProvider is not null);
            Validation.Assert(normalizedMusicFileInfo.VideoStreams.None());
            Validation.Assert(normalizedMusicFileInfo.AudioStreams.Take(2).Count() == 1);

            ExecuteFfmpegCommand(musicFileInfo, normalizedMusicFileInfo, _outputFlleProvider, inputFormat, inputFile, outputFormat, outputFile, metadata, _doOverwrite, _verbose, _messageReporter);
            ExecuteKid3ConsoleCommad(musicFileInfo, _outputFlleProvider, metadata, inputFile, outputFile, _disableVideoStream, _allowDuplicateCoverArtPictureType, _messageReporter);

            static void ExecuteFfmpegCommand(MovieInformation musicFileInfo, MovieInformation normalizedMusicFileInfo, IMusicFileMetadataProvider outputFlleProvider, string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile, MusicFileMetadata metadata, bool doOverwrite, bool verbose, Action<LogCategory, string> messageReporter)
            {
                var ffmpegCommandParameters =
                    new List<string>
                    {
                        "-hide_banner",
                    };
                if (doOverwrite)
                    ffmpegCommandParameters.Add("-y");
                if (inputFormat is not null)
                    ffmpegCommandParameters.Add($"-f {inputFormat.EncodeCommandLineArgument()}");
                ffmpegCommandParameters.Add($"-i {inputFile.FullName.EncodeCommandLineArgument()}");

                {
                    var tags =
                        normalizedMusicFileInfo.Format.Tags.EnumerateTagNames()
                        .ToDictionary(tagName => tagName, tagName => string.Equals(tagName, "encoder", StringComparison.OrdinalIgnoreCase) ? normalizedMusicFileInfo.Format.Tags[tagName] ?? "" : "", StringComparer.OrdinalIgnoreCase);
                    foreach (var (metadataName, metadataValue) in outputFlleProvider.EnumerateFormatMetadata(metadata))
                        tags[metadataName] = metadataValue;
                    foreach (var metadataSetElement in tags)
                        ffmpegCommandParameters.Add($"-metadata {metadataSetElement.Key}={metadataSetElement.Value.EncodeCommandLineArgument()}");
                }

                ffmpegCommandParameters.Add("-c copy");
                foreach (var stream in normalizedMusicFileInfo.AudioStreams)
                {
                    ffmpegCommandParameters.Add($"-map 0:a:{stream.IndexWithinAudioStream}");
                    var tags =
                        stream.Tags.EnumerateTagNames()
                        .ToDictionary(tagName => tagName, tagName => string.Equals(tagName, "encoder", StringComparison.OrdinalIgnoreCase) ? stream.Tags[tagName] ?? "" : "", StringComparer.OrdinalIgnoreCase);
                    foreach ((var metadataName, var metadataValue) in outputFlleProvider.EnumerateStreamMetadata(metadata))
                        tags[metadataName] = metadataValue;
                    foreach (var tag in tags)
                        ffmpegCommandParameters.Add($"-metadata:s:a:{stream.IndexWithinAudioStream} {tag.Key}={tag.Value.EncodeCommandLineArgument()}");
                }

                if (outputFormat is not null)
                    ffmpegCommandParameters.Add($"-f {outputFormat.EncodeCommandLineArgument()}");
                ffmpegCommandParameters.Add($"{outputFile.FullName.EncodeCommandLineArgument()}");

                var ffmpegCommandParameterText = string.Join(" ", ffmpegCommandParameters);
                if (verbose)
                    messageReporter(LogCategory.Information, $"Execute: ffmpeg {ffmpegCommandParameterText}");
                ExecuteCommand(_ffmpegCommandFile, ffmpegCommandParameterText, messageReporter);
            }

            static void ExecuteKid3ConsoleCommad(MovieInformation musicFileInfo, IMusicFileMetadataProvider outputFlleProvider, MusicFileMetadata metadata, FilePath inputFile, FilePath outputFile, bool disableVideoStream, bool allowDuplicateCoverArtPictureType, Action<LogCategory, string> messageReporter)
            {
                var coverArtImageFiles = new List<FilePath>();
                try
                {
                    // カバーアート画像を入力ファイルから抽出するために ffmpeg を使用している。
                    // その理由は、入力元ファイルの拡張子がミュージックなどを意味するものと保証されていないことにより kid3-cli を使用することが出来ないため。

                    var ffmpegCommandParameters = new List<string>();
                    var kid3SetCommandParameters = new List<string>();

                    // 出力先ファイルにカバーアート画像をコピーする
                    if (!disableVideoStream)
                    {
                        // コマンドオプションでカバーアート画像のコピーが抑止されていない場合

                        var coverArtStreams = musicFileInfo.VideoStreams.ToList();
                        if (coverArtStreams.Any(stream => stream.Disposition.AttachedPic == false))
                            throw new ApplicationException($"There is a video stream that is not a cover image.: \"{inputFile.FullName}\"");

                        Validation.Assert(coverArtStreams.All(stream => stream.Disposition.AttachedPic == true));

                        var coverArtIndexForKid3 = 0;
                        var coverArtTypes = new List<(string? coverArtTypeName, int coverArtTypeId)>();
                        foreach (var (coverArtStream, coverArtType) in coverArtStreams.Select(coverArtStream => (coverArtStream, coverArtType: GetCoverType(coverArtStream, messageReporter))).OrderBy(item => item.coverArtType.coverArtTypeId == 3 ? -2 : item.coverArtType.coverArtTypeId == 4 ? -1 : item.coverArtType.coverArtTypeId))
                        {
                            // Cover (front), Cover (back), その他, の順に処理する。

                            // カバーアート画像を保存するための一時ファイルのファイル名を決定する
                            var coverArtImageFile =
                                coverArtStream.CodecName == "mjpeg"
                                ? FilePath.CreateTemporaryFile(suffix: ".jpg")
                                : coverArtStream.CodecName == "png"
                                ? FilePath.CreateTemporaryFile(suffix: ".png")
                                : throw new ApplicationException($"Cover image for codec \"{coverArtStreams[0].CodecName}\" is not supported.: \"{inputFile.FullName}\"");
                            coverArtImageFiles.Add(coverArtImageFile);

                            // カバーアート画像を一時ファイルに保存する ffmpeg パラメタを追加する
                            ffmpegCommandParameters.Add($"-c:v copy -update 1 -map 0:v:{coverArtStream.IndexWithinVideoStream} {coverArtImageFile.FullName.EncodeCommandLineArgument()}");

                            coverArtTypes.Add(coverArtType);

                            // 出力先ファイルに適用する kid3-cli のコマンドラインに、カバーアート画像を付加するパラメタを追加する
                            kid3SetCommandParameters.Add($"-c {$"set picture[{coverArtIndexForKid3}]:{coverArtImageFile.FullName.EncodeKid3CommandLineArgument(quoteAlways: true)} \"\"".EncodeCommandLineArgument()}");
                            kid3SetCommandParameters.Add($"-c {$"set picture[{coverArtIndexForKid3}].picturetype {coverArtType.coverArtTypeId}".EncodeCommandLineArgument()}");

                            ++coverArtIndexForKid3;
                        }

                        for (var index1 = 0; index1 < coverArtTypes.Count; ++index1)
                        {
                            var (coverArtTypeName1, coverArtTypeId1) = coverArtTypes[index1];
                            for (var index2 = index1 + 1; index2 < coverArtTypes.Count; ++index2)
                            {
                                var (coverArtTypeName2, coverArtTypeId2) = coverArtTypes[index2];
                                if (coverArtTypeId1 == coverArtTypeId2)
                                {
                                    if (string.Equals(coverArtTypeName1, coverArtTypeName2, StringComparison.OrdinalIgnoreCase))
                                    {
                                        if (allowDuplicateCoverArtPictureType)
                                            messageReporter(LogCategory.Warning, $"Duplicate cover art of type is found. : type=\"{coverArtTypeName1 ?? "<not set>"}\", path=\"{musicFileInfo.Format.File.FullName}\"");
                                        else
                                            throw new ApplicationException($"Duplicate cover art of type is found. : type=\"{coverArtTypeName1 ?? "<not set>"}\", path=\"{musicFileInfo.Format.File.FullName}\"");
                                    }
                                    else
                                    {
                                        if (allowDuplicateCoverArtPictureType)
                                            messageReporter(LogCategory.Warning, $"Duplicate cover art of type is found. : types=\"{coverArtTypeName1 ?? "<not set>"}\" and \"{coverArtTypeName2 ?? "<not set>"}\", path=\"{musicFileInfo.Format.File.FullName}\"");
                                        else
                                            throw new ApplicationException($"Duplicate cover art of type is found. : type=\"{coverArtTypeName1 ?? "<not set>"}\" and \"{coverArtTypeName2 ?? "<not set>"}\", path=\"{musicFileInfo.Format.File.FullName}\"");
                                    }
                                }
                            }
                        }
                    }

                    // 抽出すべきカバーアートがあれば ffmpeg を実行する
                    if (ffmpegCommandParameters.Count > 0)
                        ExecuteCommand(_ffmpegCommandFile, $"-hide_banner -y -nostdin -f {musicFileInfo.Format.FormatName.EncodeCommandLineArgument()} -i {musicFileInfo.Format.File.FullName.EncodeCommandLineArgument()} {string.Join(" ", ffmpegCommandParameters)}", messageReporter, executeSilectly: true);

                    // kid3-cli を使用して出力先ファイルに付加するべきメタデータを収集する
                    foreach (var (metadataName, metadataValue) in outputFlleProvider.EnumerateKid3Metadata(metadata))
                        kid3SetCommandParameters.Add($"-c {$"set {metadataName.EncodeKid3CommandLineArgument()} {metadataValue.EncodeKid3CommandLineArgument()}".EncodeCommandLineArgument()}");

                    if (kid3SetCommandParameters.Count > 0)
                    {
                        // kid3-cli を使用しなければならない場合

                        if (_kid3ConsoleCommandFile.Value is null)
                            throw new ApplicationException("\"kid3-cli\" is not installed correctly.");

                        var kid3CommandParameterText = $"{string.Join(" ", kid3SetCommandParameters)} {outputFile.FullName.EncodeCommandLineArgument()}";
                        try
                        {
                            ExecuteCommand(_kid3ConsoleCommandFile.Value, kid3CommandParameterText, messageReporter);
                        }
                        catch (Exception ex)
                        {

                            throw new ApplicationException($"\"kid-cli\" command terminated abnormally. : parameters=\"{kid3CommandParameterText}\"", ex);
                        }
                    }
                }
                finally
                {
                    foreach (var coverArtImageFile in coverArtImageFiles)
                        coverArtImageFile.SafetyDelete();
                }

                static (string? coverArtTypeName, int coverArtTypeId) GetCoverType(VideoStreamInfo coverArtStream, Action<LogCategory, string> messageReporter)
                {
                    var coverArtTypeName = coverArtStream.Tags.Comment;
                    if (coverArtTypeName is null)
                    {
                        messageReporter(LogCategory.Warning, "Cover art type is not set, \"Other\" is set instead.");
                        return (null, 0);
                    }

                    if (!_coverArtPictureTypes.TryGetValue(coverArtTypeName, out var coverArtTypeId))
                    {
                        messageReporter(LogCategory.Warning, $"Unsupported cover art type is found. It will be changed to \"Other\". : \"{coverArtTypeName}\"");
                        return (coverArtTypeName, 0);
                    }

                    return (coverArtTypeName, coverArtTypeId);
                }
            }

            static void ExecuteCommand(FilePath commandFile, string commandParameterText, Action<LogCategory, string> messageReporter, bool executeSilectly = false)
            {
                var outputEncoding = executeSilectly ? Encoding.UTF8.WithoutPreamble() : null;
                var outputRedirector = executeSilectly ? Command.GetNullOutputRedirector() : null;
                var exitCode =
                    Command.ExecuteCommand(
                        commandFile,
                        commandParameterText,
                        null,
                        outputEncoding,
                        null,
                        outputRedirector,
                        outputRedirector,
                        (level, message) =>
                        {
                            if (level != LogCategory.Information)
                                messageReporter(level, message);
                        },
                        null);
                if (exitCode != 0)
                    throw new ApplicationException($"An error occurred in the \"{commandFile.FullName}\" command. : exit-code={exitCode}");
            }
        }

        private static string? AlternativeFormatForInput(string? format)
        {
            if (format is null)
                return null;
            else if (format == "opus")
                return "ogg";
            else if (format == "adts")
                return "aac";
            else
                return format;
        }
    }
}
