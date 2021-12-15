using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Peach.CodeAnalysis.Binding
{
    internal abstract class BoundNode
    {
        public abstract BoundNodeKind Kind { get; }

        public IEnumerable<BoundNode> GetChildren()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType))
                {
                    var child = (BoundNode)property.GetValue(this);
                    if (child is not null)
                        yield return child;
                }
                else if (typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                {
                    var children = (IEnumerable<BoundNode>)property.GetValue(this);
                    foreach (var child in children)
                    {
                        if (child is not null)
                            yield return child;
                    }
                }
            }
        }

        public IEnumerable<(string name, object value)> GetProperties()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.Name == nameof(Kind) ||
                    property.Name == nameof(BoundBinaryExpression.Op))
                    continue;

                if (typeof(BoundNode).IsAssignableFrom(property.PropertyType) ||
                    typeof(IEnumerable<BoundNode>).IsAssignableFrom(property.PropertyType))
                {
                    continue;
                }

                var value = property.GetValue(this);
                if (value is not null)
                    yield return (property.Name, value);
            }
        }

        public void WriteTo(TextWriter writer)
        {
            PrettyPrint(writer, this);
        }

        private static void PrettyPrint(TextWriter writer, BoundNode node)
        {
            PrettyPrint(writer, node, "", true, writer == Console.Out);
        }

        private static void PrettyPrint(TextWriter writer, BoundNode node, string indent = "", bool isLast = true, bool isToConsole = false)
        {
            var marker = isLast ? "└──" : "├──";

            writer.Write(indent);
            writer.Write(marker);

            if (isToConsole)
                Console.ForegroundColor = GetColour(node);

            WriteNode(writer, node);
            WriteProperties(writer, node, isToConsole);

            if (isToConsole)
                Console.ResetColor();

            writer.WriteLine();

            indent += isLast ? "   " : "│  ";

            var lastChild = node.GetChildren().LastOrDefault();

            foreach (var child in node.GetChildren())
            {
                PrettyPrint(writer, child, indent, child == lastChild, isToConsole);
            }
        }

        private static void WriteProperties(TextWriter writer, BoundNode node, bool isToConsole)
        {
            bool isFirst = true;

            foreach (var (name, value) in node.GetProperties())
            {
                if (isFirst)
                    isFirst = false;
                else
                {
                    if (isToConsole)
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                    writer.Write(",");
                }

                writer.Write(" ");

                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                writer.Write(name);
                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                writer.Write(" = ");
                if (isToConsole)
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                writer.Write(value);
            }
        }

        private static void WriteNode(TextWriter writer, BoundNode node)
        {
            var text = GetText(node);
            writer.Write(text);
            Console.ResetColor();
        }

        private static string GetText(BoundNode node)
        {
            if (node is BoundBinaryExpression b)
                return b.Op.Kind.ToString() + "Expression";

            if (node is BoundUnaryExpression u)
                return u.Op.Kind.ToString() + "Expression";

            return node.Kind.ToString();
        }

        private static ConsoleColor GetColour(BoundNode node)
        {
            if (node is BoundExpression)
                return ConsoleColor.Blue;

            if (node is BoundStatement)
                return ConsoleColor.Cyan;

            return ConsoleColor.Yellow;
        }

        public override string ToString()
        {
            var stringWriter = new StringWriter();
            WriteTo(stringWriter);
            return stringWriter.ToString();
        }
    }
}