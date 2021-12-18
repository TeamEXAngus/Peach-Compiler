using System.IO;

namespace Peach.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }

        public override string ToString()
        {
            using var stringWriter = new StringWriter();
            this.WriteTo(stringWriter);
            return stringWriter.ToString();
        }
    }
}