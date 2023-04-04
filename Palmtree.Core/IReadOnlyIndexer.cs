namespace Palmtree
{
    /// <summary>
    /// 読み込み専用のインデクサのインターフェースです。
    /// </summary>
    /// <typeparam name="INDEX_T">インデックスの型です。</typeparam>
    /// <typeparam name="VALUE_T">値の型です。</typeparam>
    public interface IReadOnlyIndexer<INDEX_T, VALUE_T>
    {
        /// <summary>
        /// インデックスに対応する値を取得します。
        /// </summary>
        /// <param name="index">インデックスである <typeparamref name="INDEX_T"/> 値です。 </param>
        /// <returns>
        /// インデックスに対応する値である <typeparamref name="VALUE_T"/> 値です。
        /// </returns>
        VALUE_T this[INDEX_T index] { get; }
    }
}
