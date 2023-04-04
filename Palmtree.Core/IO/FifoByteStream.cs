using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Palmtree.Collections;

namespace Palmtree.IO
{
    /// <summary>
    /// <see cref="Stream"/>型のバイトストリームをサポートする FIFO バッファのクラスです。
    /// </summary>
    public class FifoByteStream
        : IFifoBufferReader<byte>, IFifoBufferWriter<byte>
    {
        private class InputStream
            : Stream
        {
            private readonly IFifoBufferReader<byte> _buffer;
            private bool _isDisposed;

            public InputStream(IFifoBufferReader<byte> buffer)
            {
                _buffer = buffer;
                _isDisposed = false;
                _buffer.Reference();
            }

            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public override void Flush()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);
            }

            public override int Read(byte[] buffer, int offset, int count)
                => !_isDisposed
                ? _buffer.Read(buffer.AsSpan(offset, count))
                : throw new ObjectDisposedException(GetType().FullName);

            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

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

        private class OutputStream
            : Stream
        {
            private readonly IFifoBufferWriter<byte> _buffer;
            private bool _isDisposed;

            public OutputStream(IFifoBufferWriter<byte> buffer)
            {
                _buffer = buffer;
                _isDisposed = false;
                _buffer.Reference();
            }

            public override bool CanRead => false;

            public override bool CanSeek => false;

            public override bool CanWrite => true;

            public override long Length => throw new NotSupportedException();

            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

            public override void Flush()
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);
            }

            public override int Read(byte[] buffer, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

            public override void SetLength(long value)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                if (_isDisposed)
                    throw new ObjectDisposedException(GetType().FullName);

                var bufferSpan = buffer.AsSpan(offset, count);
                while (!bufferSpan.IsEmpty)
                {
                    var length = _buffer.Write(bufferSpan);
                    bufferSpan = bufferSpan[length..];
                }
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

        private readonly ConcurrentFifoBuffer<byte> _buffer;
        private int _referenceCount;

        /// <summary>
        /// コンストラクタです。
        /// </summary>
        /// <param name="bufferSize">
        /// FIFO バッファのサイズです。
        /// </param>
        public FifoByteStream(int bufferSize)
        {
            _buffer = new ConcurrentFifoBuffer<byte>(bufferSize);
            _referenceCount = 0;
        }

        /// <summary>
        /// 入力専用バイトストリームを開きます。
        /// </summary>
        /// <returns>
        /// FIFO バッファに対する入力専用バイトストリームである <see cref="Stream"/> オブジェクトです。
        /// </returns>
        public Stream OpenInputStream() => new InputStream(this);

        /// <summary>
        /// 出力専用バイトストリームを開きます。
        /// </summary>
        /// <returns>
        /// FIFO バッファに対する出力専用バイトストリームである <see cref="Stream"/> オブジェクトです。
        /// </returns>
        public Stream OpenOutputStream() => new OutputStream(this);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Reference() => _ = Interlocked.Increment(ref _referenceCount);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Unreference()
        {
            if (Interlocked.Decrement(ref _referenceCount) <= 0)
                _buffer.Dispose();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IFifoBufferReader<byte>.Peek(out byte value) => _buffer.Peek(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        bool IFifoBufferReader<byte>.Read(out byte value) => _buffer.Dequeue(out value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IFifoBufferReader<byte>.Read(Span<byte> buffer) => _buffer.Dequeue(buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferReader<byte>.Reference() => Reference();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferReader<byte>.Unreference() => Unreference();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferWriter<byte>.Write(byte data) => _buffer.Enqueue(data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        int IFifoBufferWriter<byte>.Write(ReadOnlySpan<byte> buffer) => _buffer.Enqueue(buffer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferWriter<byte>.Close() => _buffer.Close();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferWriter<byte>.Reference() => Reference();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void IFifoBufferWriter<byte>.Unreference() => Unreference();
    }
}
