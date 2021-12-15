using Peach.CodeAnalysis.Binding;
using Peach.CodeAnalysis.Syntax;
using System.Collections.Immutable;

namespace Peach.CodeAnalysis.Lowering
{
    internal class Lowerer : BoundTreeRewriter
    {
        private Lowerer()
        { }

        public static BoundStatement Lower(BoundStatement statement)
        {
            var lowerer = new Lowerer();
            return lowerer.RewriteStatement(statement);
        }

        protected override BoundStatement RewriteForStatement(BoundForStatement node)
        {
            /*
                                                                                let <var> = <start>
                for <var> from <start> to <stop> step <step>        -->         let <end> = <stop>      <-- stop should only be calculated once
                {                                                               while <var> != <end>
                    <body>                                                      {
                }                                                                   <body>
                                                                                    <var> = <var> + <step>
                                                                                }
            */

            var declaration = new BoundVariableDeclaration(node.Variable, node.Start);
            var endDeclaration = new BoundVariableDeclaration(node.StopVar, node.Stop);
            var variableExpression = new BoundVariableExpression(node.Variable);

            var condition = new BoundBinaryExpression(
                variableExpression,
                BoundBinaryOperator.Bind(SyntaxKind.ExclamationEqualsToken, typeof(int), typeof(int)),
                new BoundVariableExpression(node.StopVar)
            );

            var increment = new BoundExpressionStatement(
                new BoundAssignmentExpression(
                    node.Variable,
                    new BoundBinaryExpression(
                        variableExpression,
                        BoundBinaryOperator.Bind(SyntaxKind.PlusToken, typeof(int), typeof(int)),
                        node.Step
                    )
                )
            );

            var whileBody = new BoundBlockStatement(ImmutableArray.Create(node.Body, increment));
            var whileStatement = new BoundWhileStatement(condition, false, whileBody);

            var result = new BoundBlockStatement(ImmutableArray.Create<BoundStatement>(declaration, endDeclaration, whileStatement));

            return RewriteStatement(result);
        }

        protected override BoundStatement RewriteLoopStatement(BoundLoopStatement node)
        {
            /*
                loop                    while true
                {                       {
                    <body>      -->         <body>
                }                       {
            */

            var literal = new BoundLiteralExpresion(true);

            var whileStatement = new BoundWhileStatement(literal, false, node.Body);

            return RewriteStatement(whileStatement);
        }
    }
}