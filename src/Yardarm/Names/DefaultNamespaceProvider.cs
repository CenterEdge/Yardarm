using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Names
{
    public class DefaultNamespaceProvider : INamespaceProvider
    {
        private readonly IRootNamespace _rootNamespace;

        public DefaultNamespaceProvider(IRootNamespace rootNamespace)
        {
            _rootNamespace = rootNamespace ?? throw new ArgumentNullException(nameof(rootNamespace));
        }

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
            SyntaxFactory.QualifiedName(_rootNamespace.Name, SyntaxFactory.IdentifierName("Requests"));

        protected virtual NameSyntax GetRequestBodyNamespace(LocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            SyntaxFactory.QualifiedName(_rootNamespace.Name, SyntaxFactory.IdentifierName("Models"));

        protected virtual NameSyntax GetResponseNamespace(LocatedOpenApiElement<OpenApiResponse> response) =>
            SyntaxFactory.QualifiedName(_rootNamespace.Name, SyntaxFactory.IdentifierName("Responses"));

        protected virtual NameSyntax GetResponsesNamespace(LocatedOpenApiElement<OpenApiResponses> responses) =>
            SyntaxFactory.QualifiedName(_rootNamespace.Name, SyntaxFactory.IdentifierName("Responses"));

        protected virtual NameSyntax GetSchemaNamespace(LocatedOpenApiElement<OpenApiSchema> schema) =>
            SyntaxFactory.QualifiedName(_rootNamespace.Name, SyntaxFactory.IdentifierName("Models"));

        protected virtual NameSyntax GetTagNamespace(LocatedOpenApiElement<OpenApiTag> tag) =>
            SyntaxFactory.QualifiedName(_rootNamespace.Name, SyntaxFactory.IdentifierName("Api"));
    }
}
