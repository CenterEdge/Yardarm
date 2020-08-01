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

        public static LiteralExpressionSyntax StringLiteral(string value) =>
            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
    }
}
