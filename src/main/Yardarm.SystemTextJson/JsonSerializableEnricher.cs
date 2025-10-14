using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
            bool hasEmittedDynamicTypes = false;
            char[] workingBuffer = new char[256];

            foreach (var type in _schemaGeneratorRegistry.GetAll().OfType<TypeGeneratorBase<OpenApiSchema>>())
            {
                if (!type.Element.IsJsonSchema)
                {
                    continue;
                }

                YardarmTypeInfo typeInfo = type.TypeInfo;
                TypeSyntax modelName = typeInfo.Name;
                string modelNameString = modelName.ToString();

                if (!hasEmittedDynamicTypes && typeInfo.RequiresDynamicSerialization)
                {
                    // For JsonSerializerContext, it can't handle dynamic object cases unless we explicitly include
                    // the dynamic types. If we have any dynamic object cases, include them, but only once.

                    hasEmittedDynamicTypes = true;

                    yield return (SystemTextJsonTypes.JsonElement, "JsonElement");
                    yield return (SystemTextJsonTypes.Nodes.JsonNodeName, "JsonNode");
                }

                if (alreadyEmitted.Contains(modelNameString))
                {
                    continue;
                }

                if (typeInfo.IsGenerated)
                {
                    alreadyEmitted.Add(modelNameString);
                    yield return (modelName,
                        GetPropertyName(workingBuffer, _rootNamespacePrefix, $"{modelNameString}"));
                }
                else if (WellKnownTypes.System.Collections.Generic.ListT.IsOfType(modelName, out var genericArgument))
                {
                    alreadyEmitted.Add(modelNameString);
                    yield return (modelName,
                        GetPropertyName(workingBuffer, _rootNamespacePrefix,$"__List__{genericArgument}"));
                }
                else if (WellKnownTypes.System.Collections.Generic.DictionaryT.IsOfType(modelName,
                             out var keyArgument, out var valueArgument))
                {
                    alreadyEmitted.Add(modelNameString);
                    yield return (modelName,
                        GetPropertyName(workingBuffer, _rootNamespacePrefix, $"__Dictionary__Of__{keyArgument}__AndOf__{valueArgument}"));
                }
            }
        }

        // Since we may have multiple models with the same name but nested within different classes, we need
        // to avoid collisions by giving them a unique name.
        private static string GetPropertyName(
            Span<char> initialBuffer,
            string rootNamespacePrefix,
            [InterpolatedStringHandlerArgument(nameof(initialBuffer), nameof(rootNamespacePrefix))] ref IdentifierSafeInterpolatedStringHandler typeName) =>
            typeName.ToStringAndClear();

        [InterpolatedStringHandler]
        private ref struct IdentifierSafeInterpolatedStringHandler(
            int literalLength,
            int formattedCount,
            Span<char> initialBuffer,
            string rootNamespacePrefix)
        {
            private DefaultInterpolatedStringHandler _innerHandler = new(literalLength, formattedCount, null, initialBuffer);

            public void AppendLiteral(string s) => _innerHandler.AppendLiteral(s);

            public void AppendFormatted<T>(T value) => AppendFormatted(value?.ToString());

            public void AppendFormatted(string? value)
            {
                const string GlobalPrefix = "global::";

                if (value is null)
                {
                    return;
                }

                ReadOnlySpan<char> chars = value.AsSpan();

                Span<char> buffer = chars.Length <= 256
                    ? stackalloc char[chars.Length]
                    : new char[chars.Length];

                while (chars.Length > 0)
                {
                    if (chars.StartsWith(GlobalPrefix))
                    {
                        chars = chars[GlobalPrefix.Length..];
                    }
                    if (chars.StartsWith(rootNamespacePrefix))
                    {
                        chars = chars[rootNamespacePrefix.Length..];
                    }

                    // Skip separators
                    int bufferLength = 0;
                    bool newIdentifier = false;
                    int i = 0;
                    while (i < chars.Length && !newIdentifier)
                    {
                        char ch = chars[i];

                        switch (ch)
                        {
                            case '.' or '?':
                                buffer[bufferLength] = '_';
                                bufferLength++;
                                break;

                            case '<':
                                // Append the buffer so far, then append "__Of__" to indicate a generic type
                                _innerHandler.AppendFormatted(buffer.Slice(0, bufferLength));
                                bufferLength = 0;

                                _innerHandler.AppendLiteral("__Of__");

                                newIdentifier = true;
                                break;

                            case ',':
                                // Append the buffer so far, then append "__AndOf__" to indicate a additional generic parameter
                                _innerHandler.AppendFormatted(buffer.Slice(0, bufferLength));
                                bufferLength = 0;

                                _innerHandler.AppendLiteral("__AndOf__");

                                newIdentifier = true;
                                break;

                            case '>':
                                // Drop the trailing '>' character from the generic type
                                break;

                            default:
                                buffer[bufferLength] = ch;
                                bufferLength++;
                                break;
                        }

                        i++;
                    }

                    chars = chars.Slice(i);
                    if (bufferLength > 0)
                    {
                        _innerHandler.AppendFormatted(buffer.Slice(0, bufferLength));
                    }
                }
            }

            public override string ToString() => _innerHandler.ToString();

            public string ToStringAndClear() => _innerHandler.ToStringAndClear();
        }
    }
}
