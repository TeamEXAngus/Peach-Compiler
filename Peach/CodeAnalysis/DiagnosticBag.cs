using Peach.CodeAnalysis.Symbols;
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

        internal void ReportInvalidNumber(TextSpan span, string text, TypeSymbol type)
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

        internal void ReportUnterminatedString(TextSpan span)
        {
            var message = GetUnterminatedStringErrorMesage();
            Report(span, message);
        }

        internal static string GetUnterminatedStringErrorMesage()
        {
            return "Unterminated string literal in input";
        }

        internal void ReportUnexpectedToken(TextSpan span, SyntaxKind kind, SyntaxKind expected)
        {
            string message = GetUnexpectedTokenErrorMessage(kind, expected);
            Report(span, message);
        }

        internal static string GetUnexpectedTokenErrorMessage(SyntaxKind kind, SyntaxKind expected)
        {
            return $"Unexpected token <{kind}>, expected <{expected}>";
        }

        internal void ReportUndefinedUnaryOperator(TextSpan span, string text, TypeSymbol type)
        {
            string message = GetUndefinedUnaryOperatorErrorMessage(text, type);
            Report(span, message);
        }

        internal static string GetUndefinedUnaryOperatorErrorMessage(string text, TypeSymbol type)
        {
            return $"Unary operator '{text}' is not defined for type {type}";
        }

        internal void ReportUndefinedBinaryOperator(TextSpan span, string text, TypeSymbol left, TypeSymbol right)
        {
            string message = GetUndefinedBinaryOperatorErrorMessage(text, left, right);
            Report(span, message);
        }

        internal static string GetUndefinedBinaryOperatorErrorMessage(string text, TypeSymbol left, TypeSymbol right)
        {
            return $"Binary operator '{text}' is not defined for types {left} and {right}";
        }

        internal void ReportUndefinedName(TextSpan span, string name)
        {
            string message = GetUndefinedNameErrorMessage(name);
            Report(span, message);
        }

        internal static string GetUndefinedNameErrorMessage(string name)
        {
            return $"Variable '{name}' is undefined";
        }

        internal void ReportVariableAlreadyDeclared(TextSpan span, string name)
        {
            string message = GetVariableAlreadyDeclaredErrorMessage(name);
            Report(span, message);
        }

        internal static string GetVariableAlreadyDeclaredErrorMessage(string name)
        {
            return $"Cannot redeclare variable '{name}'";
        }

        internal void ReportCannotConvertTypes(TextSpan span, TypeSymbol varType, TypeSymbol expressionType)
        {
            string message = GetCannotConvertTypesErrorMessage(varType, expressionType);
            Report(span, message);
        }

        internal static string GetCannotConvertTypesErrorMessage(TypeSymbol varType, TypeSymbol expressionType)
        {
            return $"Cannot convert type {varType} to type {expressionType}";
        }

        internal void ReportCannotAssignToConst(TextSpan span, string name)
        {
            string message = GetCannotAssignToConstErrorMessage(name);
            Report(span, message);
        }

        internal static string GetCannotAssignToConstErrorMessage(string name)
        {
            return $"Cannot assign to constant variable '{name}'";
        }

        internal void ReportUndefinedFunction(TextSpan span, string name)
        {
            string message = GetUndefinedFunctionErrorMessage(name);
            Report(span, message);
        }

        internal static string GetUndefinedFunctionErrorMessage(string name)
        {
            return $"The function '{name}' does not exist";
        }

        internal void ReportWrongNumberOfArguments(TextSpan span, int count, int expected)
        {
            string message = GetWrongNumberOfArgumentsErrorMessage(count, expected);
            Report(span, message);
        }

        internal static string GetWrongNumberOfArgumentsErrorMessage(int count, int expected)
        {
            return $"Incorrect number of arguments. Expected: '{expected}' Actual: '{count}'";
        }

        internal void ReportIncorrectArgumentType(TextSpan span, TypeSymbol type, TypeSymbol expected)
        {
            string message = GetIncorrectArgumentTypeErrorMessage(type, expected);
            Report(span, message);
        }

        internal static string GetIncorrectArgumentTypeErrorMessage(TypeSymbol type, TypeSymbol expected)
        {
            return $"Incorrect argument type for function. Expected: '{expected}' Actual: '{type}'";
        }
    }
}