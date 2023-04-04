using System;
using System.IO;
using System.Text;
using Palmtree.Collections;

namespace Palmtree
{
    internal class ConsoleTextReader
        : TextReader
    {
        private readonly TextReader _baseReader;
        private readonly ConcurrentFifoBuffer<char> _cache;
        private bool _isDisposed;

        public ConsoleTextReader(TextReader baseReader)
        {
            _baseReader = baseReader;
            _cache = new ConcurrentFifoBuffer<char>(256);
            _isDisposed = false;
        }

        public ConsoleTextReader(Stream baseStream, Encoding inputEncoding)
            : this(
                  baseStream.CanRead
                  ? new StreamReader(baseStream, inputEncoding)
                  : throw new ArgumentException($"\"{nameof(baseStream)}\" must be {nameof(Stream)} value that can be input."))
        {
        }

        public override int Peek()
        {
            lock (_cache)
            {
                if (_cache.Peek(out var c))
                    return c;
            }

            return _baseReader.Peek();
        }

        public override int Read()
        {
            lock (_cache)
            {
                lock (_cache)
                {
                    if (_cache.Peek(out var c))
                    {
                        Validation.Assert(_cache.Dequeue(out c), "_cache.Dequeue(out c)");
                        return c;
                    }
                }
            }

            return _baseReader.Read();
        }

        public override int Read(char[] buffer, int index, int count)
        {
            lock (_cache)
            {
                if (_cache.Peek(out var c))
                    return _cache.Dequeue(buffer.AsSpan(index, count));
            }

            return _baseReader.Read(buffer, index, count);
        }

        public void AppendToCache(char data) => _cache.Enqueue(data);

        public void AppendToCache(Span<char> data)
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
                    _baseReader.Dispose();
                    _cache.Dispose();
                }

                _isDisposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
