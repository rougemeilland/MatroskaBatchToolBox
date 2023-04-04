namespace Palmtree.Terminal
{
    /// <summary>
    /// カーソルの表示方法を表す列挙体です。
    /// </summary>
    public enum CursorStyle
    {
        /// <summary>
        /// 既定
        /// </summary>
        Default = 0,

        /// <summary>
        /// 点滅ブロック
        /// </summary>
        BlinkingBlock = 1,

        /// <summary>
        /// ブロック
        /// </summary>
        SteadyBlock = 2,

        /// <summary>
        /// 点滅下線
        /// </summary>
        BlinkingUnderline = 3,

        /// <summary>
        /// 下線
        /// </summary>
        SteadyUnderline = 4,

        /// <summary>
        /// 点滅バー
        /// </summary>
        BlinkingBar = 5,

        /// <summary>
        /// バー
        /// </summary>
        SteadyBar = 6,

        /// <summary>
        /// <see cref="CursorStyle"/>の最小値
        /// </summary>
        Minimum = Default,

        /// <summary>
        /// <see cref="CursorStyle"/>の最大値
        /// </summary>
        Maximum = SteadyBar,
    }
}
