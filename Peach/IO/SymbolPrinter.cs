using Peach.IO;
using System;
using System.IO;

namespace Peach.CodeAnalysis.Symbols
{
    internal static class SymbolPrinter
    {
        public static void WriteTo(this Symbol symbol, TextWriter writer)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.GlobalVariable:
                    WriteGlobalVariableTo(symbol as GlobalVariableSymbol, writer);
                    break;

                case SymbolKind.LocalVariable:
                    WriteLocalVariableTo(symbol as LocalVariableSymbol, writer);
                    break;

                case SymbolKind.Parameter:
                    WriteParameter(symbol as ParameterSymbol, writer);
                    break;

                case SymbolKind.Type:
                    WriteTypeTo(symbol as TypeSymbol, writer);
                    break;

                case SymbolKind.Function:
                    WriteFunctionTo(symbol as FunctionSymbol, writer);
                    break;

                default: throw new Exception($"Unknown symbol kind {symbol.Kind} {symbol}");
            }
        }

        private static void WriteGlobalVariableTo(GlobalVariableSymbol symbol, TextWriter writer)
        {
            writer.WriteKeyword("global ");
            if (symbol.IsConst) writer.WriteKeyword("const ");
            writer.WriteIdentifier(symbol.Name);
            writer.WritePunctuation(": ");
            symbol.Type.WriteTo(writer);
        }

        private static void WriteLocalVariableTo(LocalVariableSymbol symbol, TextWriter writer)
        {
            writer.WriteKeyword("local ");
            if (symbol.IsConst) writer.WriteKeyword("const ");
            writer.WriteIdentifier(symbol.Name);
            writer.WritePunctuation(": ");
            symbol.Type.WriteTo(writer);
        }

        private static void WriteParameter(ParameterSymbol symbol, TextWriter writer)
        {
            writer.WriteIdentifier(symbol.Name);
            writer.WritePunctuation(": ");
            symbol.Type.WriteTo(writer);
        }

        private static void WriteTypeTo(TypeSymbol symbol, TextWriter writer)
        {
            foreach (char c in symbol.Name)
                if (c == '[')
                    writer.WritePunctuation("[");
                else if (c == ']')
                    writer.WritePunctuation("]");
                else writer.WriteKeyword(c.ToString());
        }

        private static void WriteFunctionTo(FunctionSymbol symbol, TextWriter writer)
        {
            writer.WriteKeyword("function ");
            writer.WriteIdentifier(symbol.Name);
            writer.WritePunctuation("(");

            bool isFirst = true;
            foreach (var param in symbol.Parameters)
            {
                if (isFirst)
                    isFirst = false;
                else
                    writer.WritePunctuation(", ");

                param.WriteTo(writer);
            }

            writer.WritePunctuation("): ");
            symbol.Type.WriteTo(writer);
            writer.WriteLine();
        }
    }
}