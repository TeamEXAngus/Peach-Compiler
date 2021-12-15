﻿using System;
using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Binding
{
    internal abstract class BoundTreeRewriter
    {
        public virtual BoundStatement RewriteStatement(BoundStatement node)
        {
            return node.Kind switch
            {
                BoundNodeKind.BlockStatement => RewriteBlockStatement(node as BoundBlockStatement),
                BoundNodeKind.ExpressionStatement => RewriteExpressionStatement(node as BoundExpressionStatement),
                BoundNodeKind.VariableDeclaration => RewriteVariableDeclaration(node as BoundVariableDeclaration),
                BoundNodeKind.IfStatement => RewriteIfStatement(node as BoundIfStatement),
                BoundNodeKind.WhileStatement => RewriteWhileStatement(node as BoundWhileStatement),
                BoundNodeKind.LoopStatement => RewriteLoopStatement(node as BoundLoopStatement),
                BoundNodeKind.ForStatement => RewriteForStatement(node as BoundForStatement),
                _ => throw new Exception($"Unexpected node kind: {node.Kind}"),
            };
        }

        protected virtual BoundStatement RewriteBlockStatement(BoundBlockStatement node)
        {
            ImmutableArray<BoundStatement>.Builder builder = null;

            for (int i = 0; i < node.Statements.Length; i++)
            {
                var old = node.Statements[i];
                var statement = RewriteStatement(old);

                if (statement != old)
                {
                    if (builder is null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundStatement>(node.Statements.Length);

                        for (int j = 0; j < i; j++)
                        {
                            builder.Add(node.Statements[j]);
                        }
                    }
                }

                if (builder is not null)
                    builder.Add(statement);
            }

            if (builder is null)
                return node;

            return new BoundBlockStatement(builder.MoveToImmutable());
        }

        protected virtual BoundStatement RewriteExpressionStatement(BoundExpressionStatement node)
        {
            return node;
        }

        protected virtual BoundStatement RewriteVariableDeclaration(BoundVariableDeclaration node)
        {
            var initializer = RewriteExpression(node.Initializer);

            if (initializer == node.Initializer)
                return node;

            return new BoundVariableDeclaration(node.Variable, initializer);
        }

        protected virtual BoundStatement RewriteIfStatement(BoundIfStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var thenStatement = RewriteStatement(node.ThenStatment);
            var elseStatement = node.ElseStatement == null ? null : RewriteStatement(node.ElseStatement);

            if (condition == node.Condition && thenStatement == node.ThenStatment && elseStatement == node.ThenStatment)
                return node;

            return new BoundIfStatement(condition, node.Negated, thenStatement, elseStatement);
        }

        protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);

            if (condition == node.Condition && body == node.Body)
                return node;

            return new BoundWhileStatement(condition, node.Negated, body);
        }

        protected virtual BoundStatement RewriteLoopStatement(BoundLoopStatement node)
        {
            var body = RewriteStatement(node.Body);

            if (body == node.Body)
                return node;

            return new BoundLoopStatement(body);
        }

        protected virtual BoundStatement RewriteForStatement(BoundForStatement node)
        {
            var start = RewriteExpression(node.Start);
            var stop = RewriteExpression(node.Stop);
            var step = RewriteExpression(node.Step);
            var body = RewriteStatement(node.Body);

            if (start == node.Start && stop == node.Stop && step == node.Step && body == node.Body)
                return node;

            return new BoundForStatement(node.Variable, start, stop, node.StopVar, step, body);
        }

        public virtual BoundExpression RewriteExpression(BoundExpression node)
        {
            return node.Kind switch
            {
                BoundNodeKind.LiteralExpression => RewriteLiteralExpression(node as BoundLiteralExpresion),
                BoundNodeKind.VariableExpression => RewriteVariableExpression(node as BoundVariableExpression),
                BoundNodeKind.AssignmentExpression => RewriteAssignmentExpression(node as BoundAssignmentExpression),
                BoundNodeKind.UnaryExpression => RewriteUnaryExpression(node as BoundUnaryExpression),
                BoundNodeKind.BinaryExpression => RewriteBinaryExpresion(node as BoundBinaryExpression),
                _ => throw new Exception($"Unexpected node kind: {node.Kind}"),
            };
        }

        protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpresion node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteVariableExpression(BoundVariableExpression node)
        {
            return node;
        }

        protected virtual BoundExpression RewriteAssignmentExpression(BoundAssignmentExpression node)
        {
            var expression = RewriteExpression(node.Expression);

            if (expression == node.Expression)
                return node;

            return new BoundAssignmentExpression(node.Variable, expression);
        }

        protected virtual BoundExpression RewriteUnaryExpression(BoundUnaryExpression node)
        {
            var operand = RewriteExpression(node.Operand);

            if (operand == node.Operand)
                return node;

            return new BoundUnaryExpression(node.Op, operand);
        }

        protected virtual BoundExpression RewriteBinaryExpresion(BoundBinaryExpression node)
        {
            var left = RewriteExpression(node.Left);
            var right = RewriteExpression(node.Right);

            if (left == node.Left && right == node.Right)
                return node;

            return new BoundBinaryExpression(left, node.Op, right);
        }
    }
}