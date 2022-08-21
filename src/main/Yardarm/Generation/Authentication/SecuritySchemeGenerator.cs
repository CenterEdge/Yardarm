using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Authentication
{
    public class SecuritySchemeGenerator : ISyntaxTreeGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly ITypeGeneratorRegistry<OpenApiSecurityScheme> _securitySchemeGeneratorRegistry;

        public SecuritySchemeGenerator(OpenApiDocument document, ITypeGeneratorRegistry<OpenApiSecurityScheme> securitySchemeGeneratorRegistry)
        {
            _document = document ?? throw new ArgumentNullException(nameof(document));
            _securitySchemeGeneratorRegistry = securitySchemeGeneratorRegistry ?? throw new ArgumentNullException(nameof(securitySchemeGeneratorRegistry));
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

        private IEnumerable<ILocatedOpenApiElement<OpenApiSecurityScheme>> GetSecuritySchemes() =>
            _document.Components.SecuritySchemes
                .Select(p => p.Value.CreateRoot(p.Key));

        protected virtual SyntaxTree? Generate(ILocatedOpenApiElement<OpenApiSecurityScheme> securityScheme) =>
            _securitySchemeGeneratorRegistry.Get(securityScheme).GenerateSyntaxTree();
    }
}
