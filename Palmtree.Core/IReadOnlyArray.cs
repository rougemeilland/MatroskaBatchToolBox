using System;
using System.Collections.Generic;

namespace Palmtree
{
    /// <summary>
    /// 読み込み専用の配列のインターフェースです。
    /// </summary>
    /// <typeparam name="VALUE_T">
    /// 配列の</typeparam>
    public interface IReadOnlyArray<VALUE_T>
        : IEnumerable<VALUE_T>, IReadOnlyIndexer<int, VALUE_T>
    {
        /// <summary>
        /// 配列の長さです。
        /// </summary>
        int Length { get; }

        /// <summary>
        /// 配列を参照する <see cref="ReadOnlySpan{VALUE_T}"/> を作成します。
        /// </summary>
        /// <returns>
        /// 配列を参照する <see cref="ReadOnlySpan{VALUE_T}"/> です。
        /// </returns>
        ReadOnlySpan<VALUE_T> AsSpan();

        /// <summary>
        /// 配列を参照する <see cref="ReadOnlyMemory{VALUE_T}"/> を作成します。
        /// </summary>
        /// <returns>
        /// 配列を参照する <see cref="ReadOnlyMemory{VALUE_T}"/> です。
        /// </returns>
        ReadOnlyMemory<VALUE_T> AsMemory();
    }
}
