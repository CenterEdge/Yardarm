using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment;

public static class DefaultLiteralConverterEnricherExtensions
{
    public static ExpressionSyntax AddLiteralConverter(this ExpressionSyntax currentExpression, ExpressionSyntax literalConverter) =>
        InvocationExpression(
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                currentExpression.WithTrailingTrivia(TriviaList(CarriageReturnLineFeed, Whitespace("    "))),
                IdentifierName("Add")),
            ArgumentList(SingletonSeparatedList(Argument(literalConverter))));
}
