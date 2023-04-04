using System.Diagnostics.CodeAnalysis;

namespace Palmtree.Terminal.StringExpansion
{
    internal interface IArgumentIndexer<INDEX_T, VALUE_T>
        : IIndexer<INDEX_T, VALUE_T>
    {
        bool TryGet(INDEX_T index, [MaybeNullWhen(false)] out VALUE_T value);
    }
}
