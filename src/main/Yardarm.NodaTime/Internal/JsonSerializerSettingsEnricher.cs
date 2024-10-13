using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Enrichment.Registration;
using Yardarm.NodaTime.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NodaTime.Internal;

/// <summary>
/// Enriches the JsonSerializerSettings on the JsonTypeSerializer when Newtonsoft.Json is used.
/// </summary>
internal sealed class JsonSerializerSettingsEnricher : StatementInsertingRegistrationEnricher
{
    protected override IEnumerable<StatementSyntax> GenerateStatements(ExpressionSyntax? returnExpression)
    {
        if (returnExpression is null)
        {
            return [];
        }

        StatementSyntax statement = ExpressionStatement(InvocationExpression(
            MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                NodaTimeTypes.Serialization.JsonNet.Extensions,
                IdentifierName("ConfigureForNodaTime")),
            ArgumentList(SeparatedList(
            [
                Argument(returnExpression),
                Argument(MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    NodaTimeTypes.DateTimeZoneProviders,
                    IdentifierName("Tzdb"))),
            ]))));

        return [statement];
    }
}
