using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using Palmtree.Collections;

namespace Palmtree.IO
{
    /// <summary>
    /// <see cref="TextReader"/> / <see cref="TextWriter"/> 型の文字ストリームをサポートする FIFO バッファのクラスです。
    /// </summary>
    public class FifoTextStream
        : IFifoBufferReader<char>, IFifoBufferWriter<char>
    {
        private class BufferReader
            : TextReader
        {
            private readonly IFifoBufferReader<char> _buffer;
            private bool _isDisposed;

            public BufferReader(IFifoBufferReader<char> buffer)
            {
                _buffer = buffer;
                _isDisposed = false;
                _buffer.Reference();
            }

            public override int Peek()
                => !_isDisposed
                    ? _buffer.Peek(out var c) ? c : -1
                    : throw new ObjectDisposedException(GetType().FullName);

            public override int Read()
                => !_isDisposed
                    ? _buffer.Read(out var c) ? c : -1
                : throw new ObjectDisposedException(GetType().FullName);

            public override int Read(char[] buffer, int index, int count)
                => !_isDisposed
                    ? _buffer.Read(buffer.AsSpan(index, count))
                    : throw new ObjectDisposedException(GetType().FullName);

            protected override void Dispose(bool disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                        _buffer.Unreference();
                    }

                    _isDisposed = true;
                    base.Dispose(disposing);
                }
            }
        }

        private class BufferWriter
            : TextWriter
        {
            private readonly IFifoBufferWriter<char> _buffer;
            private bool _isDisposed;

            public BufferWriter(IFifoBufferWriter<char> buffer)
            {
                _buffer = buffer;
                _isDisposed = false;
                _buffer.Reference();
            }

            public override Encoding Encoding => Encoding.UTF8;

            public override void Write(char[] buffer, int index, int count)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                var bufferSpan = buffer.AsSpan(index, count);
                while (!bufferSpan.IsEmpty)
                {
                    var length = _buffer.Write(bufferSpan);
                    bufferSpan = bufferSpan[length..];
                }
            }

            public override void Write(char value)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                Span<char> buffer = stackalloc char[1];
                buffer[0] = value;
                _ = _buffer.Write(buffer);
            }

            protected override void Dispose(bool disposing)
            {
                if (!_isDisposed)
                {
                    if (disposing)
                    {
                        _buffer.Close();
                        _buffer.Unreference();
                    }

                    _isDisposed = true;
                    base.Dispose(disposing);
                }
            }
        }

        private readonly ConcurrentFifoBuffer<char> _buffer;
        private int _referenceCount;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="bufferSize">
        /// FIFO バッファのサイズです。
        /// </param>
        public FifoTextStream(int bufferSize = 64 * 1024)
        {
            _buffer = new ConcurrentFifoBuffer<char>(bufferSize);
            _referenceCount = 0;
        }

        /// <summary>
        /// 入力専用文字ストリームを開きます。
        /// </summary>
        /// <returns>
        /// FIFO バッファに対する入力専用文字ストリームである <see cref="TextReader"/> オブジェクトです。
        /// </returns>
        public TextReader OpenReader() => new BufferReader(this);

        /// <summary>
        /// 出力専用文字ストリームを開きます。
        /// </summary>
        /// <returns>
        /// FIFO バッファに対する出力専用文字ストリームである <see cref="TextWriter"/> オブジェクトです。
        /// </returns>
        public TextWriter OpenWriter() => new BufferWriter(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reference() => _ = Interlocked.Increment(ref _referenceCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Unreference()
        {
            if (Interlocked.Decrement(ref _referenceCount) <= 0)
                _buffer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IFifoBufferReader<char>.Peek(out char value) => _buffer.Peek(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IFifoBufferReader<char>.Read(out char value) => _buffer.Dequeue(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IFifoBufferReader<char>.Read(Span<char> buffer) => _buffer.Dequeue(buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferReader<char>.Reference() => Reference();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferReader<char>.Unreference() => Unreference();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferWriter<char>.Write(char value) => _buffer.Enqueue(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IFifoBufferWriter<char>.Write(ReadOnlySpan<char> buffer) => _buffer.Enqueue(buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferWriter<char>.Close() => _buffer.Close();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferWriter<char>.Reference() => Reference();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferWriter<char>.Unreference() => Unreference();
    }
}
