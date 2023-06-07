using System;
using System.Linq;
using MatroskaBatchToolBox.Utility;
using MatroskaBatchToolBox.Utility.Interprocess;
using Palmtree;

namespace InterlaceChecker
{
    partial class Program
    {
        private static CommandOptionDefinition<OptionType>[] GetOptionDefinitions()
            => new CommandOptionDefinition<OptionType>[]
            {
#region 入出力オプション
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
#endregion

#region トリミングオプション
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
                                arg.TryParse(TimeParsingMode.LazyMode, out TimeSpan duration)
                                ? new object[] { (TimeSpan?)duration }
                                : throw new InvalidCommandOptionException($"Invalid time format: {arg}"),
                            _ => throw Validation.GetFailErrorException($"parsing {optionName}"),
                        },
                    // 同一オプションの重複のみ NG
                    (option, otherOpton) => !(otherOpton.OptionType == option.OptionType),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "-ss <start time for trimming>",
                    new[]
                    {
                        "Specifies the start time for trimming.",
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
                                arg.TryParse(TimeParsingMode.LazyMode, out TimeSpan duration)
                                ? new object[] { (TimeSpan?)duration }
                                : throw new InvalidCommandOptionException($"Invalid time format: {arg}"),
                            _ => throw Validation.GetFailErrorException($"parsing {optionName}"),
                        },
                    // 同一オプションの重複、または -t オプションとの混在で NG
                    (option, otherOpton) => otherOpton.OptionType.IsNoneOf(option.OptionType, OptionType.DurationForTrimming),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "-to <end time for trimming>",
                    new[]
                    {
                        "Specifies the end time for trimming.",
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
                                arg.TryParse(TimeParsingMode.LazyMode, out TimeSpan duration)
                                ? new object[] { (TimeSpan?)duration }
                                : throw new InvalidCommandOptionException($"Invalid time format: {arg}"),
                            _ => throw Validation.GetFailErrorException($"parsing {optionName}"),
                        },
                    // 同一オプションの重複、または -to オプションとの混在で NG
                    (option, otherOpton) => otherOpton.OptionType.IsNoneOf(option.OptionType, OptionType.ToForTrimming),
                    // 常に OK (必須ではない)
                    (options) => null,
                    "-t <duration time for trimming>",
                    new[]
                    {
                        "Specifies the duration for trimming.",
                    }),
#endregion

#region その他のオプション
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
#endregion
            };
    }
}
