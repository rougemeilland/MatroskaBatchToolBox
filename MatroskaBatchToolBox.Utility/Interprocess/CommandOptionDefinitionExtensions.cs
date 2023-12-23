using System;
using System.Collections.Generic;
using System.Linq;
using Palmtree;

namespace MatroskaBatchToolBox.Utility.Interprocess
{
    public static class CommandOptionDefinitionExtensions
    {
        /// <summary>
        /// 引数の配列を解析してオプションのコレクションを得ます。
        /// </summary>
        /// <typeparam name="COMMAND_OPTION_TYPE_T">
        /// オプションの種類を識別する型です。
        /// </typeparam>
        /// <param name="definitions">
        /// オプションの定義オブジェクトのコレクションです。
        /// </param>
        /// <param name="args">
        /// 引数の配列です。
        /// </param>
        /// <returns>
        /// 解析されたオプションのコレクションです。
        /// </returns>
        /// <exception cref="InvalidCommandOptionException">
        /// コマンドラインの構文に誤りがあった場合にスローされます。
        /// この例外のメッセージを適切な方法で利用者に通知してください。
        /// </exception>
        public static IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>> ParseCommandArguments<COMMAND_OPTION_TYPE_T>(this IEnumerable<CommandOptionDefinition<COMMAND_OPTION_TYPE_T>> definitions, ReadOnlyMemory<string> args)
            where COMMAND_OPTION_TYPE_T : Enum
        {
            var indexedDefinitions = definitions.ToDictionary(definition => definition.OptionType, definition => definition);
            var options =
                EnumerateOptions(definitions, args)
                .ToList();
            var numberedOptions =
                options
                .Select((option, index) => new { index, option })
                .ToList();
            foreach (var numberedOption in numberedOptions)
            {
                var option = numberedOption.option;
                var otherOptions =
                    numberedOptions
                    .Where(numberedOtherOption => numberedOtherOption.index != numberedOption.index)
                    .Select(numberedOtherOption => numberedOtherOption.option);
                var optionDefinition = indexedDefinitions[numberedOption.option.OptionType];
                optionDefinition.ValidateExclusiveOptions(option, otherOptions);
            }

            foreach (var optionDefinition in indexedDefinitions.Values)
                optionDefinition.ValidateOptionsRequirement(options);

            return options;
        }

        private static IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>> EnumerateOptions<COMMAND_OPTION_TYPE_T>(this IEnumerable<CommandOptionDefinition<COMMAND_OPTION_TYPE_T>> definitions, ReadOnlyMemory<string> args)
            where COMMAND_OPTION_TYPE_T : Enum
        {
            for (var index = 0; index < args.Length; ++index)
            {
                var matchedDefinitions = definitions.Where(definition => definition.IsMatch(args.Span[index])).ToArray();
                if (matchedDefinitions.Length <= 0)
                    throw new InvalidCommandOptionException($"Unsupported argument: \"{args.Span[index]}\"");
                Validation.Assert(matchedDefinitions.Length <= 1, "matchedDefinitions.Length <= 1");
                var matchedDefinition = matchedDefinitions[0];
                yield return
                    index + matchedDefinition.OptionParameters < args.Length
                    ? matchedDefinition.ParseArguments(
                        args.Span[index],
                        args.Slice(index + 1, matchedDefinition.OptionParameters))
                    : throw new InvalidCommandOptionException($"Not enough arguments.: [{string.Join(", ", args.Slice(index, args.Length - 1).GetSequence().Select(arg => $"\"{arg.Replace(@"\", @"\\").Replace(@"""", @"\""")}\""))}]");
                index += matchedDefinition.OptionParameters;
            }
        }
    }
}
