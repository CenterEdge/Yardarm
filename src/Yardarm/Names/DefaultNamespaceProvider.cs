using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Names
{
    public class DefaultNamespaceProvider : INamespaceProvider
    {
        private readonly NameSyntax _rootNamespace;

        public DefaultNamespaceProvider(YardarmGenerationSettings settings)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            _rootNamespace = SyntaxFactory.ParseName(settings.RootNamespace);
        }

        public virtual NameSyntax GetSchemaNamespace(LocatedOpenApiElement<OpenApiSchema> schema) =>
            SyntaxFactory.QualifiedName(_rootNamespace, SyntaxFactory.IdentifierName("Models"));

        public virtual NameSyntax GetRequestBodyNamespace(LocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            SyntaxFactory.QualifiedName(_rootNamespace, SyntaxFactory.IdentifierName("Models"));
    }
}
