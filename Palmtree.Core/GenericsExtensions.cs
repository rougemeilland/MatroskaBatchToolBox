using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Palmtree
{
    /// <summary>
    /// ジェネリック拡張メソッドのクラスです。
    /// </summary>
    public static class GenericsExtensions
    {
        #region ReadOnlyArray<VALUE_T> class

        private class ReadOnlyArray<VALUE_T>
            : IReadOnlyArray<VALUE_T>
        {
            private readonly VALUE_T[] _sourceArray;

            public ReadOnlyArray(VALUE_T[] sourceArray) => _sourceArray = sourceArray;

            VALUE_T IReadOnlyIndexer<int, VALUE_T>.this[int index] => _sourceArray[index];

            int IReadOnlyArray<VALUE_T>.Length => _sourceArray.Length;

            ReadOnlyMemory<VALUE_T> IReadOnlyArray<VALUE_T>.AsMemory() => _sourceArray.AsMemory();
            ReadOnlySpan<VALUE_T> IReadOnlyArray<VALUE_T>.AsSpan() => _sourceArray.AsSpan();
            IEnumerator<VALUE_T> IEnumerable<VALUE_T>.GetEnumerator() => _sourceArray.AsEnumerable().GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => _sourceArray.GetEnumerator();
        }

        #endregion

        #region IsAnyOf

        /// <summary>
        /// ある値が別の 2 つの値の何れかに等しいかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">値の型です。</typeparam>
        /// <param name="value">調べる値です。</param>
        /// <param name="otherValue1">比較する最初の値です。</param>
        /// <param name="otherValue2">比較する 2 番目の値です。</param>
        /// <returns><paramref name="value"/> が <paramref name="otherValue1"/> または <paramref name="otherValue2"/> の何れかと等しいなら true、そうではないのなら false です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2)
            => value is null
                ? otherValue1 is null || otherValue2 is null
                : value.Equals(otherValue1) || value.Equals(otherValue2);

        /// <summary>
        /// ある値が別の 3 つの値の何れかに等しいかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">値の型です。</typeparam>
        /// <param name="value">調べる値です。</param>
        /// <param name="otherValue1">比較する最初の値です。</param>
        /// <param name="otherValue2">比較する 2 番目の値です。</param>
        /// <param name="otherValue3">比較する 3 番目の値です。</param>
        /// <returns><paramref name="value"/> が <paramref name="otherValue1"/> または <paramref name="otherValue2"/>、 <paramref name="otherValue3"/> の何れかと等しいなら true、そうではないのなら false です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3)
            => value is null
                ? otherValue1 is null || otherValue2 is null || otherValue3 is null
                : value.Equals(otherValue1) || value.Equals(otherValue2) || value.Equals(otherValue3);

        /// <summary>
        /// ある値が別の 4 つの値の何れかに等しいかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">値の型です。</typeparam>
        /// <param name="value">調べる値です。</param>
        /// <param name="otherValue1">比較する最初の値です。</param>
        /// <param name="otherValue2">比較する 2 番目の値です。</param>
        /// <param name="otherValue3">比較する 3 番目の値です。</param>
        /// <param name="otherValue4">比較する 4 番目の値です。</param>
        /// <returns><paramref name="value"/> が <paramref name="otherValue1"/> または <paramref name="otherValue2"/>、 <paramref name="otherValue3"/>、 <paramref name="otherValue4"/> の何れかと等しいなら true、そうではないのなら false です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4)
            => value is null
                ? otherValue1 is null || otherValue2 is null || otherValue3 is null || otherValue4 is null
                : value.Equals(otherValue1) || value.Equals(otherValue2) || value.Equals(otherValue3) || value.Equals(otherValue4);

        /// <summary>
        /// ある値が別の配列の要素の何れかに等しいかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">値の型です。</typeparam>
        /// <param name="value">調べる値です。</param>
        /// <param name="otherValues">比較する値の配列です。</param>
        /// <returns><paramref name="value"/> が <paramref name="otherValues"/> の要素の何れかと等しい場合は true、そうではないのなら false です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAnyOf<VALUE_T>(this VALUE_T value, params VALUE_T[] otherValues)
        {
            if (otherValues is null)
                throw new ArgumentNullException(nameof(otherValues));
            if (value is null)
            {
                for (var index = 0; index < otherValues.Length; ++index)
                {
                    if (otherValues[index] is null)
                        return true;
                }
            }
            else
            {
                for (var index = 0; index < otherValues.Length; ++index)
                {
                    if (value.Equals(otherValues[index]))
                        return true;
                }
            }

            return false;
        }

        #endregion

        #region IsNoneOf

        /// <summary>
        /// ある値が別の 2 つの値の何れとも等しくないかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">値の型です。</typeparam>
        /// <param name="value">調べる値です。</param>
        /// <param name="otherValue1">比較する最初の値です。</param>
        /// <param name="otherValue2">比較する 2 番目の値です。</param>
        /// <returns><paramref name="value"/> が <paramref name="otherValue1"/> および <paramref name="otherValue2"/> の何れかとも等しくないなら true、そうではないのなら false です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2)
            => !value.IsAnyOf(otherValue1, otherValue2);

        /// <summary>
        /// ある値が別の 3 つの値の何れとも等しくないかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">値の型です。</typeparam>
        /// <param name="value">調べる値です。</param>
        /// <param name="otherValue1">比較する最初の値です。</param>
        /// <param name="otherValue2">比較する 2 番目の値です。</param>
        /// <param name="otherValue3">比較する 3 番目の値です。</param>
        /// <returns><paramref name="value"/> が <paramref name="otherValue1"/> および <paramref name="otherValue2"/>、<paramref name="otherValue3"/> の何れかとも等しくないなら true、そうではないのなら false です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3)
            => !value.IsAnyOf(otherValue1, otherValue2, otherValue3);

        /// <summary>
        /// ある値が別の 4 つの値の何れとも等しくないかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">値の型です。</typeparam>
        /// <param name="value">調べる値です。</param>
        /// <param name="otherValue1">比較する最初の値です。</param>
        /// <param name="otherValue2">比較する 2 番目の値です。</param>
        /// <param name="otherValue3">比較する 3 番目の値です。</param>
        /// <param name="otherValue4">比較する 4 番目の値です。</param>
        /// <returns><paramref name="value"/> が <paramref name="otherValue1"/> および <paramref name="otherValue2"/>、<paramref name="otherValue3"/>>、<paramref name="otherValue4"/> の何れかとも等しくないなら true、そうではないのなら false です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNoneOf<VALUE_T>(this VALUE_T value, VALUE_T otherValue1, VALUE_T otherValue2, VALUE_T otherValue3, VALUE_T otherValue4)
            => !value.IsAnyOf(otherValue1, otherValue2, otherValue3, otherValue4);

        /// <summary>
        /// ある値が別の配列の要素の何れとも等しくないかを調べます。
        /// </summary>
        /// <typeparam name="VALUE_T">値の型です。</typeparam>
        /// <param name="value">調べる値です。</param>
        /// <param name="otherValues">比較する値の配列です。</param>
        /// <returns><paramref name="value"/> が <paramref name="otherValues"/> の要素の何れとも等しくない場合は true、そうではないのなら false です。</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNoneOf<VALUE_T>(this VALUE_T value, params VALUE_T[] otherValues)
            => !value.IsAnyOf(otherValues);

        #endregion

        #region None

        /// <summary>
        /// 与えられた入力シーケンスが空かどうかを調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// シーケンスの要素の型です。
        /// </typeparam>
        /// <param name="source">
        /// 入力シーケンスです。
        /// </param>
        /// <returns>
        /// 入力シーケンス <paramref name="source"/> が空であれば true、そうではないのなら false です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None<ELEMENT_T>(this IEnumerable<ELEMENT_T> source)
            => !source.Any();

        /// <summary>
        /// 与えられた条件を満たす要素が与えられた入力シーケンスに存在しないかどうかを調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// シーケンスの要素の型です。
        /// </typeparam>
        /// <param name="source">
        /// 入力シーケンスです。
        /// </param>
        /// <param name="predicate">
        /// シーケンスの要素から真偽値を導き出すデリゲートです。
        /// </param>
        /// <returns>
        /// 与えられた条件 <paramref name="predicate"/> を満たす要素が入力シーケンス <paramref name="source"/> に存在しないのであれば true、そうではないのなら false です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool None<ELEMENT_T>(this IEnumerable<ELEMENT_T> source, Func<ELEMENT_T, bool> predicate)
            => !source.Any(predicate);

        #endregion

        #region NotAny

        /// <summary>
        /// 与えられた入力シーケンスに与えられた条件を満たさない要素が存在するかどうかを調べます。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// シーケンスの要素の型です。
        /// </typeparam>
        /// <param name="source">
        /// 入力シーケンスです。
        /// </param>
        /// <param name="predicate">
        /// シーケンスの要素から真偽値を導き出すデリゲートです。
        /// </param>
        /// <returns>
        /// 与えられた条件 <paramref name="predicate"/> を満たさない要素が入力シーケンス <paramref name="source"/> に一つでも存在するのなら true、そうではないのなら false です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool NotAny<ELEMENT_T>(this IEnumerable<ELEMENT_T> source, Func<ELEMENT_T, bool> predicate)
            => !source.All(predicate);

        #endregion

        #region SingleOrNone

        /// <summary>
        /// 与えられた入力シーケンスから0個または1個の要素を取得します。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// シーケンスの要素の型です。
        /// </typeparam>
        /// <param name="source">
        /// 入力シーケンスです。
        /// </param>
        /// <returns>
        /// 入力シーケンス <paramref name="source"/> が空である場合は要素の default(<typeparamref name="ELEMENT_T"/>) 既定値が返ります。(例えば要素の型が参照型ならば null です)
        /// 入力シーケンス <paramref name="source"/> に要素が 1 つしかない場合はその要素が返ります。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// 入力シーケンス <paramref name="source"/> に要素が 2 つ以上あります。
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ELEMENT_T? SingleOrNone<ELEMENT_T>(this IEnumerable<ELEMENT_T> source)
        {
            var matchedItems = source.Take(2).ToList();
            return
                matchedItems.Count > 1
                ? throw new ArgumentException($"{nameof(source)} contains multiple elements.")
                : matchedItems.Count > 0
                ? matchedItems.First()
                : default;
        }

        /// <summary>
        /// 与えられた入力シーケンスから与えられた条件を満たす要素を0個または1個取得します。
        /// </summary>
        /// <typeparam name="ELEMENT_T">
        /// シーケンスの要素の型です。
        /// </typeparam>
        /// <param name="source">
        /// 入力シーケンスです。
        /// </param>
        /// <param name="predicate">
        /// 入力シーケンスの要素から真偽値を導き出すデリゲートです。
        /// </param>
        /// <returns>
        /// 入力シーケンス <paramref name="source"/> に条件 <paramref name="predicate"/> を満たす要素が存在しない場合は default(<typeparamref name="ELEMENT_T"/>) 既定値が返ります。(例えば要素の型が参照型ならば null です)
        /// 入力シーケンス <paramref name="source"/> に条件 <paramref name="predicate"/> を満たす要素が 1 つだけ存在する場合はその要素が返ります。
        /// </returns>
        /// <exception cref="ArgumentException">
        /// 入力シーケンス <paramref name="source"/> に条件 <paramref name="predicate"/> を満たす要素が 2 つ以上あります。
        /// </exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ELEMENT_T? SingleOrNone<ELEMENT_T>(this IEnumerable<ELEMENT_T> source, Func<ELEMENT_T, bool> predicate)
        {
            var matchedItems = source.Where(predicate).Take(2).ToList();
            return
                matchedItems.Count > 1
                ? throw new ArgumentException($"More than one element of {nameof(source)} matched the condition of {nameof(predicate)}.")
                : matchedItems.Count > 0
                ? matchedItems.First()
                : default;
        }

        #endregion

        #region AsReadOnlyArray

        /// <summary>
        /// 配列を <see cref="IReadOnlyArray{VALUE_T}"/> オブジェクトに変換します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 配列の要素の型です。
        /// </typeparam>
        /// <param name="sourceArray">
        /// 変換元の配列です。
        /// </param>
        /// <returns>
        /// 変換された <see cref="IReadOnlyArray{VALUE_T}"/> オブジェクトです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyArray<VALUE_T> AsReadOnlyArray<VALUE_T>(this VALUE_T[] sourceArray)
            => new ReadOnlyArray<VALUE_T>(sourceArray);

        #endregion

        #region ToReadOnlyArray

        /// <summary>
        /// シーケンスを <see cref="IReadOnlyArray{VALUE_T}"/> オブジェクトに変換します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// シーケンスの要素の型です。
        /// </typeparam>
        /// <param name="source">
        /// 変換元のシーケンスです。
        /// </param>
        /// <returns>
        /// 変換された <see cref="IReadOnlyArray{VALUE_T}"/> オブジェクトです。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IReadOnlyArray<VALUE_T> ToReadOnlyArray<VALUE_T>(this IEnumerable<VALUE_T> source)
            => new ReadOnlyArray<VALUE_T>(source.ToArray());

        #endregion

        #region AsSpan

        /// <summary>
        /// 配列の指定された開始位置から終わりまでを参照する新たな <see cref="ReadOnlySpan{T}"/> を作成します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 配列要素の型です。
        /// </typeparam>
        /// <param name="array">
        /// 配列である <see cref="IReadOnlyArray{VALUE_T}"/> です。
        /// </param>
        /// <param name="offset">
        /// 配列の開始位置です。
        /// </param>
        /// <returns>
        /// 新たに作成された <see cref="ReadOnlySpan{T}"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<VALUE_T> AsSpan<VALUE_T>(this IReadOnlyArray<VALUE_T> array, int offset)
            => array.AsSpan()[offset..];

        /// <summary>
        /// 配列の指定された開始位置から指定された長さを参照する新たな <see cref="ReadOnlySpan{T}"/> を作成します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 配列要素の型です。
        /// </typeparam>
        /// <param name="array">
        /// 配列である <see cref="IReadOnlyArray{VALUE_T}"/> です。
        /// </param>
        /// <param name="offset">
        /// 新たな <see cref="ReadOnlySpan{T}"/> から参照する配列の開始位置です。
        /// </param>
        /// <param name="length">
        /// 新たな <see cref="ReadOnlySpan{T}"/> から参照する配列の開始位置からの長さです。
        /// </param>
        /// <returns>
        /// 新たに作成された <see cref="ReadOnlySpan{T}"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlySpan<VALUE_T> AsSpan<VALUE_T>(this IReadOnlyArray<VALUE_T> array, int offset, int length)
            => array.AsSpan().Slice(offset, length);

        #endregion

        #region AsMemory

        /// <summary>
        /// 配列の指定された開始位置から終わりまでを参照する新たな <see cref="ReadOnlyMemory{T}"/> を作成します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 配列要素の型です。
        /// </typeparam>
        /// <param name="array">
        /// 配列である <see cref="ReadOnlyMemory{VALUE_T}"/> です。
        /// </param>
        /// <param name="offset">
        /// 配列の開始位置です。
        /// </param>
        /// <returns>
        /// 新たに作成された <see cref="ReadOnlyMemory{T}"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<VALUE_T> AsMemory<VALUE_T>(this IReadOnlyArray<VALUE_T> array, int offset)
            => array.AsMemory()[offset..];

        /// <summary>
        /// 配列の指定された開始位置から指定された長さを参照する新たな <see cref="ReadOnlyMemory{T}"/> を作成します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 配列要素の型です。
        /// </typeparam>
        /// <param name="array">
        /// 配列である <see cref="IReadOnlyArray{VALUE_T}"/> です。
        /// </param>
        /// <param name="offset">
        /// 新たな <see cref="ReadOnlyMemory{T}"/> から参照する配列の開始位置です。
        /// </param>
        /// <param name="length">
        /// 新たな <see cref="ReadOnlyMemory{T}"/> から参照する配列の開始位置からの長さです。
        /// </param>
        /// <returns>
        /// 新たに作成された <see cref="ReadOnlyMemory{T}"/> です。
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ReadOnlyMemory<VALUE_T> AsMemory<VALUE_T>(this IReadOnlyArray<VALUE_T> array, int offset, int length)
            => array.AsMemory().Slice(offset, length);

        #endregion

        #region IndexOf

        /// <summary>
        /// <see cref="ReadOnlySpan{T}"/> から値が一致する要素を検索します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 要素の型です。
        /// </typeparam>
        /// <param name="buffer">
        /// 検索対象の <see cref="ReadOnlySpan{T}"/> です。
        /// </param>
        /// <param name="value">
        /// 検索する値です。
        /// </param>
        /// <returns>
        /// <paramref name="buffer"/> 内に <paramref name="value"/> と一致する要素が見つかった場合は、最初に見つかった位置を示すインデックス番号が返ります。
        /// 一致する要素が見つからなかった場合は負の整数が返ります。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<VALUE_T>(this ReadOnlySpan<VALUE_T> buffer, VALUE_T value)
        {
            for (var index = 0; index < buffer.Length; ++index)
            {
                var bufferValue = buffer[index];
                if (bufferValue is null && value is null)
                    return index;
                if (bufferValue is not null && bufferValue.Equals(value))
                    return index;
            }

            return -1;
        }

        /// <summary>
        /// <see cref="ReadOnlySpan{T}"/> から条件を満たす要素を検索します。
        /// </summary>
        /// <typeparam name="VALUE_T">
        /// 要素の型です。
        /// </typeparam>
        /// <param name="buffer">
        /// 検索対象の <see cref="ReadOnlySpan{T}"/> です。
        /// </param>
        /// <param name="predicate">
        /// 要素から真偽値を導き出すデリゲートです。
        /// </param>
        /// <returns>
        /// <paramref name="buffer"/> 内に <paramref name="predicate"/> を満たす要素が見つかった場合は、最初に見つかった位置を示すインデックス番号が返ります。
        /// 条件を満たす要素が見つからなかった場合は負の整数が返ります。
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int IndexOf<VALUE_T>(this Span<VALUE_T> buffer, Func<VALUE_T, bool> predicate)
        {
            for (var index = 0; index < buffer.Length; ++index)
            {
                if (predicate(buffer[index]))
                    return index;
            }

            return -1;
        }

        #endregion
    }
}
