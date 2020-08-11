using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm.Names
{
    public class DefaultNamespaceProvider : INamespaceProvider
    {
        private readonly INameFormatter _namespaceFormatter;
        private readonly NameSyntax _rootNamespace;

        public DefaultNamespaceProvider(YardarmGenerationSettings settings, INameFormatterSelector nameFormatterSelector)
        {
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }
            if (nameFormatterSelector == null)
            {
                throw new ArgumentNullException(nameof(nameFormatterSelector));
            }

            _rootNamespace = SyntaxFactory.ParseName(settings.RootNamespace);
            _namespaceFormatter = nameFormatterSelector.GetFormatter(NameKind.Namespace);
        }

        public NameSyntax GetRootNamespace() => _rootNamespace;

        public NameSyntax GetNamespace(LocatedOpenApiElement element) =>
            element switch
            {
                LocatedOpenApiElement<OpenApiOperation> operation => GetOperationNamespace(operation),
                LocatedOpenApiElement<OpenApiRequestBody> requestBody => GetRequestBodyNamespace(requestBody),
                LocatedOpenApiElement<OpenApiResponse> response => GetResponseNamespace(response),
                LocatedOpenApiElement<OpenApiResponses> responses => GetResponsesNamespace(responses),
                LocatedOpenApiElement<OpenApiSchema> schema => GetSchemaNamespace(schema),
                LocatedOpenApiElement<OpenApiTag> tag => GetTagNamespace(tag),
                _ => throw new InvalidOperationException($"Element type {element.Element.GetType()} doesn't have a namespace.")
            };

        protected virtual NameSyntax GetOperationNamespace(LocatedOpenApiElement<OpenApiOperation> operation) =>
            SyntaxFactory.QualifiedName(_rootNamespace, SyntaxFactory.IdentifierName("Requests"));

        protected virtual NameSyntax GetRequestBodyNamespace(LocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            SyntaxFactory.QualifiedName(_rootNamespace, SyntaxFactory.IdentifierName("Models"));

        protected virtual NameSyntax GetResponseNamespace(LocatedOpenApiElement<OpenApiResponse> response) =>
            SyntaxFactory.QualifiedName(_rootNamespace, SyntaxFactory.IdentifierName("Responses"));

        protected virtual NameSyntax GetResponsesNamespace(LocatedOpenApiElement<OpenApiResponses> responses) =>
            SyntaxFactory.QualifiedName(_rootNamespace, SyntaxFactory.IdentifierName("Responses"));

        protected virtual NameSyntax GetSchemaNamespace(LocatedOpenApiElement<OpenApiSchema> schema) =>
            SyntaxFactory.QualifiedName(_rootNamespace, SyntaxFactory.IdentifierName("Models"));

        protected virtual NameSyntax GetTagNamespace(LocatedOpenApiElement<OpenApiTag> tag) =>
            SyntaxFactory.QualifiedName(_rootNamespace, SyntaxFactory.IdentifierName(_namespaceFormatter.Format(tag.Element.Name)));
    }
}
