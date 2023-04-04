namespace Palmtree
{
    /// <summary>
    /// 取得/設定が可能なインデクサーのインターフェースです。
    /// </summary>
    /// <typeparam name="INDEX_T">インデックスの型です。</typeparam>
    /// <typeparam name="VALUE_T">値の型です。</typeparam>
    public interface IIndexer<INDEX_T, VALUE_T>
        : IReadOnlyIndexer<INDEX_T, VALUE_T>
    {
        /// <summary>
        /// 指定されたインデックスに対応する値を取得または設定します。
        /// </summary>
        /// <param name="index">インデックスである <typeparamref name="INDEX_T"/> です。</param>
        /// <returns>
        /// 取得した値である <typeparamref name="VALUE_T"/> です。
        /// </returns>
        new VALUE_T this[INDEX_T index] { get; set; }
    }
}
