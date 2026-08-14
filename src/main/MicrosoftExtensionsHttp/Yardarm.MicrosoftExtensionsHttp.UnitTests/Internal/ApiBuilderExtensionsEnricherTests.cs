using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Xunit;
using Yardarm.Enrichment.Compilation;
using Yardarm.Generation;
using Yardarm.MicrosoftExtensionsHttp.Internal;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

#pragma warning disable xUnit1051

namespace Yardarm.MicrosoftExtensionsHttp.UnitTests.Internal;

public class ApiBuilderExtensionsEnricherTests
{
    [Fact]
    public void Enrich_OnlyAnnotatedGeneratedTagInterfaces_AddsOnlyGeneratedTags()
    {
        ILocatedOpenApiElement<IOpenApiTag> includedTag = new OpenApiTag { Name = "Included" }.CreateRoot("included");
        var elementRegistry = new TestElementRegistry();
        var resourceTree = CSharpSyntaxTree.ParseText("""
            public static class ApiBuilderExtensions
            {
                private static void AddAllApisInternal(IApiBuilder builder, object configureClient, bool skipIfAlreadyRegistered)
                {
                }
            }
            """);
        var generatedTree = CSharpSyntaxTree.Create(
            CompilationUnit().AddMembers(
                InterfaceDeclaration("IIncluded").AddElementAnnotation(includedTag, elementRegistry),
                InterfaceDeclaration("IExcluded"),
                InterfaceDeclaration("INotATag")));
        var compilation = CSharpCompilation.Create("Test", [resourceTree, generatedTree]);
        var enricher = new ApiBuilderExtensionsEnricher(
            new TestTypeGeneratorRegistry("IIncluded"),
            new TestTypeGeneratorRegistry("Included"),
            elementRegistry);

        CompilationUnitSyntax result = enricher.Enrich(resourceTree.GetCompilationUnitRoot(),
            new ResourceFileEnrichmentContext(compilation, resourceTree,
                "Yardarm.MicrosoftExtensionsHttp.Client.ApiBuilderExtensions.cs"));

        string body = result.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == "AddAllApisInternal").Body!.ToFullString();
        body.Should().Contain("AddApi<IIncluded,Included>");
        body.Should().NotContain("IExcluded");
    }

    private sealed class TestElementRegistry : IOpenApiElementRegistry
    {
        private readonly Dictionary<string, ILocatedOpenApiElement> _elements = [];

        public ILocatedOpenApiElement<T> Get<T>(string key) where T : IOpenApiElement =>
            TryGet<T>(key, out var element) ? element : throw new KeyNotFoundException();

        public bool TryGet<T>(string key, [MaybeNullWhen(false)] out ILocatedOpenApiElement<T> element) where T : IOpenApiElement
        {
            if (_elements.TryGetValue(key, out var untypedElement) && untypedElement is ILocatedOpenApiElement<T> typedElement)
            {
                element = typedElement;
                return true;
            }

            element = null!;
            return false;
        }

        public string Add<T>(ILocatedOpenApiElement<T> element) where T : IOpenApiElement
        {
            string key = Guid.NewGuid().ToString();
            _elements.Add(key, element);
            return key;
        }
    }

    private sealed class TestTypeGeneratorRegistry(string typeName) : ITypeGeneratorRegistry<IOpenApiTag>
    {
        private readonly ITypeGenerator _generator = new TestTypeGenerator(typeName);

        public ITypeGenerator Get(ILocatedOpenApiElement<IOpenApiTag> element) => _generator;

        public IEnumerable<ITypeGenerator> GetAll() => [_generator];
    }

    private sealed class TestTypeGenerator(string typeName) : ITypeGenerator
    {
        public ITypeGenerator Parent => null;

        public YardarmTypeInfo TypeInfo { get; } = new(IdentifierName(typeName), NameKind.Interface);

        public QualifiedNameSyntax GetTypeName() => null;

        public SyntaxTree GenerateSyntaxTree() => null;

        public IEnumerable<MemberDeclarationSyntax> Generate() => [];

        public QualifiedNameSyntax GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
            where TChild : IOpenApiElement => null;
    }
}
