using Peach.CodeAnalysis.Syntax;
using Peach.CodeAnalysis.Text;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Peach.CodeAnalysis
{
    internal sealed class DiagnosticBag : IEnumerable<Diagnostic>
    {
        private readonly List<Diagnostic> _diagnostics = new();

        public IEnumerator<Diagnostic> GetEnumerator() => _diagnostics.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _diagnostics.GetEnumerator();

        internal void AddRange(DiagnosticBag diagnostics)
        {
            _diagnostics.AddRange(diagnostics._diagnostics);
        }

        private void Report(TextSpan span, string message)
        {
            var diagnostic = new Diagnostic(span, message);
            _diagnostics.Add(diagnostic);
        }

        internal void ReportInvalidNumber(TextSpan span, string text, Type type)
        {
            var message = $"The number '{text}' isn't valid {type}";
            Report(span, message);
        }

        internal void ReportBadCharacter(int position, char current)
        {
            var span = new TextSpan(position, 1);
            var message = $"Bad character input: '{current}'";
            Report(span, message);
        }

        internal void ReportUnexpectedToken(TextSpan span, SyntaxKind kind, SyntaxKind expected)
        {
            var message = $"Unexpected token <{kind}>, expected <{expected}>";
            Report(span, message);
        }

        internal void ReportUndefinedUnaryOperator(TextSpan span, string text, Type type)
        {
            var message = $"Unary operator '{text}' is not defined for type {type}";
            Report(span, message);
        }

        internal void ReportUndefinedBinaryOperator(TextSpan span, string text, Type left, Type right)
        {
            var message = $"Binary operator '{text}' is not defined for types {left} and {right}";
            Report(span, message);
        }

        internal void ReportUndefinedName(TextSpan span, string name)
        {
            var message = $"Variable '{name}' is undefined";
            Report(span, message);
        }

        internal void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            var message = $"Cannot redeclare variable '{name}'";
            Report(span, message);
        }

        internal void ReportCannotConvert(TextSpan span, Type varType, Type expressionType)
        {
            var message = $"Cannot convert type {varType} to type {expressionType}";
            Report(span, message);
        }
    }
}