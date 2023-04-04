namespace Palmtree
{
    /// <summary>
    /// カーソルの可視性や表示方法を示す列挙体です。
    /// </summary>
    public enum ConsoleCursorVisiblity
    {
        /// <summary>
        /// カーソルの可視性が不明です。
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// カーソルが表示されません。
        /// </summary>
        Invisible = 0,

        /// <summary>
        /// カーソルが通常に表示されます。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// カーソルがどのように表示されるかはターミナルの実装によって異なります。(例: 縦棒、下線など)
        /// </item>
        /// </list>
        /// </remarks>
        NormalMode = 1,

        /// <summary>
        /// カーソルが見やすく表示されます。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>
        /// カーソルがどのように表示されるかはターミナルの実装によって異なります。(例: ブロック、点滅する下線、<see cref="NormalMode"/> と同じ表示方法、など)
        /// </item>
        /// </list>
        /// </remarks>
        HighVisibilityMode = 2,
    }
}
