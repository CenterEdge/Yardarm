using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Helpers
{
    public static class SyntaxHelpers
    {
        public static LiteralExpressionSyntax StringLiteral(string value) =>
            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value));
    }
}
