using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    public static class SyntaxHelpers
    {
        public static ExpressionSyntax MemberAccess(params string[] identifierNames) =>
            identifierNames
                .Skip(1)
                .Aggregate((ExpressionSyntax) IdentifierName(identifierNames[0]),
                    (agg, current) =>
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, agg, IdentifierName(current)));

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
