using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    public static class SyntaxHelpers
    {
        public static ParameterSyntax DefaultedCancellationTokenParameter() =>
            Parameter(Identifier("cancellationToken"))
                .WithType(WellKnownTypes.CancellationToken())
                .WithDefault(EqualsValueClause(
                    LiteralExpression(SyntaxKind.DefaultLiteralExpression,
                        Token(SyntaxKind.DefaultKeyword))));

        public static LiteralExpressionSyntax StringLiteral(string value) =>
            LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));

        public static ExpressionSyntax ParameterWithNullCheck(string parameterName) =>
            BinaryExpression(SyntaxKind.CoalesceExpression,
                IdentifierName(parameterName),
                ThrowExpression(ObjectCreationExpression(
                    QualifiedName(IdentifierName("System"), IdentifierName("ArgumentNullException")),
                    ArgumentList(SingletonSeparatedList(Argument(
                        StringLiteral(parameterName)))),
                    null)));
    }
}
