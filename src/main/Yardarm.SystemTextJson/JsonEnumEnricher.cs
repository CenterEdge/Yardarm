using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.SystemTextJson.Helpers;
using Yardarm.SystemTextJson.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson
{
    public class JsonEnumEnricher : IOpenApiSyntaxNodeEnricher<EnumDeclarationSyntax, OpenApiSchema>
    {
        public EnumDeclarationSyntax Enrich(EnumDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context) =>
            context.Element.Type == "string" && context.LocatedElement.IsJsonSchema
                ? EnrichEnum(target)
                : target;

        private static EnumDeclarationSyntax EnrichEnum(EnumDeclarationSyntax target)
        {
            var members = target.Members
                .Select(member =>
                {
                    AttributeSyntax? enumMemberAttribute = member.AttributeLists
                        .SelectMany(p => p.Attributes)
                        .FirstOrDefault(p =>
                        {
                            NameSyntax name = p.Name;
                            while (name is QualifiedNameSyntax qualifiedName)
                            {
                                name = qualifiedName.Right;
                            }

                            return name is IdentifierNameSyntax identifierName && identifierName.Identifier.ValueText == "EnumMember";
                        });

                    if (enumMemberAttribute is not null)
                    {
                        ExpressionSyntax? name = enumMemberAttribute.ArgumentList?.Arguments
                            .FirstOrDefault()?.Expression;
                        if (name is not null)
                        {
                            member = member
                                .AddAttributeLists(AttributeList(SingletonSeparatedList(
                                    Attribute(
                                        SystemTextJsonTypes.Serialization.JsonStringEnumMemberNameAttributeName,
                                        AttributeArgumentList(SingletonSeparatedList(
                                            AttributeArgument(name)))))))
                                .WithTrailingTrivia(ElasticCarriageReturnLineFeed);
                        }
                    }

                    return member;
                });

            target = target
                .AddAttributeLists(AttributeList(SingletonSeparatedList(
                    Attribute(
                        SystemTextJsonTypes.Serialization.JsonConverterAttributeName,
                        AttributeArgumentList(SingletonSeparatedList(
                            AttributeArgument(TypeOfExpression(
                                SystemTextJsonTypes.Serialization.JsonStringEnumConverterName(IdentifierName(target.Identifier)))))))))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed))
                .WithMembers(SeparatedList(members));

            return target;
        }
    }
}
