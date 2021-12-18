using System;
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
                BoundNodeKind.GotoStatement => RewriteGotoStatement(node as BoundGotoStatement),
                BoundNodeKind.ConditionalGotoStatement => RewriteConditionalGotoStatement(node as BoundConditionalGotoStatement),
                BoundNodeKind.LabelStatement => RewriteLabelStatement(node as BoundLabelStatement),
                _ => throw new Exception($"Unexpected node kind in {nameof(RewriteStatement)}: {node.Kind}"),
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

            return new BoundIfStatement(condition, node.IsNegated, thenStatement, elseStatement);
        }

        protected virtual BoundStatement RewriteWhileStatement(BoundWhileStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            var body = RewriteStatement(node.Body);

            if (condition == node.Condition && body == node.Body)
                return node;

            return new BoundWhileStatement(condition, node.IsNegated, body);
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

            return new BoundForStatement(node.Variable, start, stop, step, body);
        }

        protected virtual BoundStatement RewriteGotoStatement(BoundGotoStatement node)
        {
            return node;
        }

        protected virtual BoundStatement RewriteConditionalGotoStatement(BoundConditionalGotoStatement node)
        {
            var condition = RewriteExpression(node.Condition);
            if (condition == node.Condition)
                return node;

            return new BoundConditionalGotoStatement(node.Label, condition, node.JumpIfTrue);
        }

        protected virtual BoundStatement RewriteLabelStatement(BoundLabelStatement node)
        {
            return node;
        }

        public virtual BoundExpression RewriteExpression(BoundExpression node)
        {
            return node.Kind switch
            {
                BoundNodeKind.LiteralExpression => RewriteLiteralExpression(node as BoundLiteralExpression),
                BoundNodeKind.VariableExpression => RewriteVariableExpression(node as BoundVariableExpression),
                BoundNodeKind.AssignmentExpression => RewriteAssignmentExpression(node as BoundAssignmentExpression),
                BoundNodeKind.UnaryExpression => RewriteUnaryExpression(node as BoundUnaryExpression),
                BoundNodeKind.BinaryExpression => RewriteBinaryExpresion(node as BoundBinaryExpression),
                BoundNodeKind.ParenthesisedExpression => RewriteParenthesisedExpresion(node as BoundParenthesisedExpression),
                BoundNodeKind.FunctionCallExpression => RewriteFunctionCallExpression(node as BoundFunctionCallExpression),
                BoundNodeKind.TypeCastExpression => RewriteTypeCastExpression(node as BoundTypeCastExpression),
                BoundNodeKind.IndexingExpression => RewriteIndexingExpression(node as BoundIndexingExpression),
                BoundNodeKind.ListExpression => RewriteListExpression(node as BoundListExpression),
                BoundNodeKind.ErrorExpression => RewriteErrorExpression(node as BoundErrorExpression),
                _ => throw new Exception($"Unexpected node kind in {nameof(RewriteExpression)}: {node.Kind}"),
            };
        }

        protected virtual BoundExpression RewriteLiteralExpression(BoundLiteralExpression node)
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

        protected virtual BoundExpression RewriteParenthesisedExpresion(BoundParenthesisedExpression node)
        {
            var expression = RewriteExpression(node.Expression);

            if (expression == node.Expression)
                return node;

            return new BoundParenthesisedExpression(expression);
        }

        protected virtual BoundExpression RewriteFunctionCallExpression(BoundFunctionCallExpression node)
        {
            ImmutableArray<BoundExpression>.Builder builder = null;

            for (int i = 0; i < node.Arguments.Length; i++)
            {
                var old = node.Arguments[i];
                var statement = RewriteExpression(old);

                if (statement != old)
                {
                    if (builder is null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundExpression>(node.Arguments.Length);

                        for (int j = 0; j < i; j++)
                        {
                            builder.Add(node.Arguments[j]);
                        }
                    }
                }

                if (builder is not null)
                    builder.Add(statement);
            }

            if (builder is null)
                return node;

            return new BoundFunctionCallExpression(node.Function, builder.MoveToImmutable());
        }

        protected virtual BoundExpression RewriteTypeCastExpression(BoundTypeCastExpression node)
        {
            var expression = RewriteExpression(node.Expression);

            if (expression == node.Expression)
                return node;

            return new BoundTypeCastExpression(node.Type, expression);
        }

        protected virtual BoundExpression RewriteIndexingExpression(BoundIndexingExpression node)
        {
            var expression = RewriteExpression(node.Index);

            if (expression == node.Index)
                return node;

            return new BoundIndexingExpression(node.List, expression);
        }

        protected virtual BoundExpression RewriteListExpression(BoundListExpression node)
        {
            ImmutableArray<BoundExpression>.Builder builder = null;

            for (int i = 0; i < node.Contents.Length; i++)
            {
                var old = node.Contents[i];
                var statement = RewriteExpression(old);

                if (statement != old)
                {
                    if (builder is null)
                    {
                        builder = ImmutableArray.CreateBuilder<BoundExpression>(node.Contents.Length);

                        for (int j = 0; j < i; j++)
                        {
                            builder.Add(node.Contents[j]);
                        }
                    }
                }

                if (builder is not null)
                    builder.Add(statement);
            }

            if (builder is null)
                return node;

            return new BoundListExpression(builder.MoveToImmutable());
        }

        protected virtual BoundExpression RewriteErrorExpression(BoundErrorExpression node)
        {
            return node;
        }
    }
}