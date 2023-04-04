using System;
using System.Text;

namespace Palmtree
{
    internal class ConsoleEncoding
        : Encoding
    {
        private readonly Encoding _sourceEncoding;

        public ConsoleEncoding(Encoding sourceEncoding)
            : base(sourceEncoding.CodePage)
            => _sourceEncoding = sourceEncoding;

        public override byte[] GetPreamble()
            => Array.Empty<byte>();

        public override bool IsSingleByte
            => _sourceEncoding.IsSingleByte;

        public override object Clone()
            => new ConsoleEncoding(_sourceEncoding);

        public override int GetByteCount(char[] chars, int index, int count)
            => _sourceEncoding.GetByteCount(chars, index, count);

        public override int GetBytes(char[] chars, int charIndex, int charCount, byte[] bytes, int byteIndex)
            => _sourceEncoding.GetBytes(chars, charIndex, charCount, bytes, byteIndex);

        public override int GetCharCount(byte[] bytes, int index, int count)
            => _sourceEncoding.GetCharCount(bytes, index, count);

        public override int GetChars(byte[] bytes, int byteIndex, int byteCount, char[] chars, int charIndex)
            => _sourceEncoding.GetChars(bytes, byteIndex, byteCount, chars, charIndex);

        public override int GetMaxByteCount(int charCount)
            => _sourceEncoding.GetMaxByteCount(charCount);

        public override int GetMaxCharCount(int byteCount)
            => _sourceEncoding.GetMaxCharCount(byteCount);
    }
}
