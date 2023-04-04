namespace Palmtree.Terminal
{
    /// <summary>
    /// 下線の種類を表す列挙体です。
    /// </summary>
    public enum UnderlineStyle
    {
        /// <summary>
        /// 下線なし
        /// </summary>
        None = 0,

        /// <summary>
        /// 通常の下線
        /// </summary>
        NormalUnderline = 1,

        /// <summary>
        /// 二重の下線
        /// </summary>
        DoubleUnderline = 2,

        /// <summary>
        /// 波の下線
        /// </summary>
        CurlyUnderline = 3,

        /// <summary>
        /// 点線の下線
        /// </summary>
        DottedUnderline = 4,

        /// <summary>
        /// 破線の下線
        /// </summary>
        DashedUnderline = 5,

        /// <summary>
        /// <see cref="UnderlineStyle"/> の最小値です。
        /// </summary>
        Minimum = None,

        /// <summary>
        /// <see cref="UnderlineStyle"/> の最大値です。
        /// </summary>
        Maximum = DashedUnderline,
    }
}
