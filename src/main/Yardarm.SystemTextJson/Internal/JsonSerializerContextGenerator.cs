using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Generation;
using Yardarm.SystemTextJson.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson.Internal
{
    /// <summary>
    /// Creates an empty <see cref="JsonSerializerContext"/> which will later be enriched with
    /// <see cref="JsonSerializableAttribute"/> attributes.
    /// </summary>
    internal class JsonSerializerContextGenerator : ISyntaxTreeGenerator
    {
        public static SyntaxAnnotation GeneratorAnnotation { get; } = new(
            GeneratorSyntaxNodeExtensions.GeneratorAnnotationName,
            typeof(JsonSerializerContextGenerator).FullName);

        public static SyntaxToken TypeName { get; } = Identifier("ModelSerializerContext");

        private readonly IJsonSerializationNamespace _jsonSerializationNamespace;

        public JsonSerializerContextGenerator(IJsonSerializationNamespace jsonSerializationNamespace)
        {
            ArgumentNullException.ThrowIfNull(jsonSerializationNamespace);

            _jsonSerializationNamespace = jsonSerializationNamespace;
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            // Create a partial class inherited from JsonSerializerContext with the attributes applied
            var classDeclaration =
                ClassDeclaration(
                    SingletonList(AttributeList(SingletonSeparatedList(Attribute(
                        SystemTextJsonTypes.Serialization.JsonSourceGenerationOptionsAttributeName,
                        AttributeArgumentList(SingletonSeparatedList(
                            AttributeArgument(
                                nameEquals: NameEquals("NumberHandling"),
                                nameColon: null,
                                expression: SystemTextJsonTypes.Serialization.JsonNumberHandling.AllowReadingFromString))))))),
                    TokenList(Token(SyntaxKind.InternalKeyword), Token(SyntaxKind.PartialKeyword)),
                    TypeName,
                    null,
                    BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                        SimpleBaseType(SystemTextJsonTypes.Serialization.JsonSerializerContextName))),
                    default,
                    default)
                .WithAdditionalAnnotations(GeneratorAnnotation);

            yield return CSharpSyntaxTree.Create(CompilationUnit(
                default,
                default,
                default,
                SingletonList<MemberDeclarationSyntax>(NamespaceDeclaration(
                    _jsonSerializationNamespace.Name,
                    default,
                    default,
                    SingletonList<MemberDeclarationSyntax>(classDeclaration)))));
        }
    }
}
