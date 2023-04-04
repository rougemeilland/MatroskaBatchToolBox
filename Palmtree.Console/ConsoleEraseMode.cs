namespace Palmtree
{
    /// <summary>
    /// コンソールの消去方法を表す列挙体です。
    /// </summary>
    public enum ConsoleEraseMode
    {
        /// <summary>
        /// カーソル位置からコンソールウィンドウの右下端まで消去します。
        /// </summary>
        FromCursorToEndOfScreen,

        /// <summary>
        /// コンソールウィンドウの左上端からカーソル位置の1文字前まで消去します。
        /// </summary>
        FromBeggingOfScreenToCursor,

        /// <summary>
        /// コンソールウィンドウ全体を消去して、カーソルをホームポジションに移動します。
        /// </summary>
        EntireScreen,

        /// <summary>
        /// コンソールバッファ全体を消去して、カーソルをホームポジションへ移動します。
        /// </summary>
        EntireConsoleBuffer,

        /// <summary>
        /// カーソルの位置から行の右端まで消去します。
        /// </summary>
        FromCursorToEndOfLine,

        /// <summary>
        /// カーソルの位置の1文字手前から左端まで消去します。
        /// </summary>
        FromBeggingOfLineToCursor,

        /// <summary>
        /// カーソルのある行全体を消去します。
        /// </summary>
        EntireLine,
    }
}
