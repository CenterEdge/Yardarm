using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment.Compilation;
using Yardarm.Generation;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.SystemTextJson.Helpers;
using Yardarm.SystemTextJson.Internal;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson
{
    /// <summary>
    /// Adds <see cref="JsonSerializableAttribute"/> for each schema to the ModelSerializerContext.
    /// </summary>
    internal class JsonSerializableEnricher : ICompilationEnricher
    {
        private readonly ITypeGeneratorRegistry<OpenApiSchema> _schemaGeneratorRegistry;
        private readonly string _rootNamespacePrefix;

        public Type[] ExecuteAfter { get; } =
        [
            typeof(ResourceFileCompilationEnricher),
            typeof(SyntaxTreeCompilationEnricher)
        ];

        public JsonSerializableEnricher(ITypeGeneratorRegistry<OpenApiSchema> schemaGeneratorRegistry, IRootNamespace rootNamespace)
        {
            ArgumentNullException.ThrowIfNull(schemaGeneratorRegistry);

            _schemaGeneratorRegistry = schemaGeneratorRegistry;
            _rootNamespacePrefix = rootNamespace.Name + ".";
        }

        public ValueTask<CSharpCompilation> EnrichAsync(CSharpCompilation target, CancellationToken cancellationToken = default)
        {
            foreach (var syntaxTree in target.SyntaxTrees)
            {
                var compilationUnit = syntaxTree.GetRoot(cancellationToken);
                var declarations = compilationUnit
                    .GetAnnotatedNodes(JsonSerializerContextGenerator.GeneratorAnnotation)
                    .OfType<ClassDeclarationSyntax>()
                    .ToArray();

                if (declarations.Length > 0)
                {
                    var newCompilationUnit = compilationUnit.ReplaceNodes(
                        declarations,
                        AddAttributes);

                    target = target.ReplaceSyntaxTree(syntaxTree,
                        syntaxTree.WithRootAndOptions(newCompilationUnit, syntaxTree.Options));
                }
            }

            return new(target);
        }

        public ClassDeclarationSyntax AddAttributes(ClassDeclarationSyntax _, ClassDeclarationSyntax currentDeclaration)
        {
            var schemas = GetSchemas();

            // Generate JsonSerializable attributes for each type
            var typeInfoPropertyName = NameEquals(IdentifierName("TypeInfoPropertyName"));
            var attributeLists = schemas.Select(schema =>
                AttributeList(SingletonSeparatedList(Attribute(
                    SystemTextJsonTypes.Serialization.JsonSerializableAttributeName,
                    AttributeArgumentList(SeparatedList(new[]
                    {
                        AttributeArgument(TypeOfExpression(schema.typeName)),
                        AttributeArgument(
                            typeInfoPropertyName,
                            default,
                            SyntaxHelpers.StringLiteral(schema.propertyName))
                    }))))));

            return currentDeclaration.WithAttributeLists(
                currentDeclaration.AttributeLists.AddRange(attributeLists));
        }

        private IEnumerable<(TypeSyntax typeName, string propertyName)> GetSchemas()
        {
            // Track already generated types to avoid duplicates. This tends to occur with lists.
            var alreadyEmitted = new HashSet<string>(StringComparer.Ordinal);

            foreach (var type in _schemaGeneratorRegistry.GetAll().OfType<TypeGeneratorBase<OpenApiSchema>>())
            {
                if (!type.Element.IsJsonSchema())
                {
                    continue;
                }

                YardarmTypeInfo typeInfo = type.TypeInfo;
                TypeSyntax modelName = typeInfo.Name;
                string modelNameString = modelName.ToString();

                if (alreadyEmitted.Contains(modelNameString))
                {
                    continue;
                }

                if (typeInfo.IsGenerated)
                {
                    alreadyEmitted.Add(modelNameString);
                    yield return (modelName, GetPropertyName(modelNameString, false));
                }
                else if (WellKnownTypes.System.Collections.Generic.ListT.IsOfType(modelName, out var genericArgument))
                {
                    alreadyEmitted.Add(modelNameString);
                    yield return (modelName, GetPropertyName(genericArgument.ToString(), true));
                }
            }
        }

        // Since we may have multiple models with the same name but nested within different classes, we need
        // to avoid collisions by giving them a unique name.
        private string GetPropertyName(string typeName, bool isList)
        {
            const string listNamePrefix = "__List_";

            ReadOnlySpan<char> typeNameSpan = typeName.AsSpan();

            if (typeNameSpan.StartsWith("global::"))
            {
                typeNameSpan = typeNameSpan["global::".Length..];
            }
            if (typeNameSpan.StartsWith(_rootNamespacePrefix))
            {
                typeNameSpan = typeNameSpan[_rootNamespacePrefix.Length..];
            }

            Span<char> dest = stackalloc char[typeNameSpan.Length + listNamePrefix.Length];

            int length = 0;
            if (isList)
            {
                listNamePrefix.CopyTo(dest);
                length += listNamePrefix.Length;
            }

            // Skip separators
            foreach (char ch in typeNameSpan)
            {
                dest[length++] = ch switch
                {
                    '.' => '_',
                    '<' or '>' => 'ſ',
                    _ => ch
                };
            }

            return new string(dest[..length]);
        }
    }
}
