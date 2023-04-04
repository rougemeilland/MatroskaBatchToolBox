using Palmtree.IO;

namespace Palmtree.Terminal.StringExpansion
{
    internal class ExpansionReader
        : IPrefetchableTextReader
    {
        private readonly string _source;
        private int _currentIndex;

        public ExpansionReader(string source)
        {
            _source = source;
            _currentIndex = 0;
        }

        public char? Read()
            => _currentIndex < _source.Length
                ? _source[_currentIndex++]
                : null;

        public bool StartsWith(char c)
            => _currentIndex < _source.Length && _source[_currentIndex] == c;

        public bool StartsWith(string s)
        {
            if (_currentIndex + s.Length > _source.Length)
                return false;

            for (var count = 0; count < s.Length; ++count)
            {
                if (_source[_currentIndex + count] != s[count])
                    return false;
            }

            return true;
        }
    }
}
