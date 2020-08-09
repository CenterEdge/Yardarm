using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Helpers
{
    public static class SyntaxHelpers
    {
        public static TypeSyntax ListT(TypeSyntax itemType) =>
            SyntaxFactory.QualifiedName(
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.IdentifierName("System"),
                        SyntaxFactory.IdentifierName("Collections")
                    ),
                    SyntaxFactory.IdentifierName("Generic")),
                SyntaxFactory.GenericName(
                        SyntaxFactory.Identifier("List"),
                        SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(itemType))));

        public static TypeSyntax TaskT(TypeSyntax resultType) =>
            SyntaxFactory.QualifiedName(
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.QualifiedName(
                        SyntaxFactory.IdentifierName("System"),
                        SyntaxFactory.IdentifierName("Threading")
                    ),
                    SyntaxFactory.IdentifierName("Tasks")),
                SyntaxFactory.GenericName(
                    SyntaxFactory.Identifier("Task"),
                    SyntaxFactory.TypeArgumentList(SyntaxFactory.SingletonSeparatedList<TypeSyntax>(resultType))));

        public static TypeSyntax CancellationToken() =>
            SyntaxFactory.QualifiedName(
                SyntaxFactory.QualifiedName(
                    SyntaxFactory.IdentifierName("System"),
                    SyntaxFactory.IdentifierName("Threading")
                ),
                SyntaxFactory.IdentifierName("CancellationToken"));

        public static ParameterSyntax DefaultedCancellationTokenParameter() =>
            SyntaxFactory.Parameter(SyntaxFactory.Identifier("cancellationToken"))
                .WithType(CancellationToken())
                .WithDefault(SyntaxFactory.EqualsValueClause(
                    SyntaxFactory.LiteralExpression(SyntaxKind.DefaultLiteralExpression,
                        SyntaxFactory.Token(SyntaxKind.DefaultKeyword))));

        public static LiteralExpressionSyntax StringLiteral(string value) =>
            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
    }
}
