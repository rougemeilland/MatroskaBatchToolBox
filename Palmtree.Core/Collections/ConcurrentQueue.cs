using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Palmtree.Collections
{
    /// <summary>
    /// スレッドセーフな FIFO キューです。
    /// </summary>
    /// <typeparam name="ELEMENT_T">
    /// 要素の型です。
    /// </typeparam>
    public class ConcurrentQueue<ELEMENT_T>
        : IDisposable
    {
        private readonly Queue<ELEMENT_T> _queue;
        private readonly ManualResetEventSlim _notZeroEvent;
        private bool _isDisposed;
        private bool _closed;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        public ConcurrentQueue()
        {
            _queue = new Queue<ELEMENT_T>();
            _notZeroEvent = new ManualResetEventSlim(false);
            _isDisposed = false;
            _closed = false;
        }

        /// <summary>
        /// 状態を変更することなく FIFO キューの要素を取り出します。
        /// </summary>
        /// <param name="value">
        /// 取り出した要素です。
        /// </param>
        /// <returns>
        /// FIFO キューから要素を取り出せた場合、true が返ります。そうではない場合は false が返ります。
        /// </returns>
        public bool Peek([NotNullWhen(true)] out ELEMENT_T? value)
        {
            while (true)
            {
                if (!_notZeroEvent.Wait(0))
                {
                    value = default;
                    return false;
                }

                lock (this)
                {
                    try
                    {
                        if (_queue.Count <= 0)
                        {
                            value = default;
                            return false;
                        }

                        var data = _queue.Peek();
                        Validation.Assert(data is not null, "_queue.Peek() is not null");
                        value = data;
                        return true;
                    }
                    finally
                    {
                        RefreshState();
                    }
                }
            }
        }

        /// <summary>
        /// FIFO キューから要素を取り出します。
        /// </summary>
        /// <param name="value">
        /// 取り出した要素です。
        /// </param>
        /// <returns>
        /// FIFO キューから要素を取り出せた場合、true が返ります。
        /// FIFO キューが終了している場合は false が返ります。
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>FIFO キューが空でありかつ FIFO キューが終了していない場合は、<see cref="Enqueue(ELEMENT_T)"/> によって要素が追加されるまでブロックします。</item>
        /// <item>FIFO キューが空でありかつ FIFO キューが終了している場合は、常に false が返ります。</item>
        /// </list>
        /// </remarks>
        public bool Dequeue([NotNullWhen(true)] out ELEMENT_T? value)
        {
            while (true)
            {
                _notZeroEvent.Wait();
                lock (this)
                {
                    try
                    {
                        if (_queue.Count > 0)
                        {
                            var data = _queue.Dequeue();
                            Validation.Assert(data is not null, "_queue.Dequeue() is not null");
                            value = data;
                            return true;
                        }
                        else
                        {
                            if (_closed)
                            {
                                value = default;
                                return false;
                            }
                        }
                    }
                    finally
                    {
                        RefreshState();
                    }
                }
            }
        }

        /// <summary>
        /// FIFO キューに要素を追加します。
        /// </summary>
        /// <param name="data">
        /// 追加する要素です。
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="data"/> が null です。
        /// </exception>
        /// <exception cref="ObjectDisposedException">
        /// 既に FIFO キューの終了が宣言されています。
        /// </exception>
        public void Enqueue(ELEMENT_T data)
        {
            if (data is null)
                throw new ArgumentNullException(nameof(data));

            lock (this)
            {
                if (_closed)
                    throw new ObjectDisposedException(GetType().FullName);
                try
                {
                    _queue.Enqueue(data);
                }
                finally
                {
                    RefreshState();
                }
            }
        }

        /// <summary>
        /// FIFO キューの終了を宣言します。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>このメソッドを呼び出した後に <see cref="Enqueue(ELEMENT_T)"/> を呼び出すことはできません。 </item>
        /// </list>
        /// </remarks>
        public void Close()
        {
            lock (this)
            {
                _closed = true;
                RefreshState();
            }
        }

        /// <summary>
        /// このオブジェクトに関連付けられたリソースを解放します。
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// このオブジェクトに関連付けられたリソースを解放します。
        /// </summary>
        /// <param name="disposing"><see cref="Dispose()"/>から呼び出されたかどうかの<see cref="bool"/>値です。 </param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                    _notZeroEvent.Dispose();

                _isDisposed = true;
            }
        }

        private void RefreshState()
        {
            if (_queue.Count > 0 || _closed)
                _notZeroEvent.Set();
            else
                _notZeroEvent.Reset();
        }
    }
}
