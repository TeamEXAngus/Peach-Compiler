﻿using Peach.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Symbols
{
    public sealed class FunctionSymbol : Symbol
    {
        public FunctionSymbol(string name, ImmutableArray<SyntaxKind>? modifiers, ImmutableArray<ParameterSymbol> parameters, TypeSymbol type, FunctionDeclarationSyntax declaration = null)
            : base(name)
        {
            Modifiers = modifiers ?? ImmutableArray<SyntaxKind>.Empty;
            Parameters = parameters;
            Type = type;
            Declaration = declaration;
        }

        public override SymbolKind Kind => SymbolKind.Function;

        public ImmutableArray<SyntaxKind> Modifiers { get; }
        public ImmutableArray<ParameterSymbol> Parameters { get; }
        public TypeSymbol Type { get; }
        public FunctionDeclarationSyntax Declaration { get; }
    }
}