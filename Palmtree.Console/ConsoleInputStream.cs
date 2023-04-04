using System;
using System.IO;
using Palmtree.Collections;

namespace Palmtree
{
    internal class ConsoleInputStream
        : Stream
    {
        private readonly Stream _baseStream;
        private readonly ConcurrentFifoBuffer<byte> _cache;
        private bool _isDisposed;

        public ConsoleInputStream(Stream baseStream)
        {
            if (!baseStream.CanRead)
                throw new ArgumentException($"\"{nameof(baseStream)}\" must be {nameof(Stream)} value that can be input.");

            _baseStream = baseStream;
            _cache = new ConcurrentFifoBuffer<byte>(256);
            _isDisposed = false;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            lock (_cache)
            {
                if (_cache.Peek(out var data))
                {
                    return _cache.Dequeue(new Span<byte>(buffer, offset, count));
                }
            }

            return _baseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        public void AppendToCache(byte data) => _cache.Enqueue(data);

        public void AppendToCache(Span<byte> data)
        {
            while (!data.IsEmpty)
            {
                var length = _cache.Enqueue(data);
                data = data[length..];
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!_isDisposed)
            {
                if (disposing)
                {
                    _baseStream.Dispose();
                    _cache.Dispose();
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
