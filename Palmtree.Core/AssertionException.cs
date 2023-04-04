using System;

namespace Palmtree
{
    /// <summary>
    /// 通常はあってはならない条件 (内部エラーなど) で通知される例外のクラスです。
    /// </summary>
    [Serializable]
    public sealed class AssertionException
        : Exception
    {
        internal AssertionException(string message)
            : base(message)
            => System.Diagnostics.Debug.Fail(message);

        internal AssertionException(string message, Exception inner)
            : base(message, inner)
            => System.Diagnostics.Debug.Fail(message);
    }
}
