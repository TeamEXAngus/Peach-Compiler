using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Peach.CodeAnalysis.Syntax
{
    public sealed class SeparatedSyntaxList<T> : IEnumerable<T>
        where T : SyntaxNode
    {
        public SeparatedSyntaxList(ImmutableArray<SyntaxNode> nodesAndSeparators)
        {
            NodesAndSeparators = nodesAndSeparators;
        }

        public ImmutableArray<SyntaxNode> NodesAndSeparators { get; }

        public int Count => (NodesAndSeparators.Length + 1) / 2;

        public T this[int index] => NodesAndSeparators[index * 2] as T;

        public SyntaxToken GetSeparator(int index) => NodesAndSeparators.ElementAtOrDefault(index * 2 + 1) as SyntaxToken;

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}