using System.IO;

namespace Peach.CodeAnalysis.Symbols
{
    public abstract class Symbol
    {
        protected Symbol(string name)
        {
            Name = name;
        }

        public abstract SymbolKind Kind { get; }
        public string Name { get; }

        public override string ToString()
        {
            using var writer = new StringWriter();
            this.WriteTo(writer);
            return writer.ToString();
        }
    }
}