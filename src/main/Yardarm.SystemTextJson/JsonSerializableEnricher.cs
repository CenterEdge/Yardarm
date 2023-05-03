using System;
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
        {
            typeof(ResourceFileCompilationEnricher),
            typeof(SyntaxTreeCompilationEnricher)
        };

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
            // Collect a list of schemas which have types generated
            var types = _schemaGeneratorRegistry.GetAll()
                .OfType<TypeGeneratorBase<OpenApiSchema>>()
                .Where(p => p.Element.IsJsonSchema())
                .Select(p =>
                {
                    YardarmTypeInfo typeInfo = p.TypeInfo;

                    TypeSyntax modelName = typeInfo.Name;
                    bool isList = false;
                    if (!typeInfo.IsGenerated
                        && WellKnownTypes.System.Collections.Generic.ListT.IsOfType(modelName, out var genericArgument))
                    {
                        isList = true;
                        modelName = genericArgument;
                    }

                    return (typeInfo, modelName, isList);
                })
                .Where(p => p.isList || p.typeInfo.IsGenerated);

            // Generate JsonSerializable attributes for each type
            var typeInfoPropertyName = NameEquals(IdentifierName("TypeInfoPropertyName"));
            var attributeLists = types.Select(type =>
                AttributeList(SingletonSeparatedList(Attribute(
                    SystemTextJsonTypes.Serialization.JsonSerializableAttributeName,
                    AttributeArgumentList(SeparatedList(new[]
                    {
                        AttributeArgument(TypeOfExpression(type.typeInfo.Name)),
                        AttributeArgument(
                            typeInfoPropertyName,
                            default,
                            SyntaxHelpers.StringLiteral(GetPropertyName(type.modelName, type.isList)))
                    }))))));

            return currentDeclaration.WithAttributeLists(currentDeclaration.AttributeLists.AddRange(attributeLists));
        }

        // Since we may have multiple models with the same name but nested within different classes, we need
        // to avoid collisions by giving them a unique name.
        private string GetPropertyName(TypeSyntax typeName, bool isList)
        {
            const string listNamePrefix = "__List_";

            ReadOnlySpan<char> typeNameString = typeName.ToString().AsSpan();

            if (typeNameString.StartsWith("global::"))
            {
                typeNameString = typeNameString.Slice("global::".Length);
            }
            if (typeNameString.StartsWith(_rootNamespacePrefix))
            {
                typeNameString = typeNameString.Slice(_rootNamespacePrefix.Length);
            }

            Span<char> dest = stackalloc char[typeNameString.Length + listNamePrefix.Length];

            int length = 0;
            if (isList)
            {
                listNamePrefix.CopyTo(dest);
                length += listNamePrefix.Length;
            }

            // Skip separators
            foreach (char ch in typeNameString)
            {
                dest[length++] = ch switch
                {
                    '.' => '_',
                    '<' or '>' => 'ſ',
                    _ => ch
                };
            }

            return new string(dest.Slice(0, length));
        }
    }
}
