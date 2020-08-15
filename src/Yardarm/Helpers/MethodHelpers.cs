using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    public static class MethodHelpers
    {
        public static ParameterSyntax DefaultedCancellationTokenParameter() =>
            Parameter(Identifier("cancellationToken"))
                .WithType(WellKnownTypes.CancellationToken())
                .WithDefault(EqualsValueClause(
                    LiteralExpression(SyntaxKind.DefaultLiteralExpression,
                        Token(SyntaxKind.DefaultKeyword))));

        public static StatementSyntax LocalVariableDeclarationWithInitializer(string variableName,
            ExpressionSyntax initializer) =>
            LocalDeclarationStatement(VariableDeclaration(IdentifierName("var"))
                .AddVariables(VariableDeclarator(variableName).WithInitializer(EqualsValueClause(initializer))));
    }
}
