using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Schema;
using Yardarm.Generation;
using Yardarm.Spec;
using Yardarm.SystemTextJson.Helpers;
using Yardarm.SystemTextJson.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson
{
    /// <summary>
    /// Updates additional properties members with the <see cref="JsonExtensionDataAttribute" />.
    /// </summary>
    public class JsonAdditionalPropertiesEnricher : IOpenApiSyntaxNodeEnricher<CompilationUnitSyntax, OpenApiSchema>
    {
        public Type[] ExecuteAfter { get; } =
        {
            typeof(AdditionalPropertiesEnricher)
        };

        private readonly IOpenApiElementRegistry _elementRegistry;

        public JsonAdditionalPropertiesEnricher(IOpenApiElementRegistry elementRegistry)
        {
            ArgumentNullException.ThrowIfNull(elementRegistry);
            _elementRegistry = elementRegistry;
        }

        public CompilationUnitSyntax Enrich(CompilationUnitSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            var members = target
                .GetSpecialMembers(SpecialMembers.AdditionalProperties)
                .OfType<PropertyDeclarationSyntax>()
                .Where(p => p.Parent is ClassDeclarationSyntax classDeclaration && _elementRegistry.IsJsonSchema(classDeclaration))
                .ToArray();

            if (members.Length == 0)
            {
                return target;
            }

            target = target.TrackNodes((IEnumerable<PropertyDeclarationSyntax>) members);

            return members.Aggregate(target,
                (current, member) => current.ReplaceNode(current.GetCurrentNode(member)!, AddAttribute(member)));
        }

        private PropertyDeclarationSyntax AddAttribute(PropertyDeclarationSyntax property)
        {
            var dictionaryType = property.Type;

            if (dictionaryType is QualifiedNameSyntax qualifiedName)
            {
                dictionaryType = qualifiedName.Right;
            }

            if (dictionaryType is not GenericNameSyntax genericName)
            {
                // Don't mutate
                return property;
            }

            return property
                // We must have a setter for JsonExtensionData to work with System.Text.Json
                .WithAccessorList(AccessorList(property.AccessorList!.Accessors.Add(
                    AccessorDeclaration(
                        SyntaxKind.SetAccessorDeclaration,
                        default,
                        default,
                        Token(SyntaxKind.SetKeyword),
                        null,
                        null,
                        Token(SyntaxKind.SemicolonToken)))))
                // Add the JsonExtensionData attribute
                .AddAttributeLists(AttributeList(SingletonSeparatedList(
                Attribute(SystemTextJsonTypes.Serialization.JsonExtensionDataAttributeName)))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
        }
    }
}
