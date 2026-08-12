using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Authentication
{
    public class SecuritySchemeGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<IOpenApiSecurityScheme> _securitySchemeGeneratorRegistry;

        public SecuritySchemeGenerator(OpenApiDocument document, ITypeGeneratorRegistry<IOpenApiSecurityScheme> securitySchemeGeneratorRegistry)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(securitySchemeGeneratorRegistry);

            _document = document;
            _securitySchemeGeneratorRegistry = securitySchemeGeneratorRegistry;
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            foreach (var syntaxTree in GetSecuritySchemes()
                .Select(Generate)
                .Where(p => p != null))
            {
                yield return syntaxTree!;
            }
        }

        private IEnumerable<ILocatedOpenApiElement<IOpenApiSecurityScheme>> GetSecuritySchemes() =>
            _document.Components?.SecuritySchemes?
                .Select(p => p.Value.CreateRoot(p.Key))
            ?? [];

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<IOpenApiSecurityScheme> securityScheme) =>
            _securitySchemeGeneratorRegistry.Get(securityScheme).GenerateSyntaxTree();
    }
}
