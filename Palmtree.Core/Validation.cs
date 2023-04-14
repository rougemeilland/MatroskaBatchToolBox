using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Palmtree
{
    /// <summary>
    /// コードを検証するクラスです。
    /// </summary>
    public static class Validation
    {
        /// <summary>
        /// 与えられた条件を検証し、条件が満たされていない場合に与えられたメッセージの例外を発生させます。
        /// </summary>
        /// <param name="condition">
        /// 検証する条件です。
        /// </param>
        /// <param name="conditionText">
        /// 検証する条件を示すテキストです。
        /// </param>
        /// <remarks>
        /// 通常はあってはならない状況 (内部エラーなど) を検証するために使用します。
        /// </remarks>
        /// <exception cref="AssertionException">
        /// 検証条件が満たされませんでした。
        /// </exception>
        [Conditional("DEBUG")]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Assert([DoesNotReturnIf(false)] bool condition, string conditionText)
        {
            if (!condition)
            {
                Debug.Fail(conditionText);
                throw new AssertionException($"Failed to assert.; condition=\"{conditionText}\"");
            }
        }

        /// <summary>
        /// 致命的エラーの例外オブジェクトを取得します。
        /// </summary>
        /// <param name="message">
        /// 例外のメッセージです。
        /// </param>
        /// <remarks>
        /// 通常はあってはならない状況 (内部エラーなど) が発生してプログラムの続行ができない場合に使用します。
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Exception GetFailErrorException(string message)
        {
            Debug.Fail(message);
            return new AssertionException(message);
        }

        /// <summary>
        /// 致命的エラーの例外オブジェクトを取得します。
        /// </summary>
        /// <param name="message">
        /// 例外のメッセージです。
        /// </param>
        /// <param name="innerException">
        /// 内部例外のオブジェクトです。
        /// </param>
        /// <remarks>
        /// 通常はあってはならない状況 (内部エラーなど) が発生してプログラムの続行ができない場合に使用します。
        /// </remarks>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Exception GetFailErrorException(string message, Exception? innerException)
        {
            Debug.Fail(message);
            return new AssertionException(message, innerException);
        }
    }
}
