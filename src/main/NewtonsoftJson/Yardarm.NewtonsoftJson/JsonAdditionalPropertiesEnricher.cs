using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Newtonsoft.Json;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Schema;
using Yardarm.Generation;
using Yardarm.Helpers;
using Yardarm.NewtonsoftJson.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NewtonsoftJson
{
    /// <summary>
    /// Replaces additional properties members with a backing implementation that can convert from JToken objects.
    /// Also wires up a private field to use <see cref="JsonExtensionDataAttribute"/> to serializer/deserialize
    /// the properties.
    /// </summary>
    /// <remarks>
    /// This enriches class declarations rather than compilation units. Compilation units are only annotated with the
    /// element of their root type, so schemas nested within other types, such as inline request and response body
    /// schemas, are not reachable from a compilation unit enricher.
    /// </remarks>
    public class JsonAdditionalPropertiesEnricher : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, IOpenApiSchema>
    {
        private const string BackingFieldName = "_additionalProperties";
        private const string WrapperFieldName = "_additionalPropertiesWrapper";

        private static readonly TypeSyntax _backingFieldType =
            WellKnownTypes.System.Collections.Generic.DictionaryT.Name(
                PredefinedType(Token(SyntaxKind.StringKeyword)),
                NewtonsoftJsonTypes.JTokenName);

        private static readonly FieldDeclarationSyntax _backingFieldDeclaration = FieldDeclaration(VariableDeclaration(
                _backingFieldType,
                SingletonSeparatedList(
                    VariableDeclarator(Identifier(BackingFieldName),
                        null,
                        EqualsValueClause(ObjectCreationExpression(_backingFieldType))))))
            .AddModifiers(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.ReadOnlyKeyword))
            .WithAttributeLists(SingletonList(AttributeList(SingletonSeparatedList(
                Attribute(NewtonsoftJsonTypes.JsonExtensionDataAttributeName)))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed)));

        private readonly IJsonSerializationNamespace _jsonSerializationNamespace;

        public Type[] ExecuteAfter { get; } =
        {
            typeof(AdditionalPropertiesEnricher)
        };

        public JsonAdditionalPropertiesEnricher(IJsonSerializationNamespace jsonSerializationNamespace)
        {
            ArgumentNullException.ThrowIfNull(jsonSerializationNamespace);

            _jsonSerializationNamespace = jsonSerializationNamespace;
        }

        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<IOpenApiSchema> context)
        {
            // Only direct members, nested schema classes are enriched separately
            var members = target.Members
                .OfType<PropertyDeclarationSyntax>()
                .Where(p => p.GetSpecialMemberAnnotation() == SpecialMembers.AdditionalProperties)
                .ToArray();

            target = target.TrackNodes((IEnumerable<PropertyDeclarationSyntax>) members);

            return members.Aggregate(target,
                (current, member) => current.ReplaceNode(current.GetCurrentNode(member)!, GenerateNewNodes(member)));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateNewNodes(PropertyDeclarationSyntax property)
        {
            TypeSyntax valueType = property.Type.DescendantNodes()
                .OfType<GenericNameSyntax>()
                .First()
                .TypeArgumentList.Arguments[1];

            bool isDynamic = SyntaxHelpers.IsObject(valueType, out bool isNullable);

            TypeSyntax wrapperType = isDynamic
                ? isNullable
                    ? _jsonSerializationNamespace.NullableDynamicAdditionalPropertiesDictionary
                    : _jsonSerializationNamespace.DynamicAdditionalPropertiesDictionary
                : _jsonSerializationNamespace.AdditionalPropertiesDictionary(valueType);

            yield return _backingFieldDeclaration;

            yield return FieldDeclaration(VariableDeclaration(
                    NullableType(wrapperType),
                    SingletonSeparatedList(VariableDeclarator(Identifier(WrapperFieldName)))))
                .AddModifiers(Token(SyntaxKind.PrivateKeyword));

            yield return property
                // Remove the getters and setters
                .WithAccessorList(null)
                // Remove the old initializer
                .WithInitializer(null)
                // Prevent serialization
                .AddAttributeLists(AttributeList(SingletonSeparatedList(Attribute(NewtonsoftJsonTypes.JsonIgnoreAttributeName)))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed))
                // Provide an AdditionalPropertiesDictionary referencing the backing field
                .WithExpressionBody(ArrowExpressionClause(
                    AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                        IdentifierName(WrapperFieldName),
                        ObjectCreationExpression(wrapperType)
                            .AddArgumentListArguments(
                                Argument(IdentifierName(BackingFieldName))))))
                .WithSemicolonToken(Token(SyntaxKind.SemicolonToken));
        }
    }
}
