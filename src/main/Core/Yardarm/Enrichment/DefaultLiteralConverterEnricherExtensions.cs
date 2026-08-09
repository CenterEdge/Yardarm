using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment;

public static class DefaultLiteralConverterEnricherExtensions
{
    extension(ExpressionSyntax currentExpression)
    {
        public ExpressionSyntax AddLiteralConverter(ExpressionSyntax literalConverter) =>
            InvocationExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    currentExpression.WithTrailingTrivia(TriviaList(CarriageReturnLineFeed, Whitespace("    "))),
                    IdentifierName("Add")),
                ArgumentList(SingletonSeparatedList(Argument(literalConverter))));
    }
}
