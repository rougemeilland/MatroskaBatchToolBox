using System.Text;

namespace Palmtree
{
    internal static class ConsoleEncodingExtensions
    {
        public static Encoding RemovePreamble(this Encoding encoding)
            => new ConsoleEncoding(encoding);
    }
}
