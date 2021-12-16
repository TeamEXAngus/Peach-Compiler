using Peach.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Peach.CodeAnalysis.Syntax
{
    public abstract class SyntaxNode
    {
        public abstract SyntaxKind Kind { get; }

        public abstract IEnumerable<SyntaxNode> GetChildren();

        public virtual TextSpan Span
        {
            get
            {
                var first = GetChildren().First().Span;
                var last = GetChildren().Last().Span;
                return TextSpan.FromBounds(first.Start, last.End);
            }
        }

        public void WriteTo(TextWriter writer)
        {
            PrettyPrint(writer, this, isToConsole: writer == Console.Out);
        }

        public SyntaxToken GetLastToken()
        {
            if (this is SyntaxToken token)
                return token;

            return GetChildren().Last().GetLastToken();
        }

        private static void PrettyPrint(TextWriter writer, SyntaxNode node, string indent = "", bool isLast = true, bool isToConsole = true)

        {
            var marker = isLast ? "└──" : "├──";

            if (isToConsole)
                Console.ForegroundColor = ConsoleColor.DarkGray;
            writer.Write(indent);
            writer.Write(marker);

            if (isToConsole)
                Console.ForegroundColor = node is SyntaxToken ? ConsoleColor.Blue : ConsoleColor.Cyan;
            writer.Write(node.Kind);

            if (node is SyntaxToken t)

            {
                if (t.Value is not null)

                {
                    writer.Write(" ");
                    writer.Write(t.Value);
                }

                if (t.Kind == SyntaxKind.IdentifierToken)
                {
                    writer.Write(" ");
                    writer.Write(t.Text);
                }
            }

            writer.WriteLine();

            indent += isLast ? "   " : "│  ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
            {
                PrettyPrint(writer, child, indent, child == lastChild, isToConsole);
            }
        }

        public override string ToString()
        {
            var stringWriter = new StringWriter();
            WriteTo(stringWriter);
            return stringWriter.ToString();
        }
    }
}