using System;
using System.Threading;

namespace Palmtree.Collections
{
    /// <summary>
    /// スレッドセーフな FIFO バッファのクラスです。
    /// </summary>
    /// <typeparam name="ELEMENT_T">
    /// FIFO バッファの要素の型です。
    /// </typeparam>
    public class ConcurrentFifoBuffer<ELEMENT_T>
        : IDisposable
        where ELEMENT_T : struct
    {
        private readonly ELEMENT_T[] _buffer;
        private readonly ManualResetEventSlim _notZeroEvent;
        private readonly ManualResetEventSlim _notFullEvent;
        private bool _isDisposed;
        private int _currentOffset;
        private int _currentLength;
        private bool _closed;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="bufferSize">
        /// FIFO バッファのサイズです。
        /// </param>
        public ConcurrentFifoBuffer(int bufferSize = 64 * 1024)
        {
            _buffer = new ELEMENT_T[bufferSize];
            _notZeroEvent = new ManualResetEventSlim(false);
            _notFullEvent = new ManualResetEventSlim(true);
            _isDisposed = false;
            _currentOffset = 0;
            _currentLength = 0;
            _closed = false;
        }

        /// <summary>
        /// FIFO バッファの状態を変えることなく、FIFO バッファから要素を1つだけ取り出します。
        /// </summary>
        /// <param name="value">
        /// 取り出した要素です。
        /// </param>
        /// <returns>
        /// FIFO バッファが空ではない場合は、取り出した要素が <paramref name="value"/> に格納され、true が返ります。
        /// そうではない場合は false が返ります。
        /// </returns>
        public bool Peek(out ELEMENT_T value)
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
                        if (_currentLength <= 0)
                        {
                            value = default;
                            return false;
                        }

                        Validation.Assert(_currentOffset < _buffer.Length, "_currentOffset < _buffer.Length");
                        value = _buffer[_currentOffset];
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
        /// FIFO バッファから要素を1つだけ取り出します。
        /// </summary>
        /// <param name="value">
        /// 取り出した要素です。
        /// </param>
        /// <returns>
        /// FIFO バッファから要素を取り出せた場合はその要素が<paramref name="value"/>に格納され、true が返ります。
        /// FIFO バッファの終わりに達した場合は false が返ります。
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>FIFO バッファが空ではなかった場合、このメソッドは一番最初に追加された要素を取り出して復帰します。</item>
        /// <item>FIFO バッファが空だった場合、<see cref="Enqueue(ELEMENT_T)"/> または <see cref="Enqueue(ReadOnlySpan{ELEMENT_T})"/> によって FIFO バッファに要素が追加されるまでブロックした後、前述と同様に要素を取り出して復帰します。</item>
        /// <item>FIFO バッファが空でありかつ <see cref="Close()"/> によって FIFO バッファの終了が宣言されていた場合、ブロックせずに復帰します。</item>
        /// </list>
        /// </remarks>
        public bool Dequeue(out ELEMENT_T value)
        {
            while (true)
            {
                _notZeroEvent.Wait();
                lock (this)
                {
                    try
                    {
                        if (_currentLength > 0)
                        {
                            Validation.Assert(_currentOffset < _buffer.Length, "_currentOffset < _buffer.Length");
                            value = _buffer[_currentOffset];
                            ++_currentOffset;
                            --_currentLength;
                            Validation.Assert(_currentOffset <= _buffer.Length, "_currentOffset <= _buffer.Length");
                            if (_currentOffset == _buffer.Length)
                                _currentLength = 0;
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
        /// FIFO バッファから要素を取り出します。
        /// </summary>
        /// <param name="buffer">
        /// 取り出した要素が格納されるバッファです。
        /// </param>
        /// <returns>
        /// FIFO バッファから要素を取り出せた場合はその要素が <paramref name="buffer"/> に格納され、取り出せた要素の数が返ります。
        /// FIFO バッファの終わりに達した場合は 0 が返ります。
        /// </returns>
        /// <remarks>
        /// <list type="bullet">
        /// <item>FIFO バッファが空だった場合、<see cref="Enqueue(ELEMENT_T)"/> または <see cref="Enqueue(ReadOnlySpan{ELEMENT_T})"/> によって FIFO バッファに要素が追加されるまでブロックします。</item>
        /// </list>
        /// </remarks>
        public int Dequeue(Span<ELEMENT_T> buffer)
        {
            if (buffer.Length <= 0)
                throw new ArgumentException($"Insufficient space in the buffer to store the value.: {nameof(buffer)}.{nameof(buffer.Length)}={nameof(buffer.Length)}", nameof(buffer));

            while (true)
            {
                _notZeroEvent.Wait();
                lock (this)
                {
                    if (_closed)
                        return 0;
                    try
                    {
                        if (_currentLength > 0)
                        {
                            Validation.Assert(_currentOffset < _buffer.Length, "_currentOffset < _buffer.Length");
                            var length = _currentLength.Minimum(_buffer.Length - _currentOffset).Minimum(buffer.Length);
                            _buffer.AsSpan().Slice(_currentOffset, length).CopyTo(buffer);
                            _currentOffset += length;
                            _currentLength -= length;
                            Validation.Assert(_currentOffset <= _buffer.Length, "_currentOffset <= _buffer.Length");
                            if (_currentOffset == _buffer.Length)
                                _currentOffset = 0;
                            return length;
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
        /// FIFO バッファに要素を追加します。
        /// </summary>
        /// <param name="data">
        /// FIFOバッファに追加する要素です。
        /// </param>
        /// <exception cref="ObjectDisposedException">
        /// 既に <see cref="Close()"/> が発行されているため、要素の追加が出来ません。
        /// </exception>
        /// <remarks>
        /// <list type="bullet">
        /// <item>FIFO バッファの空きが不足している場合、<see cref="Dequeue(out ELEMENT_T)"/> または <see cref="Dequeue(Span{ELEMENT_T})"/> によって FIFO バッファの空きができるまでブロックします。</item>
        /// </list>
        /// </remarks>
        public void Enqueue(ELEMENT_T data)
        {
            while (true)
            {
                _notFullEvent.Wait();
                lock (this)
                {
                    if (_closed)
                        throw new ObjectDisposedException(GetType().FullName);
                    try
                    {
                        if (_currentLength < _buffer.Length)
                        {
                            Validation.Assert(_currentOffset < _buffer.Length, "_currentOffset < _buffer.Length");
                            var freeSpaceOffset = _currentOffset + _currentLength;
                            if (freeSpaceOffset >= _buffer.Length)
                                freeSpaceOffset -= _buffer.Length;
                            _buffer[freeSpaceOffset] = data;
                            ++_currentLength;
                            return;
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
        /// FIFO バッファに要素を追加します。
        /// </summary>
        /// <param name="buffer">
        /// FIFO バッファに追加する要素の配列を指す <see cref="ReadOnlySpan{T}"/> です。
        /// </param>
        /// <returns>
        /// FIFO バッファに追加できた要素の数です。
        /// </returns>
        /// <exception cref="ObjectDisposedException">
        /// 既に <see cref="Close()"/> が発行されているため、要素の追加が出来ません。
        /// </exception>
        /// <remarks>
        /// <list type="bullet">
        /// <item>このメソッドは最低でも1つの要素を FIFO バッファに追加しますが、<paramref name="buffer"/> で与えられた要素をすべて追加するとは限りません。必ず復帰値を確認してください。</item>
        /// <item>FIFO バッファの空きが不足している場合、<see cref="Dequeue(out ELEMENT_T)"/> または <see cref="Dequeue(Span{ELEMENT_T})"/> によって FIFO バッファの空きができるまでブロックします。</item>
        /// </list>
        /// </remarks>
        public int Enqueue(ReadOnlySpan<ELEMENT_T> buffer)
        {
            while (true)
            {
                _notFullEvent.Wait();
                lock (this)
                {
                    if (_closed)
                        throw new ObjectDisposedException(GetType().FullName);
                    try
                    {
                        if (_currentLength < _buffer.Length)
                        {
                            Validation.Assert(_currentOffset < _buffer.Length, "_currentOffset < _buffer.Length");
                            var freeSpaceOffset = _currentOffset + _currentLength;
                            if (freeSpaceOffset >= _buffer.Length)
                                freeSpaceOffset -= _buffer.Length;
                            var length = (_buffer.Length - _currentLength).Minimum(_buffer.Length - freeSpaceOffset).Minimum(buffer.Length);
                            buffer[..length].CopyTo(_buffer.AsSpan(freeSpaceOffset, length));
                            _currentLength += length;
                            return length;
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
        /// FIFO バッファの終了を宣言します。
        /// </summary>
        /// <remarks>
        /// <list type="bullet">
        /// <item>このメソッドの発行後は、新たに要素の追加をすることはできません。</item>
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
        /// <param name="disposing"><see cref="Dispose()"/>によって呼び出されたかどうかの<see cref="bool"/>値です。</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _notFullEvent.Dispose();
                    _notZeroEvent.Dispose();
                }

                _isDisposed = true;
            }
        }

        private void RefreshState()
        {
            if (_currentLength < _buffer.Length || _closed)
                _notFullEvent.Set();
            else
                _notFullEvent.Reset();

            if (_currentLength > 0 || _closed)
                _notZeroEvent.Set();
            else
                _notZeroEvent.Reset();
        }
    }
}
