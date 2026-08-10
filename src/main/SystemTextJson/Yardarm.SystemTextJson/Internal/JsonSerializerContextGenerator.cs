using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.Helpers;
using Yardarm.SystemTextJson.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson.Internal;

/// <summary>
/// Creates an empty <see cref="JsonSerializerContext"/> which will later be enriched with
/// <see cref="JsonSerializableAttribute"/> attributes.
/// </summary>
internal class JsonSerializerContextGenerator(
    IJsonSerializationNamespace jsonSerializationNamespace,
    GenerationContext context,
    IOptions<JsonOptions> jsonOptions,
    [FromKeyedServices(JsonSerializerContextGenerator.AttributeEnricherKey)] IEnumerable<IEnricher<AttributeSyntax>> enrichers)
    : ISyntaxTreeGenerator
{
    public const string AttributeEnricherKey = "JsonSourceGenerationOptions";

    public static SyntaxAnnotation GeneratorAnnotation { get; } = new(
        GeneratorSyntaxNodeExtensions.GeneratorAnnotationName,
        typeof(JsonSerializerContextGenerator).FullName);

    public static SyntaxToken TypeName { get; } = Identifier("ModelSerializerContext");

    public IEnumerable<SyntaxTree> Generate()
    {
        JsonOptions options = jsonOptions.Value;

        List<AttributeArgumentSyntax> arguments = [
            AttributeArgument(
                nameEquals: null,
                nameColon: null,
                expression: options.Strict
                    ? SystemTextJsonTypes.JsonSerializerDefaults.Strict
                    : SystemTextJsonTypes.JsonSerializerDefaults.Web),
        ];

        if (options.AllowDuplicateProperties is bool allowDuplicateProperties)
        {
            arguments.Add(AttributeArgument(
                nameEquals: NameEquals("AllowDuplicateProperties"),
                nameColon: null,
                expression: SyntaxHelpers.BoolLiteral(allowDuplicateProperties)));
        }

        if (options.NumberHandling is JsonNumberHandling numberHandling)
        {
            arguments.Add(AttributeArgument(
                nameEquals: NameEquals("NumberHandling"),
                nameColon: null,
                expression: MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SystemTextJsonTypes.Serialization.JsonNumberHandling.Name,
                    IdentifierName(numberHandling.ToString()))));
        }

        if (options.PropertyNameCaseInsensitive is bool propertyNameCaseInsensitive)
        {
            arguments.Add(AttributeArgument(
                nameEquals: NameEquals("PropertyNameCaseInsensitive"),
                nameColon: null,
                expression: SyntaxHelpers.BoolLiteral(propertyNameCaseInsensitive)));
        }

        if (options.RespectNullableAnnotations is bool respectNullableAnnotations)
        {
            arguments.Add(AttributeArgument(
                nameEquals: NameEquals("RespectNullableAnnotations"),
                nameColon: null,
                expression: SyntaxHelpers.BoolLiteral(respectNullableAnnotations)));
        }

        if (options.RespectRequiredConstructorParameters is bool respectRequiredConstructorParameters)
        {
            arguments.Add(AttributeArgument(
                nameEquals: NameEquals("RespectRequiredConstructorParameters"),
                nameColon: null,
                expression: SyntaxHelpers.BoolLiteral(respectRequiredConstructorParameters)));
        }

        if (options.UnmappedMemberHandling is JsonUnmappedMemberHandling unmappedMemberHandling)
        {
            arguments.Add(AttributeArgument(
                nameEquals: NameEquals("UnmappedMemberHandling"),
                nameColon: null,
                expression: MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SystemTextJsonTypes.Serialization.JsonUnmappedMemberHandling.Name,
                    IdentifierName(unmappedMemberHandling.ToString()))));
        }

        AttributeSyntax sourceGenerationOptionsAttribute = Attribute(
            SystemTextJsonTypes.Serialization.JsonSourceGenerationOptionsAttributeName,
            AttributeArgumentList(SeparatedList(arguments)));

        // Enrich the JsonSourceGenerationOptions attribute with any additional enrichers registered
        // by another extension via IEnricher<AttributeSyntax> with the key "JsonSourceGenerationOptions".
        sourceGenerationOptionsAttribute = sourceGenerationOptionsAttribute.Enrich(enrichers);

        // Create a partial class inherited from JsonSerializerContext with the attributes applied
        ClassDeclarationSyntax classDeclaration =
            ClassDeclaration(
                SingletonList(AttributeList(SingletonSeparatedList(sourceGenerationOptionsAttribute))),
                TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.PartialKeyword)),
                TypeName,
                null,
                BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                    SimpleBaseType(SystemTextJsonTypes.Serialization.JsonSerializerContextName))),
                default,
                default)
            .WithAdditionalAnnotations(GeneratorAnnotation);

        return [
            CSharpSyntaxTree.Create(
                CompilationUnit(
                    default,
                    default,
                    default,
                    SingletonList<MemberDeclarationSyntax>(NamespaceDeclaration(
                        jsonSerializationNamespace.Name,
                        default,
                        default,
                        SingletonList<MemberDeclarationSyntax>(classDeclaration)))),
                options: context.ParseOptions,
                encoding: System.Text.Encoding.UTF8)
        ];
    }
}
