using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    public static class MethodHelpers
    {
        public static ParameterSyntax DefaultedCancellationTokenParameter() =>
            Parameter(Identifier("cancellationToken"))
                .WithType(WellKnownTypes.System.Threading.CancellationToken.Name)
                .WithDefault(EqualsValueClause(
                    LiteralExpression(SyntaxKind.DefaultLiteralExpression,
                        Token(SyntaxKind.DefaultKeyword))));

        public static StatementSyntax IfNotNull(ExpressionSyntax expressionToTest, BlockSyntax trueBlock) =>
            IfStatement(
                BinaryExpression(SyntaxKind.NotEqualsExpression,
                    expressionToTest,
                    LiteralExpression(SyntaxKind.NullLiteralExpression)),
                trueBlock);

        public static StatementSyntax LocalVariableDeclarationWithInitializer(string variableName,
            ExpressionSyntax initializer) =>
            LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                .AddVariables(VariableDeclarator(variableName).WithInitializer(EqualsValueClause(initializer))));
    }
}
