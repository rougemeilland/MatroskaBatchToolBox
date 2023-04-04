namespace Palmtree
{
    /// <summary>
    /// ライングラフィックスの描画で指定するシンボルの列挙体です。
    /// </summary>
    public enum AlternativeChar
        : byte
    {
        /// <summary>
        /// 左上の罫線です。('┏'に似ています)
        /// </summary>
        ULCORNER = (byte)'l',

        /// <summary>
        /// 左下の罫線です。('┗'に似ています)
        /// </summary>
        LLCORNER = (byte)'m',

        /// <summary>
        /// 右上の罫線です。('┓'に似ています)
        /// </summary>
        URCORNER = (byte)'k',

        /// <summary>
        /// 右下の罫線です。('┛'に似ています)
        /// </summary>
        LRCORNER = (byte)'j',

        /// <summary>
        /// 縦棒から右に突き出た罫線です。('┣'に似ています)
        /// </summary>
        LTEE = (byte)'t',

        /// <summary>
        /// 縦棒から左に突き出た罫線です。('┫'に似ています)
        /// </summary>
        RTEE = (byte)'u',

        /// <summary>
        /// 横棒から上に突き出た罫線です。('┻'に似ています)
        /// </summary>
        BTEE = (byte)'v',

        /// <summary>
        /// 横棒から下に突き出た罫線です。('┳'に似ています)
        /// </summary>
        TTEE = (byte)'w',

        /// <summary>
        /// 横棒の罫線です。('━'に似ています)
        /// </summary>
        HLINE = (byte)'q',

        /// <summary>
        /// 縦棒の罫線です。('┃'に似ています)
        /// </summary>
        VLINE = (byte)'x',

        /// <summary>
        /// 縦棒と横棒が交差した罫線です。('╋'に似ています)
        /// </summary>
        PLUS = (byte)'n',

        /// <summary>
        /// 走査線1です。(上寄りの'-'に似ています。)
        /// </summary>
        S1 = (byte)'o',

        /// <summary>
        /// 走査線3です。(やや上寄りの'-'に似ています。)
        /// </summary>
        S3 = (byte)'p',

        /// <summary>
        /// 走査線7です。(やや下寄りの'-'に似ています。)
        /// </summary>
        S7 = (byte)'r',

        /// <summary>
        /// 走査線9です。(下寄りの'-'に似ています。)
        /// </summary>
        S9 = (byte)'s',

        /// <summary>
        /// ダイヤの記号です。('♦'に似ています。)
        /// </summary>
        DIAMOND = (byte)'`',

        /// <summary>
        /// 市松模様のブロックの記号です。
        /// </summary>
        CKBOARD = (byte)'a',

        /// <summary>
        /// 角度の記号です。('°'に似ています)
        /// </summary>
        DEGREE = (byte)'f',

        /// <summary>
        /// プラスマイナスの記号です。('±'に似ています)
        /// </summary>
        PLMINUS = (byte)'g',

        /// <summary>
        /// 中点の記号です。('・'に似ています。)
        /// </summary>
        BULLET = (byte)'~',

        /// <summary>
        /// 左向きの矢印です。('&lt;'に似ています。)
        /// </summary>
        LARROW = (byte)',',

        /// <summary>
        /// 右向きの矢印です。('&gt;'に似ています。)
        /// </summary>
        RARROW = (byte)'+',

        /// <summary>
        /// 下向きの矢印です。('v'に似ています。)
        /// </summary>
        DARROW = (byte)'.',

        /// <summary>
        /// 上向きの矢印です。('v'を上下反転したものに似ています。)
        /// </summary>
        UARROW = (byte)'-',

        /// <summary>
        /// 四角形の記号です。
        /// </summary>
        BOARD = (byte)'h',

        /// <summary>
        /// ランタンの記号です。
        /// </summary>
        LANTERN = (byte)'i',

        /// <summary>
        /// 塗りつぶされた四角形の記号です。
        /// </summary>
        BLOCK = (byte)'0',

        /// <summary>
        /// 小なりイコールの記号です。('≦'に似ています)
        /// </summary>
        LEQUAL = (byte)'y',

        /// <summary>
        /// 大なりイコールの記号です。('≧'に似ています)
        /// </summary>
        GEQUAL = (byte)'z',

        /// <summary>
        /// 円周率の記号です。('π'に似ています)
        /// </summary>
        PI = (byte)'{',

        /// <summary>
        /// 不等号の記号です。('≠'に似ています)
        /// </summary>
        NEQUAL = (byte)'|',

        /// <summary>
        /// ポンド記号です。('￡'に似ています)
        /// </summary>
        STERLING = (byte)'}',

        /// <summary>
        /// <see cref="ULCORNER"/>の別名です。
        /// </summary>
        BSSB = ULCORNER,

        /// <summary>
        /// <see cref="LLCORNER"/>の別名です。
        /// </summary>
        SSBB = LLCORNER,

        /// <summary>
        /// <see cref="URCORNER"/>の別名です。
        /// </summary>
        BBSS = URCORNER,

        /// <summary>
        /// <see cref="LRCORNER"/>の別名です。
        /// </summary>
        SBBS = LRCORNER,

        /// <summary>
        /// <see cref="RTEE"/>の別名です。
        /// </summary>
        SBSS = RTEE,

        /// <summary>
        /// <see cref="LTEE"/>の別名です。
        /// </summary>
        SSSB = LTEE,

        /// <summary>
        /// <see cref="BTEE"/>の別名です。
        /// </summary>
        SSBS = BTEE,

        /// <summary>
        /// <see cref="TTEE"/>の別名です。
        /// </summary>
        BSSS = TTEE,

        /// <summary>
        /// <see cref="HLINE"/>の別名です。
        /// </summary>
        BSBS = HLINE,

        /// <summary>
        /// <see cref="VLINE"/>の別名です。
        /// </summary>
        SBSB = VLINE,

        /// <summary>
        /// <see cref="PLUS"/>の別名です。
        /// </summary>
        SSSS = PLUS,
    }
}
