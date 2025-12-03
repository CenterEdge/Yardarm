using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Registration;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NodaTime.Internal;

internal sealed class DefaultLiteralConverterEnricher : ReturnValueRegistrationEnricher, IDefaultLiteralConverterEnricher
{
    protected override ExpressionSyntax EnrichReturnValue(ExpressionSyntax target)
    {
        NameSyntax nodaLiteralConverters = IdentifierName("NodaLiteralConverters");

        return target
            .AddLiteralConverter(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, nodaLiteralConverters, IdentifierName("LocalDateConverter")))
            .AddLiteralConverter(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, nodaLiteralConverters, IdentifierName("LocalDateTimeConverter")))
            .AddLiteralConverter(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, nodaLiteralConverters, IdentifierName("LocalTimeConverter")))
            .AddLiteralConverter(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, nodaLiteralConverters, IdentifierName("OffsetDateTimeConverter")))
            .AddLiteralConverter(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, nodaLiteralConverters, IdentifierName("OffsetTimeConverter")))
            .AddLiteralConverter(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, nodaLiteralConverters, IdentifierName("PeriodConverter")));
    }
}
