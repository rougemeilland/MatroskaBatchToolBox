using System;
using System.Collections.Generic;
using Palmtree;

namespace MatroskaBatchToolBox.Utility.Interprocess
{
    /// <summary>
    /// コマンドオプションのクラスです。
    /// </summary>
    /// <typeparam name="COMMAND_OPTION_TYPE_T">
    /// オプションの種類を識別する型です。
    /// </typeparam>
    public class CommandOption<COMMAND_OPTION_TYPE_T>
        where COMMAND_OPTION_TYPE_T : Enum
    {
        internal CommandOption(COMMAND_OPTION_TYPE_T optionType, string optionName, IEnumerable<object> optionParameter)
        {
            OptionType = optionType;
            OptionName = optionName;
            OptionParameter = optionParameter.ToReadOnlyArray();
        }

        /// <summary>
        /// オプションの種類を識別する値です。
        /// </summary>
        public COMMAND_OPTION_TYPE_T OptionType { get; }

        /// <summary>
        /// 引数で与えられたオプションの名前です。
        /// </summary>
        public string OptionName { get; }

        /// <summary>
        /// オプションの追加パラメタです。
        /// </summary>
        public IReadOnlyArray<object> OptionParameter { get; }
    }
}
