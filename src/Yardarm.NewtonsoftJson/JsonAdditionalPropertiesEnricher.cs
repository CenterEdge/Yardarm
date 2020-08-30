using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Yardarm.Enrichment;
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
    public class JsonAdditionalPropertiesEnricher : IOpenApiSyntaxNodeEnricher<CompilationUnitSyntax, OpenApiSchema>
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
                Attribute(NewtonsoftJsonTypes.JsonExtensionDataAttributeName)))));

        private readonly IJsonSerializationNamespace _jsonSerializationNamespace;

        public int Priority => 0;

        public JsonAdditionalPropertiesEnricher(IJsonSerializationNamespace jsonSerializationNamespace)
        {
            _jsonSerializationNamespace = jsonSerializationNamespace ?? throw new ArgumentNullException(nameof(jsonSerializationNamespace));
        }

        public CompilationUnitSyntax Enrich(CompilationUnitSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            var members = target.GetSpecialMembers(SpecialMembers.AdditionalProperties)
                .OfType<PropertyDeclarationSyntax>().ToArray();

            target = target.TrackNodes((IEnumerable<PropertyDeclarationSyntax>) members);

            return members.Aggregate(target,
                (current, member) => current.ReplaceNode(current.GetCurrentNode(member), GenerateNewNodes(member)));
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateNewNodes(PropertyDeclarationSyntax property)
        {
            TypeSyntax valueType = property.Type.DescendantNodes()
                .OfType<GenericNameSyntax>()
                .First()
                .TypeArgumentList.Arguments[1];

            bool isDynamic = SyntaxHelpers.IsDynamic(valueType, out bool isNullable);

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
                .AddAttributeLists(AttributeList(SingletonSeparatedList(Attribute(NewtonsoftJsonTypes.JsonIgnoreAttributeName))))
                // Provide an AdditionalPropertiesDictionary referencing the backing field
                .WithExpressionBody(ArrowExpressionClause(
                    AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                        IdentifierName(WrapperFieldName),
                        ObjectCreationExpression(wrapperType)
                            .AddArgumentListArguments(
                                Argument(IdentifierName(BackingFieldName))))));
        }
    }
}
