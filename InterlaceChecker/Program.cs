using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Interprocess;
using Palmtree;
using Palmtree.IO;

namespace InterlaceChecker
{
    public static partial class Program
    {
        private class CommandParameter
        {
            private CommandParameter(IEnumerable<CommandOption<OptionType>> options)
            {
                InputFormat = options.SingleOrNone(option => option.OptionType == OptionType.InputFormat)?.OptionParameter[1] as string;
                Input = options.SingleOrNone(option => option.OptionType == OptionType.Input)?.OptionParameter[1] as string;
                (From, To) = GetTrimmingRange(
                    options.SingleOrNone(option => option.OptionType == OptionType.FromForTrimming)?.OptionParameter[1] as TimeSpan?,
                    options.SingleOrNone(option => option.OptionType == OptionType.ToForTrimming)?.OptionParameter[1] as TimeSpan?,
                    options.SingleOrNone(option => option.OptionType == OptionType.DurationForTrimming)?.OptionParameter[1] as TimeSpan?);
                IsHelpMode = options.Any(option => option.OptionType == OptionType.Help);
            }

            public string? InputFormat { get; }
            public string? Input { get; }
            public TimeSpan From { get; }
            public TimeSpan To { get; }
            public bool IsHelpMode { get; }

            public static CommandParameter Parse(IEnumerable<CommandOptionDefinition<OptionType>> optionDefinitions, string[] args)
                => new(optionDefinitions.ParseCommandArguments(args.AsReadOnlyArray()));

            private static (TimeSpan from, TimeSpan to) GetTrimmingRange(TimeSpan? ssValue, TimeSpan? toValue, TimeSpan? tValue)
            {
                Validation.Assert(toValue is null || tValue is null, "toValue is null || tValue is null");
                Validation.Assert(ssValue is null || ssValue.Value >= TimeSpan.Zero, "ssValue is null || ssValue.Value >= TimeSpan.Zero");
                Validation.Assert(toValue is null || toValue.Value >= TimeSpan.Zero, "toValue is null || toValue.Value >= TimeSpan.Zero");
                Validation.Assert(tValue is null || tValue.Value >= TimeSpan.Zero, "tValue is null || tValue.Value >= TimeSpan.Zero");

                var from = ssValue ?? TimeSpan.Zero;
                if (toValue is not null)
                {
                    return
                        from <= toValue.Value
                        ? (from, toValue.Value)
                        : throw new InvalidCommandOptionException($"The time specified with the \"-to\" option is less than the time specified with the \"-ss\" option.\r\nCheck the \"-ss\" option value and \"-to\" option value.: -ss {from.FormatTime(TimeFormatType.LongFormat, 3)} -to {toValue.Value.FormatTime(TimeFormatType.LongFormat, 3)}");
                }
                else
                {
                    return
                        (from,
                            tValue is null
                            ? TimeSpan.MaxValue
                            : from <= TimeSpan.MaxValue - tValue.Value
                            ? from + tValue.Value
                            : TimeSpan.MaxValue);
                }
            }
        }

        private static readonly string _thisProgramName;
        private static readonly FileInfo _ffmpegCommandFile;
        private static readonly Regex _ignoredMessagePattern;
        private static readonly Regex _progressMessagePattern;
        private static readonly Regex _resultPattern;
        private static readonly IEnumerable<CommandOptionDefinition<OptionType>> _optionDefinitions;

        static Program()
        {
            _thisProgramName = Path.GetFileNameWithoutExtension(typeof(Program).Assembly.Location);
            _ignoredMessagePattern = new Regex(@"^\[null *@", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            _progressMessagePattern = new Regex(@"^frame=", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            _resultPattern = new Regex(@"^\[Parsed_idet_\d+ *@ *[\da-f]+\] *Multi frame detection *: *TFF *: *(?<tff>\d+) +BFF *: *(?<bff>\d+) +Progressive *: *(?<progressive>\d+) +Undetermined *: *(?<undetermined>\d+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            _ffmpegCommandFile = new FileInfo(ProcessUtility.WhereIs("ffmpeg") ?? throw new FileNotFoundException("ffmpeg command is not installed."));
            _optionDefinitions = GetOptionDefinitions();
        }

        public static int Main(string[] args)
        {
            // このプロセスでは Ctrl+C を無視する。
            TinyConsole.CancelKeyPress += (sender, e) => e.Cancel = true;

            try
            {
                var commandOptions = ParseCommandOptions(args);
                if (commandOptions is null)
                    return 1;

                if (commandOptions.Input is not null)
                {
                    try
                    {
                        if (!File.Exists(commandOptions.Input))
                        {
                            PrintErrorMessage($"The input file specified by the \"--input\" option does not exist.: \"{commandOptions.Input}\"");
                            return 1;
                        }
                    }
                    catch (Exception ex)
                    {
                        PrintErrorMessage($"Failed to access the input file specified by the \"--input\" option.: \"{commandOptions.Input}\"");
                        PrintExceptionMessage(ex);
                        return 1;
                    }
                }

                if (commandOptions.IsHelpMode)
                {
                    PrintHelpMessage();
                    return 0;
                }

                try
                {
                    // 入力の準備をする (入力元が標準入力である場合は一時ファイルにコピーする)
                    var (inputFormat, inputFile, temporaryInputFile) = GetInputMovieFile(commandOptions);

                    try
                    {
                        //\Amazon>ffmpeg -hide_banner -i "I:\VIDEO\Blu-ray Ripper\COWBOY BEBOP 劇場版\MakeMKV.BONUS-5.mkv" -vf idet -an -sn -f null -
                        var ffmpegCommandParameters =
                            new List<string>
                            {
                                "-hide_banner"
                            };
                        if (inputFormat is not null)
                            ffmpegCommandParameters.Add($"-f {inputFormat.CommandLineArgumentEncode()}");
                        ffmpegCommandParameters.Add($"-i {inputFile.FullName.CommandLineArgumentEncode()}");
                        ffmpegCommandParameters.Add("-vf idet");
                        ffmpegCommandParameters.Add("-an -sn");
                        ffmpegCommandParameters.Add("-f null -");
                        var ffmpegCommandLineText = string.Join(" ", ffmpegCommandParameters);

                        var isProgressive = (bool?)null;
                        var exitCode =
                            Command.ExecuteCommand(
                                _ffmpegCommandFile,
                                ffmpegCommandLineText,
                                Encoding.UTF8,
                                null,
                                null,
                                Command.GetTextOutputRedirector(
                                    text =>
                                    {
                                        if (_ignoredMessagePattern.IsMatch(text))
                                        {
                                        }
                                        else if (_progressMessagePattern.IsMatch(text))
                                        {
                                            TinyConsole.Error.Write($"{text}\r");
                                        }
                                        else
                                        {
                                            TinyConsole.Error.WriteLine(text);
                                            var match = _resultPattern.Match(text);
                                            if (match.Success)
                                            {
                                                var tff = match.Groups["tff"].Value.ParseAsUint64();
                                                var bff = match.Groups["bff"].Value.ParseAsUint64();
                                                var progressive = match.Groups["progressive"].Value.ParseAsUint64();
                                                checked
                                                {
                                                    var ff = tff + bff;
                                                    isProgressive = ff * 5 < (ff + progressive);
                                                }
                                            }
                                        }
                                    }),
                                (level, message) =>
                                {
                                    switch (level)
                                    {
                                        case "WARNING":
                                            PrintWarningMessage(message);
                                            break;
                                        case "ERROR":
                                            PrintErrorMessage(message);
                                            break;
                                        default:
                                            // NOP
                                            break;
                                    }
                                },
                                null);
                        if (exitCode != 0)
                            throw new Exception($"An error occurred in the \"ffmpeg\" command. : exit-code={exitCode}");

                        TinyConsole.Out.Write($"{(commandOptions.Input is null ? "-" : Path.GetFullPath(commandOptions.Input))}\t");
                        TinyConsole.Out.WriteLine(
                            isProgressive is null
                            ? "Unclear"
                            : isProgressive == true
                            ? "Progressive"
                            : "Interlace");

                        return 0;
                    }
                    finally
                    {
                        if (temporaryInputFile is not null)
                        {
                            try
                            {
                                File.Delete(temporaryInputFile.FullName);
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    PrintExceptionMessage(ex);
                    return 1;
                }
            }
            finally
            {
                TinyConsole.ResetColor();
            }
        }

        private static CommandParameter? ParseCommandOptions(string[] args)
        {
            try
            {
                return CommandParameter.Parse(_optionDefinitions, args);
            }
            catch (InvalidCommandOptionException ex)
            {
                for (var e = ex as Exception; e is not null; e = e.InnerException)
                    PrintErrorMessage(e.Message);
                return null;
            }
        }

        private static (string? inputFormat, FileInfo inputFile, FileInfo? inputTemporaryFile) GetInputMovieFile(CommandParameter commandParameters)
        {

            if (commandParameters.Input is not null)
            {
                return (commandParameters.InputFormat, new FileInfo(commandParameters.Input), null);
            }
            else
            {
                var temporaryInputFile = new FileInfo(Path.GetTempFileName());

                try
                {
                    using var inputStream = TinyConsole.OpenStandardInput();
                    using var outputStream = new FileStream(temporaryInputFile.FullName, FileMode.Create, FileAccess.Write, FileShare.None);
                    CopyStream(inputStream, outputStream);
                    return (commandParameters.InputFormat, temporaryInputFile, temporaryInputFile);
                }
                catch (Exception ex)
                {
                    try
                    {
                        File.Delete(temporaryInputFile.FullName);
                    }
                    catch (Exception)
                    {
                    }

                    throw new Exception("An error occurred while preparing the input file.", ex);
                }
            }
        }

        private static void CopyStream(Stream instream, Stream outstream)
        {
            try
            {
                var state = 0;
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.Invisible;
                instream.CopyTo(
                    outstream,
                    new Progress<long>(CopiedLength =>
                    {
                        var stateSymbol =
                            state switch
                            {
                                1 => "＼",
                                2 => "│",
                                3 => "／",
                                _ => "─",
                            };
                        TinyConsole.Error.Write($"Copying... {stateSymbol}");
                        state = (state + 1) % 4;
                        try
                        {
                            TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                        }
                        catch (Exception)
                        {
                        }

                        TinyConsole.Write("\r");
                    }));
            }
            finally
            {
                TinyConsole.ResetColor();
                TinyConsole.CursorVisible = ConsoleCursorVisiblity.NormalMode;
                TinyConsole.Error.Write("\r");
                try
                {
                    TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
                }
                catch (Exception)
                {
                }
            }
        }

        private static void PrintHelpMessage()
        {
            var textLines =
                new[]
                {
#if false // この部分はREADME などで書く
                    $"[About {_thisCommandName}]",
                    $"  The {_thisCommandName} command edits the chapters of the input movie file and outputs them as a new movie file.",
                    $"  You can specify a file path name for the input/output of a movie file, or you can use the standard input/output.",
                    "",
                    "[What can be edited with chapedit]",
                    "  * You can replace the chapter times with the specified times.",
                    "  * You can change/delete the title of the specified chapter.",
                    "",
#endif
                    "[Usage]",
                    $"{_thisProgramName} <option list>",
                    $"  For \"<option list>\", specify the necessary options from the list below. You can arrange them in any order.",
                    "",
                    "[Options]",
                }
                .Concat(
                    _optionDefinitions
                    .OrderBy(optionDefinition => optionDefinition.OptionType)
                    .SelectMany(optionDefinition =>
                        optionDefinition.GetHelpDescriptionTextLines()
                        .Select(lineText =>
                            string.IsNullOrEmpty(lineText)
                            ? lineText
                            : $"    {lineText}")
                        .Prepend($"  * {optionDefinition.GetHelpSyntaxText()}")
                        .Append("")));
            foreach (var lineText in textLines)
                TinyConsole.Out.WriteLine(lineText);
        }

        private static void PrintExceptionMessage(Exception ex)
        {
            for (var exception = ex; exception != null; exception = exception.InnerException)
                PrintErrorMessage(exception.Message);
        }

        private static void PrintWarningMessage(string message)
            => PrintWarningMessage(_thisProgramName, message);

        private static void PrintWarningMessage(string programName, string message)
        {
            TinyConsole.ForegroundColor = ConsoleColor.Yellow;
            TinyConsole.Write($"{programName}:WARNING: {message}");
            TinyConsole.ResetColor();
            try
            {
                TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
            }
            catch (InvalidOperationException)
            {
            }

            TinyConsole.WriteLine();
        }

        private static void PrintErrorMessage(string message)
            => PrintErrorMessage(_thisProgramName, message);

        private static void PrintErrorMessage(string programName, string message)
        {
            TinyConsole.ForegroundColor = ConsoleColor.Red;
            TinyConsole.Write($"{programName}:ERROR: {message}");
            TinyConsole.ResetColor();
            try
            {
                TinyConsole.Erase(ConsoleEraseMode.FromCursorToEndOfLine);
            }
            catch (InvalidOperationException)
            {
            }

            TinyConsole.WriteLine();
        }
    }
}
