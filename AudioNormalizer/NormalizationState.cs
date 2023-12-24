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

namespace AudioNormalizer
{
    internal class NormalizationState
    {
        private const string _ffmpegPathEnvironmentVariableName = "FFMPEG_PATH";
        private static readonly FilePath _ffmpegCommandFile;
        private static readonly FilePath _metaeditCommandFile;
        private readonly IMusicFileMetadataProvider? _inputFlleProvider;
        private readonly IMusicFileMetadataProvider? _outputFlleProvider;
        private readonly string? _musicFileEncoder;
        private readonly string? _musicFileEncoderOption;
        private readonly bool _doOverwrite;
        private readonly bool _verbose;
        private readonly bool _disableVideoStream;
        private readonly Action<ISequentialInputByteStream, ISequentialOutputByteStream, ulong> _streamCopier;
        private readonly Action<string> _informationMessageReporter;
        private readonly Action<string> _warningMessageReporter;
        private readonly Action<string, string> _warningMessageReporter2;
        private readonly Action<string> _errorMessageReporter;
        private readonly Action<string, string> _errorMessageReporter2;

        static NormalizationState()
        {
            _ffmpegCommandFile = new FilePath(ProcessUtility.WhereIs("ffmpeg") ?? throw new FileNotFoundException($"ffmpeg is not installed."));
            var metaeditCommandName = Path.GetFileNameWithoutExtension(typeof(MovieMetadataEditor.Program).Assembly.Location);
            _metaeditCommandFile = new FilePath(ProcessUtility.WhereIs(metaeditCommandName) ?? throw new FileNotFoundException($"{metaeditCommandName} is not installed."));
        }

        public NormalizationState(
            IMusicFileMetadataProvider? inputFlleProvider,
            IMusicFileMetadataProvider? outputFlleProvider,
            string? musicFileEncoder,
            string? musicFileEncoderOption,
            bool doOverwrite,
            bool verbose,
            bool disableVideoStream,
            Action<ISequentialInputByteStream, ISequentialOutputByteStream, ulong> streamCopier,
            Action<string> informationMessageReporter,
            Action<string> warningMessageReporter,
            Action<string, string> warningMessageReporter2,
            Action<string> errorMessageReporter,
            Action<string, string> errorMessageReporter2)
        {
            _inputFlleProvider = inputFlleProvider;
            _outputFlleProvider = outputFlleProvider;
            _doOverwrite = doOverwrite;
            _verbose = verbose;
            _disableVideoStream = disableVideoStream;
            _streamCopier = streamCopier;
            _informationMessageReporter = informationMessageReporter;
            _warningMessageReporter = warningMessageReporter;
            _warningMessageReporter2 = warningMessageReporter2;
            _errorMessageReporter = errorMessageReporter;
            _errorMessageReporter2 = errorMessageReporter2;
            _musicFileEncoder = musicFileEncoder;
            _musicFileEncoderOption = musicFileEncoderOption;
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

                NormalizeMusicFile(movieInformation, inputFormat, inputFile, outputFormat, outputFile);
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

            _informationMessageReporter("Since the video file does not contain an audio track, simply copy it.");
            if (!string.IsNullOrEmpty(_musicFileEncoder))
                _warningMessageReporter("The \"--encoder\" option is ignored for files that do not have an audio stream.");
            if (!string.IsNullOrEmpty(_musicFileEncoderOption))
                _warningMessageReporter("The \"--encoder_option\" option is ignored for files that do not have an audio stream.");

            if (_verbose)
            {
                if (inputFormat is not null)
                    _informationMessageReporter($"  Input file format: {inputFormat}");
                _informationMessageReporter($"  Input file format: \"{inputFile.FullName}\"");
                if (outputFormat is not null)
                    _informationMessageReporter($"  Output file format: {outputFormat}");
                _informationMessageReporter($"  Output file format: \"{outputFile.FullName}\"");
            }

            using var instream = inputFile.OpenRead();
            using var outstream = outputFile.Create();
            _streamCopier(instream, outstream, inputFile.Length);

            if (_verbose)
                _informationMessageReporter("Copy finished.");
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

            if (!string.IsNullOrEmpty(_musicFileEncoder))
                _warningMessageReporter("The \"--encoder\" option is ignored for movie file normalization.");
            if (!string.IsNullOrEmpty(_musicFileEncoderOption))
                _warningMessageReporter("The \"--encoder_option\" option is ignored for movie file normalization.");
            if (_disableVideoStream)
                _warningMessageReporter("The \"--video_disable\" option is ignored for movie file normalization.");

            // 中間一時ファイルを作成する
            var temporaryIntermediateFile1 = new FilePath(Path.GetTempFileName());
            var temporaryIntermediateFileFormat1 = outputFormat ?? "matroska";
            var temporaryIntermediateFile2 = new FilePath(Path.GetTempFileName());
            var temporaryIntermediateFileFormat2 = outputFormat ?? "matroska";
            if (_verbose)
            {
                _informationMessageReporter($"Temprary file is created.: \"{temporaryIntermediateFile2.FullName}\"");
            }

            try
            {
                // 正規化の前に、動画ファイルのオーディオストリームの encoder タグを消去する
                // ※正規化の際にオーディオストリームの encoder タグが上書きされないので、最終的に encoder タグの値が不適切な値になってしまうのを防ぐため。
                if (_verbose)
                {
                    _informationMessageReporter("Remove audio stream tags.");
                    if (inputFormat is not null)
                        _informationMessageReporter($"  Input file format: {inputFormat}");
                    _informationMessageReporter($"  Input file format: \"{inputFile.FullName}\"");
                    if (temporaryIntermediateFileFormat2 is not null)
                        _informationMessageReporter($"  Output file format: {temporaryIntermediateFileFormat1}");
                    _informationMessageReporter($"  Output file format: \"{temporaryIntermediateFile1.FullName}\"");
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
                    _informationMessageReporter("Start audio normalization.");
                    if (temporaryIntermediateFileFormat1 is not null)
                        _informationMessageReporter($"  Input file format: {temporaryIntermediateFileFormat1}");
                    _informationMessageReporter($"  Input file format: \"{temporaryIntermediateFile1.FullName}\"");
                    if (temporaryIntermediateFileFormat2 is not null)
                        _informationMessageReporter($"  Output file format: {temporaryIntermediateFileFormat2}");
                    _informationMessageReporter($"  Output file format: \"{temporaryIntermediateFile2.FullName}\"");
                }

                NormalizeAudioStreamOfMovieFile(
                    movieInformation,
                    temporaryIntermediateFileFormat1,
                    temporaryIntermediateFile1,
                    temporaryIntermediateFileFormat2,
                    temporaryIntermediateFile2);

                if (_verbose)
                    _informationMessageReporter("Audio normalization finished.");

                // 動画ファイルの余分なメタデータを消去する
                if (_verbose)
                {
                    _informationMessageReporter("Start restoring metadata.");
                    if (temporaryIntermediateFileFormat2 is not null)
                        _informationMessageReporter($"  Input file format: {temporaryIntermediateFileFormat2}");
                    _informationMessageReporter($"  Input file format: \"{temporaryIntermediateFile2.FullName}\"");
                    if (outputFormat is not null)
                        _informationMessageReporter($"  Output file format: {outputFormat}");
                    _informationMessageReporter($"  Output file format: \"{outputFile.FullName}\"");
                }

                SetMetadataOfMovieFile(
                    movieInformation,
                    temporaryIntermediateFileFormat2,
                    temporaryIntermediateFile2,
                    outputFormat,
                    outputFile);

                if (_verbose)
                    _informationMessageReporter("Audio normalization finished.");
            }
            finally
            {
                foreach (var temporaryIntermediateFile in new[] { temporaryIntermediateFile1, temporaryIntermediateFile2 })
                {
                    if (temporaryIntermediateFile is not null)
                    {
                        try
                        {
                            File.Delete(temporaryIntermediateFile.FullName);
                            if (_verbose)
                                _informationMessageReporter($"Temporary file is deleted.: \"{temporaryIntermediateFile.FullName}\"");
                        }
                        catch (Exception)
                        {
                        }
                    }
                }
            }
        }

        private void NormalizeMusicFile(MovieInformation musicFileInfo, string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile)
        {
            // 入力ファイルが音楽ファイルであると判断し、オーディオストリームの正規化と共に以下の処理を行う。
            // - オーディオストリームのコーデックを出力先フォーマットに応じて変更する
            // - 以下の情報を維持する維持する。
            //   * チャプターのメタデータ (title)
            //   * ストリームのメタデータ (language/title)
            //   * ストリームの disposition (default/force/title/attached_pic)
            // - 上記以外のメタデータを消去する。

            if (_inputFlleProvider is null)
                throw new Exception("Input file format is unknown.");

            if (_outputFlleProvider is null)
                throw new Exception("Output file format is unknown.");

            // 中間一時ファイルを作成する
            var temporaryIntermediateFile = new FilePath(Path.GetTempFileName());
            var temporaryIntermediateFileFormat = outputFormat ?? _outputFlleProvider.GuessFileFormat();
            if (_verbose)
            {
                _informationMessageReporter($"Temprary file is created.: \"{temporaryIntermediateFile.FullName}\"");
            }

            try
            {
                var metadata = _inputFlleProvider.GetMetadata(musicFileInfo);

                // 音楽ファイルを正規化して中間一時ファイルに保存する
                if (_verbose)
                {
                    _informationMessageReporter("Start audio normalization.");
                    if (inputFormat is not null)
                        _informationMessageReporter($"  Input file format: {inputFormat}");
                    _informationMessageReporter($"  Input file format: \"{inputFile.FullName}\"");
                    if (temporaryIntermediateFileFormat is not null)
                        _informationMessageReporter($"  Output file format: {temporaryIntermediateFileFormat}");
                    _informationMessageReporter($"  Output file format: \"{temporaryIntermediateFile.FullName}\"");
                }

                NormalizeAudioStreamOfMusicFile(
                    musicFileInfo,
                    inputFormat,
                    inputFile,
                    temporaryIntermediateFileFormat,
                    temporaryIntermediateFile);

                if (_verbose)
                    _informationMessageReporter("Audio normalization finished.");

                // 音楽ファイルのメタデータを復元する
                if (_verbose)
                {
                    _informationMessageReporter("Start restoring metadata.");
                    if (temporaryIntermediateFileFormat is not null)
                        _informationMessageReporter($"  Input file format: {AlternativeFormatForInput(temporaryIntermediateFileFormat)}");
                    _informationMessageReporter($"  Input file format: \"{temporaryIntermediateFile.FullName}\"");
                    if (outputFormat is not null)
                        _informationMessageReporter($"  Output file format: {outputFormat}");
                    _informationMessageReporter($"  Output file format: \"{outputFile.FullName}\"");
                }

                var normalizedMusicFileInfo = GetMovieInformation(AlternativeFormatForInput(temporaryIntermediateFileFormat), temporaryIntermediateFile);

                SetMetadataOfMusicFile(
                    musicFileInfo,
                    normalizedMusicFileInfo,
                    AlternativeFormatForInput(temporaryIntermediateFileFormat),
                    temporaryIntermediateFile,
                    outputFormat,
                    outputFile,
                    metadata);

                if (_verbose)
                    _informationMessageReporter("Audio normalization finished.");
            }
            finally
            {
                if (temporaryIntermediateFile is not null)
                {
                    try
                    {
                        File.Delete(temporaryIntermediateFile.FullName);
                        if (_verbose)
                            _informationMessageReporter($"Temporary file is deleted.: \"{temporaryIntermediateFile.FullName}\"");
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private MovieInformation GetMovieInformation(string? inputFormat, FilePath inputFile)
        {
            if (_verbose)
                _informationMessageReporter($"Probe movie information.: \"{inputFile.FullName}\"");
            try
            {
                return
                    Command.GetMovieInformation(
                        inputFormat,
                        inputFile,
                        MovieInformationType.Chapters | MovieInformationType.Streams | MovieInformationType.Format,
                        (level, message) =>
                        {
                            switch (level)
                            {
                                case "WARNING":
                                    _warningMessageReporter2("ffprobe", message);
                                    break;
                                case "ERROR":
                                    _errorMessageReporter2("ffprobe", message);
                                    break;
                                default:
                                    break;
                            }
                        });
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to get movie information.", ex);
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
                metaeditCommandParameters.Add($"-if {inputFormat.CommandLineArgumentEncode()}");
            metaeditCommandParameters.Add($"-i {inputFile.FullName.CommandLineArgumentEncode()}");

            // ストリームのメタデータの指定
            foreach (var stream in movieFileInformation.AudioStreams)
            {
                // ストリームのメタデータの指定 (オーディオの正規化により変化するため、オーディオストリームの "encoder" メタデータは除く)
                foreach (var metadataName in new[] { "encoder", "duration" })
                {
                    var metadataValue = stream.Tags[metadataName];
                    if (metadataValue is not null)
                        metaeditCommandParameters.Add($"-s:a:{stream.IndexWithinAudioStream} {metadataName.CommandLineArgumentEncode()}=");
                }
            }

            // 出力方法の指定
            if (outputFormat is not null)
                metaeditCommandParameters.Add($"-of {outputFormat.CommandLineArgumentEncode()}");
            metaeditCommandParameters.Add($"-o {outputFile.FullName.CommandLineArgumentEncode()}");

            var metaeditCommandParameterText = string.Join(" ", metaeditCommandParameters);
            var exitCode =
                Command.ExecuteCommand(
                    _metaeditCommandFile,
                    metaeditCommandParameterText,
                    Encoding.UTF8,
                    null,
                    null,
                    null,
                    (level, message) =>
                    {
                        switch (level)
                        {
                            case "WARNING":
                                _warningMessageReporter(message);
                                break;
                            case "ERROR":
                                _errorMessageReporter(message);
                                break;
                            default:
                                // NOP
                                break;
                        }
                    },
                    null);
            if (exitCode != 0)
                throw new Exception($"An error occurred in the \"{Path.GetFileNameWithoutExtension(_metaeditCommandFile.Name)}\" command. : exit-code={exitCode}");
        }

        private void NormalizeAudioStreamOfMovieFile(MovieInformation movieFileInformation, string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile)
        {
            // チャンネルレイアウトが libopus によってサポートされていないオーディオストリームを抽出する
            var audioStreamsNotSupportedByLibOpus =
                movieFileInformation.AudioStreams
                .Where(stream => stream.ChannelLayout is not null && stream.ChannelLayout.IsAnyOf("5.0(side)", "5.1(side)"))
                .ToList();

            if (audioStreamsNotSupportedByLibOpus.Count > 0)
            {
                // チャンネルレイアウトが libopus によってサポートされていないオーディオストリームが存在する場合

                if (_verbose)
                    _informationMessageReporter($"Normalize the audio stream and encode it with \"libvorbis\". Because some audio stream channel layouts are not supported by \"libopus\".: {string.Join(", ", audioStreamsNotSupportedByLibOpus.Select(stream => $"a:{stream.IndexWithinAudioStream}(\"{stream.ChannelLayout}\")"))}");

                var audioEncoder = "libvorbis";

                // libvorbis のエンコーダオプションを作成する
                var provider = new OggMusicFileMetadataProvider(TransferDirection.Output, "ogg", ".ogg");
                var audioEncoderOptions =
                        movieFileInformation.AudioStreams
                        .SelectMany(stream => provider.GuessDefaultEncoderSpec(stream).encoderOptions)
                        .ToList();

                ExecuteFfpegNormaize(inputFormat, inputFile, outputFormat, outputFile, audioEncoder, audioEncoderOptions, false);
            }
            else
            {
                // すべてのオーディオストリームのチャンネルレイアウトが libopus によってサポートされている場合

                if (_verbose)
                    _informationMessageReporter("Normalize the audio stream and encode it by \"libopus\".");

                var audioEncoder = "libopus";
                // libopus のエンコーダオプションを作成する
                var provider = new OggMusicFileMetadataProvider(TransferDirection.Output, "opus", ".opus");
                var audioEncoderOptions =
                        movieFileInformation.AudioStreams
                        .SelectMany(stream => provider.GuessDefaultEncoderSpec(stream).encoderOptions)
                        .ToList();

                ExecuteFfpegNormaize(inputFormat, inputFile, outputFormat, outputFile, audioEncoder, audioEncoderOptions, false);
            }
        }

        private void NormalizeAudioStreamOfMusicFile(MovieInformation musicFileInformation, string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile)
        {
            Validation.Assert(_inputFlleProvider is not null, "_inputFlleProvider is not null");
            Validation.Assert(_outputFlleProvider is not null, "_outputFlleProvider is not null");
            Validation.Assert(musicFileInformation.AudioStreams.Take(2).Count() == 1, "musicFileInformation.AudioStreams.Take(2).Count() == 1");

            var sourceAudioStream = musicFileInformation.AudioStreams.First();
            var defaultEncoderSpec = _outputFlleProvider.GuessDefaultEncoderSpec(sourceAudioStream);
            var encoder = _musicFileEncoder ?? defaultEncoderSpec.encoder;
            var encoderOptions = _musicFileEncoderOption is not null ? new[] { _musicFileEncoderOption } : defaultEncoderSpec.encoderOptions;
            ExecuteFfpegNormaize(inputFormat, inputFile, outputFormat, outputFile, encoder, encoderOptions, true);
        }

        private void ExecuteFfpegNormaize(string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile, string audioEncoder, IEnumerable<string> audioEncoderOptions, bool exceptVideoStream)
        {
            var ffmpegCommandFilePath =
                ProcessUtility.WhereIs("ffmpeg")
                ?? throw new Exception("ffmpeg command not found.");
            Environment.SetEnvironmentVariable(_ffmpegPathEnvironmentVariableName, ffmpegCommandFilePath);
            var normalizeCommandFilePath =
                ProcessUtility.WhereIs("ffmpeg-normalize")
                ?? throw new Exception("ffmpeg-normalize command not found.");
            var normalizerCommandParameters = new List<string>();
            if (_verbose)
                normalizerCommandParameters.Add("-v");
            normalizerCommandParameters.Add("-f");
            normalizerCommandParameters.Add("-pr");
            normalizerCommandParameters.Add("--keep-loudness-range-target");
            if (inputFormat is not null)
                normalizerCommandParameters.Add($"-ei={string.Join(" ", new[] { "-f", $"{inputFormat.CommandLineArgumentEncode()}" }).CommandLineArgumentEncode()}");
            normalizerCommandParameters.Add(inputFile.FullName.CommandLineArgumentEncode());
            if (exceptVideoStream)
                normalizerCommandParameters.Add("-vn");
            normalizerCommandParameters.Add($"-c:a {audioEncoder.CommandLineArgumentEncode()}");
            if (audioEncoderOptions.Any())
                normalizerCommandParameters.Add($"-e={string.Join(" ", audioEncoderOptions).CommandLineArgumentEncode()}");
            if (outputFormat is not null)
                normalizerCommandParameters.Add($"-ofmt {outputFormat.CommandLineArgumentEncode()}");
            normalizerCommandParameters.Add($"-o {outputFile.FullName.CommandLineArgumentEncode()}");
            var startInfo = new ProcessStartInfo
            {
                Arguments = string.Join(" ", normalizerCommandParameters),
                FileName = normalizeCommandFilePath,
                UseShellExecute = false,
            };

            if (_verbose)
                _informationMessageReporter($"Execute: {startInfo.FileName} {startInfo.Arguments}");
            var process =
                Process.Start(startInfo)
                ?? throw new Exception($"Could not start process. :\"{normalizeCommandFilePath}\"");
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"An error occurred in the ffmpeg-normalize command.: exit-code={process.ExitCode}");

            if (_verbose)
                _informationMessageReporter($"Audio streams normalization is complete.");
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
                metaeditCommandParameters.Add($"-if {inputFormat.CommandLineArgumentEncode()}");
            metaeditCommandParameters.Add($"-i {inputFile.FullName.CommandLineArgumentEncode()}");

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
                    metaeditCommandParameters.Add($"-tt:{chapter.index} {chapter.title.CommandLineArgumentEncode()}");
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
                        metaeditCommandParameters.Add($"-s:{stream.streamTypeSymbol}:{stream.index} {metadataName.CommandLineArgumentEncode()}={(metadataValue ?? "").CommandLineArgumentEncode()}");
                    }
                }

                // ストリームの disposition の指定
                var dispositionsSpec =
                    stream.streamDispositions.EnumerateDispositions()
                    .Select(disposition => $"{(disposition.dispositionValue ? "+" : "-")}{disposition.dispositionName}");
                metaeditCommandParameters.Add($"-d:{stream.streamTypeSymbol}:{stream.index} {string.Concat(dispositionsSpec).CommandLineArgumentEncode()}");
            }

            // 出力方法の指定
            if (outputFormat is not null)
                metaeditCommandParameters.Add($"-of {outputFormat.CommandLineArgumentEncode()}");
            metaeditCommandParameters.Add($"-o {outputFile.FullName.CommandLineArgumentEncode()}");

            var metaeditCommandParameterText = string.Join(" ", metaeditCommandParameters);
            var exitCode =
                Command.ExecuteCommand(
                    _metaeditCommandFile,
                    metaeditCommandParameterText,
                    Encoding.UTF8,
                    null,
                    null,
                    null,
                    (level, message) =>
                    {
                        switch (level)
                        {
                            case "WARNING":
                                _warningMessageReporter(message);
                                break;
                            case "ERROR":
                                _errorMessageReporter(message);
                                break;
                            default:
                                // NOP
                                break;
                        }
                    },
                    null);
            if (exitCode != 0)
                throw new Exception($"An error occurred in the \"{Path.GetFileNameWithoutExtension(_metaeditCommandFile.Name)}\" command. : exit-code={exitCode}");
        }

        private void SetMetadataOfMusicFile(MovieInformation musicFileInfo, MovieInformation normalizedMusicFileInfo, string? inputFormat, FilePath inputFile, string? outputFormat, FilePath outputFile, MusicFileMetadata metadata)
        {
            Validation.Assert(_inputFlleProvider is not null, "_inputFlleProvider is not null");
            Validation.Assert(_outputFlleProvider is not null, "_outputFlleProvider is not null");
            Validation.Assert(normalizedMusicFileInfo.VideoStreams.None(), "normalizedMusicFileInfo.VideoStreams.None()");
            Validation.Assert(normalizedMusicFileInfo.AudioStreams.Take(2).Count() == 1, "musicFileInformation.AudioStreams.Take(2).Count() == 1");

            var metadataFilePath = (string?)null;
            try
            {
                var metadataText = _outputFlleProvider.FormatMetadataFile(metadata);

                if (metadataText is not null)
                {
                    metadataFilePath = Path.GetTempFileName();
                    if (_verbose)
                        _informationMessageReporter($"Temprary file is created.: \"{metadataFilePath}\"");
                    File.WriteAllText(metadataFilePath, metadataText, new UTF8Encoding(false));
                }

                var ffmpegCommandParameters =
                    new List<string>
                    {
                        "-hide_banner",
                    };
                if (_doOverwrite)
                    ffmpegCommandParameters.Add("-y");
                ffmpegCommandParameters.Add($"-f {musicFileInfo.Format.FormatName.CommandLineArgumentEncode()}");
                ffmpegCommandParameters.Add($"-i {musicFileInfo.Format.File.FullName.CommandLineArgumentEncode()}");
                if (inputFormat is not null)
                    ffmpegCommandParameters.Add($"-f {inputFormat.CommandLineArgumentEncode()}");
                ffmpegCommandParameters.Add($"-i {inputFile.FullName.CommandLineArgumentEncode()}");
                if (metadataFilePath is not null)
                {
                    ffmpegCommandParameters.Add($"-f ffmetadata -i {metadataFilePath}");
                    ffmpegCommandParameters.Add("-map_metadata 2");
                }
                else
                {
                    ffmpegCommandParameters.Add("-map_metadata -1");
                }

                ffmpegCommandParameters.Add("-c copy");
                foreach (var stream in normalizedMusicFileInfo.AudioStreams)
                {
                    ffmpegCommandParameters.Add($"-map 1:a:{stream.IndexWithinAudioStream}");
                    var tags =
                        stream.Tags.EnumerateTagNames()
                        .ToDictionary(tagName => tagName, tagName => string.Equals(tagName, "encoder", StringComparison.OrdinalIgnoreCase) ? stream.Tags[tagName] ?? "" : "");
                    foreach ((var metadataName, var metadataValue) in _outputFlleProvider.GetStreamMetadata(metadata))
                        tags[metadataName] = metadataValue;
                    foreach (var tag in tags)
                        ffmpegCommandParameters.Add($"-metadata:s:a:{stream.IndexWithinAudioStream} {tag.Key}={tag.Value.CommandLineArgumentEncode()}");
                }

                if (!_disableVideoStream)
                {
                    foreach (var stream in musicFileInfo.VideoStreams)
                    {
                        ffmpegCommandParameters.Add($"-map 0:v:{stream.IndexWithinVideoStream}");
                        ffmpegCommandParameters.Add($"-disposition:v:{stream.IndexWithinVideoStream} +attached_pic");
                        var tags =
                            stream.Tags.EnumerateTagNames()
                            .ToDictionary(tagName => tagName, tagName => string.Equals(tagName, "encoder", StringComparison.OrdinalIgnoreCase) ? stream.Tags[tagName] ?? "" : "");
                        tags["comment"] = "Cover (front)";
                        foreach (var tag in tags)
                            ffmpegCommandParameters.Add($"-metadata:s:v:{stream.IndexWithinVideoStream} {tag.Key}={tag.Value.CommandLineArgumentEncode()}");
                    }
                }

                if (outputFormat is not null)
                    ffmpegCommandParameters.Add($"-f {outputFormat.CommandLineArgumentEncode()}");
                ffmpegCommandParameters.Add($"{outputFile.FullName.CommandLineArgumentEncode()}");

                var ffmpegCommandParameterText = string.Join(" ", ffmpegCommandParameters);
                if (_verbose)
                    _informationMessageReporter($"Execute: ffmpeg {ffmpegCommandParameterText}");
                var exitCode =
                    Command.ExecuteCommand(
                        _ffmpegCommandFile,
                        ffmpegCommandParameterText,
                        Encoding.UTF8,
                        null,
                        null,
                        null,
                        (level, message) =>
                        {
                            switch (level)
                            {
                                case "WARNING":
                                    _warningMessageReporter(message);
                                    break;
                                case "ERROR":
                                    _errorMessageReporter(message);
                                    break;
                                default:
                                    // NOP
                                    break;
                            }
                        },
                        null);
                if (exitCode != 0)
                    throw new Exception($"An error occurred in the \"ffmpeg\" command. : exit-code={exitCode}");
            }
            finally
            {
                if (metadataFilePath is not null && File.Exists(metadataFilePath))
                {
                    try
                    {
                        File.Delete(metadataFilePath);
                        if (_verbose)
                            _informationMessageReporter($"Temporary file is deleted.: \"{metadataFilePath}\"");
                    }
                    catch (Exception)
                    {
                    }
                }
            }
        }

        private static string? AlternativeFormatForInput(string? format)
        {
            if (format is null)
                return null;
            else if (format == "opus")
                return "ogg";
            else
                return format;
        }
    }
}
