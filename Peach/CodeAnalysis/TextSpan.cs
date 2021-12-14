using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Peach.CodeAnalysis
{
    public struct TextSpan
    {
        public TextSpan(int start, int length)
        {
            Start = start;
            Length = length;
        }

        public int Start { get; }
        public int Length { get; }
        public int End => Start + Length;

        internal static TextSpan FromBounds(int start, int end)
        {
            var length = end - start;
            return new TextSpan(start, end);
        }
    }

    public sealed class VariableSymbol
    {
        internal VariableSymbol(string name, Type type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; }
        public Type Type { get; }
    }
}