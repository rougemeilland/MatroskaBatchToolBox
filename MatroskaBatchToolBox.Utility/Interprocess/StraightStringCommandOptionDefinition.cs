using System;
using System.Collections.Generic;
using System.Linq;

namespace MatroskaBatchToolBox.Utility.Interprocess
{
    /// <summary>
    /// オプションの名前を単純比較するオプションの定義のクラスです。
    /// </summary>
    /// <typeparam name="COMMAND_OPTION_TYPE_T">
    /// オプションを識別するための列挙体です。
    /// </typeparam>
    public class StraightStringCommandOptionDefinition<COMMAND_OPTION_TYPE_T>
        : CommandOptionDefinition<COMMAND_OPTION_TYPE_T>
        where COMMAND_OPTION_TYPE_T : Enum
    {
        private readonly IEnumerable<string> _optionNames;

        /// <summary>
        /// エイリアスがなく追加引数もないオプションの定義オブジェクトを初期化するコンストラクタです。
        /// </summary>
        /// <param name="optionType">
        /// オプションを識別する値です。
        /// </param>
        /// <param name="optionName">
        /// オプションの名前です。
        /// </param>
        /// <param name="exclusiveOptionChecker">
        /// オプションと他のオプションの排他条件を満足しているかを調べるメソッドのデリゲートです。
        /// 排他条件を満たしていれば true, 排他条件に違反していれば false を返してください。
        /// </param>
        /// <param name="requiredOptionChecker">
        /// オプションの必須条件を満足しているかどうかを調べるメソッドのデリゲートです。
        /// 必須条件を満たしていれば null、必須条件に違反していればその情報のメッセージテキストを返してください。
        /// </param>
        /// <param name="syntaxTextForHelp">
        /// ヘルプ情報に表示されるオプションの構文です。
        /// </param>
        /// <param name="descriptionTextLinesForHelp">
        /// ヘルプ情報に表示されるオプションの説明の行のコレクションです。
        /// </param>
        public StraightStringCommandOptionDefinition(
            COMMAND_OPTION_TYPE_T optionType,
            string optionName,
            Func<CommandOption<COMMAND_OPTION_TYPE_T>, CommandOption<COMMAND_OPTION_TYPE_T>, bool> exclusiveOptionChecker,
            Func<IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>>, string?> requiredOptionChecker,
            string syntaxTextForHelp,
            IEnumerable<string> descriptionTextLinesForHelp)
            : this(optionType, new[] { optionName }, 0, (index, arg, optionName) => new object[] { arg }, exclusiveOptionChecker, requiredOptionChecker, syntaxTextForHelp, descriptionTextLinesForHelp)
        {
        }

        /// <summary>
        /// 追加引数がないオプションの定義オブジェクトを初期化するコンストラクタです。
        /// </summary>
        /// <param name="optionType">
        /// オプションを識別する値です。
        /// </param>
        /// <param name="optionNames">
        /// エイリアスも含めたオプションの名前のコレクションです。
        /// </param>
        /// <param name="exclusiveOptionChecker">
        /// オプションと他のオプションの排他条件を満足しているかを調べるメソッドのデリゲートです。
        /// 排他条件を満たしていれば true, 排他条件に違反していれば false を返してください。
        /// </param>
        /// <param name="requiredOptionChecker">
        /// オプションの必須条件を満足しているかどうかを調べるメソッドのデリゲートです。
        /// 必須条件を満たしていれば null、必須条件に違反していればその情報のメッセージテキストを返してください。
        /// </param>
        /// <param name="syntaxTextForHelp">
        /// ヘルプ情報に表示されるオプションの構文です。
        /// </param>
        /// <param name="descriptionTextLinesForHelp">
        /// ヘルプ情報に表示されるオプションの説明の行のコレクションです。
        /// </param>
        public StraightStringCommandOptionDefinition(
            COMMAND_OPTION_TYPE_T optionType,
            IEnumerable<string> optionNames,
            Func<CommandOption<COMMAND_OPTION_TYPE_T>, CommandOption<COMMAND_OPTION_TYPE_T>, bool> exclusiveOptionChecker,
            Func<IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>>, string?> requiredOptionChecker,
            string syntaxTextForHelp,
            IEnumerable<string> descriptionTextLinesForHelp)
            : this(optionType, optionNames, 0, (index, arg, optionName) => new object[] { arg }, exclusiveOptionChecker, requiredOptionChecker, syntaxTextForHelp, descriptionTextLinesForHelp)
        {
        }

        /// <summary>
        /// エイリアスがないオプションの定義オブジェクトを初期化するコンストラクタです。
        /// </summary>
        /// <param name="optionType">
        /// オプションを識別する値です。
        /// </param>
        /// <param name="optionName">
        /// オプションの名前です。
        /// </param>
        /// <param name="optionParameters">
        /// オプションの追加引数の数です。
        /// </param>
        /// <param name="argumentConverter">
        /// オプションの引数を実際に使用する型に変換するメソッドのデリゲートです。
        /// コマンドラインの構文の誤りにより変換に失敗した場合には <see cref="InvalidCommandOptionException"/> 例外をスローしてください。
        /// </param>
        /// <param name="exclusiveOptionChecker">
        /// オプションと他のオプションの排他条件を満足しているかを調べるメソッドのデリゲートです。
        /// 排他条件を満たしていれば true, 排他条件に違反していれば false を返してください。
        /// </param>
        /// <param name="requiredOptionChecker">
        /// オプションの必須条件を満足しているかどうかを調べるメソッドのデリゲートです。
        /// 必須条件を満たしていれば null、必須条件に違反していればその情報のメッセージテキストを返してください。
        /// </param>
        /// <param name="syntaxTextForHelp">
        /// ヘルプ情報に表示されるオプションの構文です。
        /// </param>
        /// <param name="descriptionTextLinesForHelp">
        /// ヘルプ情報に表示されるオプションの説明の行のコレクションです。
        /// </param>
        public StraightStringCommandOptionDefinition(
            COMMAND_OPTION_TYPE_T optionType,
            string optionName,
            int optionParameters,
            Func<int, string, string, IEnumerable<object>> argumentConverter,
            Func<CommandOption<COMMAND_OPTION_TYPE_T>, CommandOption<COMMAND_OPTION_TYPE_T>, bool> exclusiveOptionChecker,
            Func<IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>>, string?> requiredOptionChecker,
            string syntaxTextForHelp,
            IEnumerable<string> descriptionTextLinesForHelp)
            : this(optionType, new[] { optionName }, optionParameters, argumentConverter, exclusiveOptionChecker, requiredOptionChecker, syntaxTextForHelp, descriptionTextLinesForHelp)
        {
        }

        /// <summary>
        /// オプションの定義オブジェクトを初期化するコンストラクタです。
        /// </summary>
        /// <param name="optionType">
        /// オプションを識別する値です。
        /// </param>
        /// <param name="optionNames">
        /// エイリアスも含めたオプションの名前のコレクションです。
        /// </param>
        /// <param name="optionParameters">
        /// オプションの追加引数の数です。
        /// </param>
        /// <param name="argumentConverter">
        /// オプションの引数を実際に使用する型に変換するメソッドのデリゲートです。
        /// コマンドラインの構文の誤りにより変換に失敗した場合には <see cref="InvalidCommandOptionException"/> 例外をスローしてください。
        /// </param>
        /// <param name="exclusiveOptionChecker">
        /// オプションと他のオプションの排他条件を満足しているかを調べるメソッドのデリゲートです。
        /// 排他条件を満たしていれば true, 排他条件に違反していれば false を返してください。
        /// </param>
        /// <param name="requiredOptionChecker">
        /// オプションの必須条件を満足しているかどうかを調べるメソッドのデリゲートです。
        /// 必須条件を満たしていれば null、必須条件に違反していればその情報のメッセージテキストを返してください。
        /// </param>
        /// <param name="syntaxTextForHelp">
        /// ヘルプ情報に表示されるオプションの構文です。
        /// </param>
        /// <param name="descriptionTextLinesForHelp">
        /// ヘルプ情報に表示されるオプションの説明の行のコレクションです。
        /// </param>
        public StraightStringCommandOptionDefinition(
            COMMAND_OPTION_TYPE_T optionType,
            IEnumerable<string> optionNames,
            int optionParameters,
            Func<int, string, string, IEnumerable<object>> argumentConverter,
            Func<CommandOption<COMMAND_OPTION_TYPE_T>, CommandOption<COMMAND_OPTION_TYPE_T>, bool> exclusiveOptionChecker,
            Func<IEnumerable<CommandOption<COMMAND_OPTION_TYPE_T>>, string?> requiredOptionChecker,
            string syntaxTextForHelp,
            IEnumerable<string> descriptionTextLinesForHelp)
            : base(optionType, optionParameters, argumentConverter, exclusiveOptionChecker, requiredOptionChecker, syntaxTextForHelp, descriptionTextLinesForHelp)
        {
            _optionNames = optionNames.ToArray();
        }

        /// <summary>
        /// 与えられた引数の文字列がオプションの名前とマッチするかどうかを調べます。
        /// </summary>
        /// <param name="argument">
        /// 引数の文字列です。
        /// </param>
        /// <returns>
        /// <paramref name="argument"/>で与えられた文字列がオプションの名前とマッチしていれば true、そうではない場合は false です。
        /// </returns>
        public override bool IsMatch(string argument)
            => _optionNames.Contains(argument);
    }
}
