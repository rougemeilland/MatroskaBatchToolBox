using System;
using System.Linq;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Interprocess;
using Palmtree;

namespace MovieChapterEditor
{
    partial class Program
    {
        private static CommandOptionDefinition<OptionType>[] GetOptionDefinitions()
            => new CommandOptionDefinition<OptionType>[]
            {
                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.InputFormat,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-if", "--input_format" },
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) => new[]{ arg },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // --input オプションが指定されていない(==標準入力からの入力)、かつ --input_format オプションが指定されていない場合は NG
                    (options) =>
                    {
                        // -help オプションが含まれている場合は OK (そもそも他のオプションは指定できないため)
                        if (options.Any(option => option.OptionType == OptionType.Help))
                            return null;

                        // --input オプションが指定されている場合はOK (標準入力からの入力ではないため、--input_format の指定は必須ではない)
                        if (options.Any(option => option.OptionType == OptionType.Input))
                            return null;

                        // --input_format オプションが指定されている場合はOK
                        if (options.Any(option => option.OptionType == OptionType.InputFormat))
                            return null;

                        // -help オプションが指定されておらず、かつ --input オプションが指定されておらず、かつ --input_format オプションが指定されていない場合

                        // 標準入力から入力される動画ファイルの形式を判別できないため、エラーとする。
                        return "The \"--input_format\" option is not specified even though the \"--input\" option is not specified (that is, the input is from standard input).";
                    },
                    "--input_format <format name>  or  -if <format name>",
                    new[]
                    {
                        "Specifies the format of the input movie file.",
                        "This is the same value that can be specified with the \"-f\" option of the \"ffmpeg\" command.",
                        "If this option is omitted, the format of the movie file is guessed from the extension of the input movie file specified by the \"--input\" option.",
                        "Note that this option must be specified if the \"--input\" option is omitted and the movie file is input from standard input.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.Input,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-i", "--input" },
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) => new[]{ arg },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--input <input file path>  or  -i <input file path>",
                    new[]
                    {
                        "Specifies the path name of the input movie file.",
                        "If this option is omitted, the movie file is read from standard input.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.OutputFormat,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-of", "--output_format" },
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) => new[]{ arg },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // --output オプションが指定されていない(==標準出力へ出力)、かつ --output_format オプションが指定されていない場合は NG
                    (options) =>
                    {
                        // -help オプションが含まれている場合は OK (そもそも他のオプションは指定できないため)
                        if (options.Any(option => option.OptionType == OptionType.Help))
                            return null;

                        // --output オプションが指定されている場合はOK (標準出力への出力ではないため、--output_format の指定は必須ではない)
                        if (options.Any(option => option.OptionType == OptionType.Output))
                            return null;

                        // --output_format オプションが指定されている場合はOK
                        if (options.Any(option => option.OptionType == OptionType.OutputFormat))
                            return null;

                        // -help オプションが指定されておらず、かつ --output オプションが指定されておらず、かつ --output_format オプションが指定されていない場合

                        // 標準出力へ出力する動画ファイルの形式を判別できないため、エラーとする。
                        return "The \"--output_format\" option is not specified even though the \"--output\" option is not specified (i.e. output to standard output).";
                    },
                    "--output_format <format name>  or  -of <format name>",
                    new[]
                    {
                        "Specifies the format of the output movie file.",
                        "This is the same value that can be specified with the \"-f\" option of the \"ffmpeg\" command.",
                        "If this option is omitted, the format of the movie file is guessed from the extension of the output movie file specified by the \"--output\" option.",
                        "Note that this option must be specified if the \"--output\" option is omitted and the movie file is output to standard output.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.Output,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-o", "--output" },
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) => new[]{ arg },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--output <output file path>  or  -o <output file path>",
                    new[]
                    {
                        "Specifies the path name of the output movie file.",
                        "If this option is omitted, the movie file will be written to standard output.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.Force,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-f", "--force" },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--force  or  -f",
                    new[]
                    {
                        "Overwrite the output file if it already exists.",
                        "If this option is not specified, an error will occur if the output file already exists.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.ChapterTimes,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-ct", "--chapter_times" },
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) =>
                    {
                        switch (index)
                        {
                            // 最初の引数(オプション名)は変換しない
                            case 0:
                                return new object[] { arg };
                            // 1番目の追加引数を TimeSpan のコレクションに変換する
                            case 1:
                            {
                                try
                                {
                                    return new object[] { arg.ParseAsChapterStartTimes().ToReadOnlyArray() };
                                }
                                catch (Exception ex)
                                {
                                    throw new InvalidCommandOptionException($"The value format of the \"--chapter_time\" option is invalid. : \"{arg}\"", ex);
                                }
                            }
                            // 追加引数の数が範囲外であれば例外 (自己矛盾)
                            default:
                                throw Validation.GetFailErrorException($"parsing {optionName}");
                        }
                    },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--chapter_times <chapter time list>  or  -ct <chapter time list>",
                    new []
                    {
                        "Set chapter time.",
                        "\"<chapter time list>\" is a comma-separated string of chapter start times.",
                        "You can also specify a delta from the previous chapter's start time by prefixing each start time with a plus sign ('+').",
                        "The end time of each chapter is set to the start time of the previous chapter.",
                        "The end time of the last chapter is set to 168 hours (==7 days) by default.",
                        "This value can be changed with the \"--maximum_duration\" option.",
                        "For example:",
                        "",
                        "  --chapter_times 0,101.835,211.144,316.115",
                        "  --chapter_times 0,0:01:41.835,0:03:31.144,0:05:16.115",
                        "  --chapter_times 0,+101.835,+109.309,+104.971",
                        "  --chapter_times 0,+0:01:41.835,+0:01:49.309,+0:01:44.971",
                        "",
                        "All of the above option specifications are equivalent and set the following chapters.",
                        "",
                        "  #0 start-time=00:00:00.000, end-time=00:01:41.835",
                        "  #1 start-time=00:01:41.835, end-time=00:03:31.144",
                        "  #2 start-time=00:03:31.144, end-time=00:05:16.115",
                        "  #3 start-time=00:05:16.115, end-time=168:00:00.000",
                    }),

                new RegexCommandOptionDefinition<OptionType>(
                    OptionType.ChapterTitle,
                    // オプション名の正規表現
                    _setTitleOptionNamePattern,
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) =>
                    {
                        switch (index)
                        {
                            // 最初の引数(オプション名)に付属しているインデックスを抽出して int に変換する
                            case 0:
                            {
                                var match = _setTitleOptionNamePattern.Match(arg);
                                Validation.Assert(match.Success, "match.Success (at parsing {optionName})");
                                var chapterIndex = match.Groups["chapterIndex"].Value.ParseAsInt32();
                                return new object []{ arg, chapterIndex };
                            }
                            // 1番目の追加引数は文字列型なので特に変換はしない
                            case 1:
                                return new object[]{ arg };
                            // 追加引数の数が範囲外であれば例外 (自己矛盾)
                            default:
                                throw Validation.GetFailErrorException($"parsing {optionName}");
                        }
                    },
                    // 同一オプションかつ同一インデックスの重複のみ NG
                    (option, otherOpton) =>
                        !(option.OptionType == otherOpton.OptionType && option.OptionParameter[1].Equals(otherOpton.OptionParameter[1])),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--set_title:<chapter number> <chapter title>  or  -tt:<chapter number> <chapter title>",
                    new[]
                    {
                        "Change the chapter title.",
                        "\"<chapter number>\" is the chapter number starting from 0, and \"<chapter title>\" is the title to set for that chapter.",
                        "  Example:",
                        "    --set_title:0 1.Prologue",
                        "    --set_title:1 \"2.Opening theme\"",
                        "",
                        "If you want to delete the chapter title, specify an empty string for the title.",
                        "  Example:",
                        "    --set_title:2 \"\"",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.MaximumDuration,
                     // オプション名
                    "--maximum_duration",
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) =>
                        index switch
                        {
                            0 => new object[] { arg },
                            1 =>
                                arg.TryParse(false, out TimeSpan duration)
                                ? new object[] { (TimeSpan?)duration }
                                : throw new InvalidCommandOptionException($"Invalid time format: {arg}"),
                            _ => throw Validation.GetFailErrorException($"parsing {optionName}"),
                        },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--maximum_duration <duratuon time>",
                    new[]
                    {
                        "Specifies the time to apply instead of the end time of the last chapter if it is unknown.",
                        "The value for this option can be specified in hour-minute-second format (hh:mm.ss.sss) or second format (sssss.sss).",
                        "The default value for this option is 168:00:00.000 (7 days).",
                        "Normally you do not need to change the value of this option.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.MinimumDuration,
                     // オプション名
                    "--minimum_duration",
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) =>
                        index switch
                        {
                            0 => new object[] { arg },
                            1 =>
                                arg.TryParse(false, out TimeSpan duration)
                                ? new object[] { (TimeSpan?)duration }
                                : throw new InvalidCommandOptionException($"Invalid time format: {arg}"),
                            _ => throw Validation.GetFailErrorException($"parsing {optionName}"),
                        },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--minimum_duration <duratuon time>",
                    new[]
                    {
                        "Specifies the minimum valid chapter length.",
                        "The default is 0.01 (10 ms).",
                        "Chapters shorter than this value are automatically merged with the chapters before and after them.",
                        "",
                        "* Commentary",
                        "  In general, chapters that are too short to be visible are meaningless.",
                        $"  So {_thisProgramName} will automatically merge chapters shorter than the value specified in this option with the chapter before or after it.",
                        "",
                        "  More specifically, if the first chapter is too short, merge it with the second chapter.",
                        "  Also, if the second and subsequent chapters are too short, they are combined with the previous chapter.",
                        "",
                        "  Titles before merging are carried over to the merged chapters.",
                        "  However, if both chapters had titles before the merge, one title will be lost and a warning message will be displayed.",
                        "",
                        "  If you want to suppress these operations, specify the \"--minimum_duration 0\" option.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.FromForTrimming,
                     // オプション名
                    "-ss",
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) =>
                        index switch
                        {
                            0 => new object[] { arg },
                            1 =>
                                arg.TryParse(false, out TimeSpan duration)
                                ? new object[] { (TimeSpan?)duration }
                                : throw new InvalidCommandOptionException($"Invalid time format: {arg}"),
                            _ => throw Validation.GetFailErrorException($"parsing {optionName}"),
                        },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // --chapter_times オプションが指定されている場合のみOK (必須ではない)
                    (options) =>
                        options.None(option => option.OptionType == OptionType.FromForTrimming) ||
                        options.Any(option => option.OptionType == OptionType.ChapterTimes)
                        ? null
                        : "The \"-ss\" option can only be specified when the \"--chapter_times\" option is specified.",
                    "-ss <start time for trimming>",
                    new[]
                    {
                        "Specify the trimming start time for the chapter time specified by the \"----chapter_times\" option.",
                        "Specify the time in hour-minute-second format (eg 00:12:34.567) or seconds format (eg 1234.567).",
                        "The default is 0 (i.e. from the beginning).",
                        "",
                        "This option is useful in the following situations:",
                        "For example, you have trimmed a video file, but you only know the chapter times of the video file before trimming.",
                        "In that case, specify the chapter time of the video before trimming in the \"----chapter_times\" option.",
                        "Then specify the trimming range of the video file with the \"-ss\" option, \"-to\" option and \"-t\" option.",
                        "That way you don't have to do the math yourself to correct the chapter times.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.ToForTrimming,
                     // オプション名
                    "-to",
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) =>
                        index switch
                        {
                            0 => new object[] { arg },
                            1 =>
                                arg.TryParse(false, out TimeSpan duration)
                                ? new object[] { (TimeSpan?)duration }
                                : throw new InvalidCommandOptionException($"Invalid time format: {arg}"),
                            _ => throw Validation.GetFailErrorException($"parsing {optionName}"),
                        },
                    // 同一オプションの重複、または -t オプションとの混在で NG
                    (option, otherOpton) => otherOpton.OptionType.IsNoneOf(option.OptionType, OptionType.DurationForTrimming),
                    // --chapter_times オプションが指定されている場合のみOK (必須ではない)
                    (options) =>
                        options.None(option => option.OptionType == OptionType.FromForTrimming) ||
                        options.Any(option => option.OptionType == OptionType.ChapterTimes)
                        ? null
                        : "The \"-to\" option can only be specified when the \"--chapter_times\" option is specified.",
                    "-to <end time for trimming>",
                    new[]
                    {
                        "Specify the trimming end time for the chapter time specified by the \"--chapter_times\" option.",
                        "Specify the time in hour-minute-second format (eg 00:12:34.567) or seconds format (eg 1234.567).",
                        "The default is \"extremely long duration\" (i.e. to the end).",
                        "",
                        "This option is useful in the following situations:",
                        "For example, you have trimmed a video file, but you only know the chapter times of the video file before trimming.",
                        "In that case, specify the chapter time of the video before trimming in the \"--chapter_times\" option.",
                        "Then specify the trimming range of the video file with the \"-ss\" option, \"-to\" option and \"-t\" option.",
                        "That way you don't have to do the math yourself to correct the chapter times.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.DurationForTrimming,
                     // オプション名
                    "-t",
                    // 追加引数の数
                    1,
                    // 引数の変換
                    (index, arg, optionName) =>
                        index switch
                        {
                            0 => new object[] { arg },
                            1 =>
                                arg.TryParse(false, out TimeSpan duration)
                                ? new object[] { (TimeSpan?)duration }
                                : throw new InvalidCommandOptionException($"Invalid time format: {arg}"),
                            _ => throw Validation.GetFailErrorException($"parsing {optionName}"),
                        },
                    // 同一オプションの重複、または -to オプションとの混在で NG
                    (option, otherOpton) => otherOpton.OptionType.IsNoneOf(option.OptionType, OptionType.ToForTrimming),
                    // --chapter_times オプションが指定されている場合のみOK (必須ではない)
                    (options) =>
                        options.None(option => option.OptionType == OptionType.FromForTrimming) ||
                        options.Any(option => option.OptionType == OptionType.ChapterTimes)
                        ? null
                        : "The \"-t\" option can only be specified when the \"--chapter_times\" option is specified.",
                    "-t <duration time for trimming>",
                    new[]
                    {
                        "Specify the length of time in the trimming range for the chapter time specified by the \"----chapter_times\" option.",
                        "Specify the time in hour-minute-second format (eg 00:12:34.567) or seconds format (eg 1234.567).",
                        "The default is \"extremely long duration\" (i.e. to the end).",
                        "",
                        "This option is useful in the following situations:",
                        "For example, you have trimmed a video file, but you only know the chapter times of the video file before trimming.",
                        "In that case, specify the chapter time of the video before trimming in the \"----chapter_times\" option.",
                        "Then specify the trimming range of the video file with the \"-ss\" option, \"-to\" option and \"-t\" option.",
                        "That way you don't have to do the math yourself to correct the chapter times.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.KeepEemptyChapter,
                     // エイリアスも含めたオプション名の配列
                    "--keep_empty_chapter",
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--keep_empty_chapter",
                    new[]
                    {
                        "Causes zero-length chapters to be output as-is.",
                        "By default chapconv automatically removes zero length chapters.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.Verbose,
                     // エイリアスも含めたオプション名の配列
                    new string[]{ "-v", "--verbose" },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "--verbose  or  -v",
                    new[]
                    {
                        "More information will be displayed.",
                    }),

                new StraightStringCommandOptionDefinition<OptionType>(
                    OptionType.Help,
                     // オプション名
                    "-help",
                    // 常にNG (同一オプションの重複または他のすべてのオプションとの混在がNG)
                    (option, otherOpton) => false,
                    // 常に OK (必須ではない)
                    (options) => null,
                    "-help",
                    new[]
                    {
                        "Prints this document to standard output."
                    }),
            };
    }
}
