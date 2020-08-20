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
        private readonly IResponsesNamespace _responsesNamespace;

        public DefaultNamespaceProvider(IRootNamespace rootNamespace, IResponsesNamespace responsesNamespace)
        {
            _rootNamespace = rootNamespace ?? throw new ArgumentNullException(nameof(rootNamespace));
            _responsesNamespace = responsesNamespace ?? throw new ArgumentNullException(nameof(responsesNamespace));
        }

        public NameSyntax GetNamespace(ILocatedOpenApiElement element) =>
            element switch
            {
                ILocatedOpenApiElement<OpenApiOperation> operation => GetOperationNamespace(operation),
                ILocatedOpenApiElement<OpenApiRequestBody> requestBody => GetRequestBodyNamespace(requestBody),
                ILocatedOpenApiElement<OpenApiUnknownResponse> response => GetUnknownResponseNamespace(response),
                ILocatedOpenApiElement<OpenApiResponse> response => GetResponseNamespace(response),
                ILocatedOpenApiElement<OpenApiResponses> responses => GetResponsesNamespace(responses),
                ILocatedOpenApiElement<OpenApiSchema> schema => GetSchemaNamespace(schema),
                ILocatedOpenApiElement<OpenApiTag> tag => GetTagNamespace(tag),
                _ => throw new InvalidOperationException($"Element type {element.Element.GetType()} doesn't have a namespace.")
            };

        protected virtual NameSyntax GetOperationNamespace(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            SyntaxFactory.QualifiedName(_rootNamespace.Name, SyntaxFactory.IdentifierName("Requests"));

        protected virtual NameSyntax GetRequestBodyNamespace(ILocatedOpenApiElement<OpenApiRequestBody> requestBody) =>
            SyntaxFactory.QualifiedName(_rootNamespace.Name, SyntaxFactory.IdentifierName("Models"));

        protected virtual NameSyntax GetResponseNamespace(ILocatedOpenApiElement<OpenApiResponse> response) =>
            _responsesNamespace.Name;

        protected virtual NameSyntax GetResponsesNamespace(ILocatedOpenApiElement<OpenApiResponses> responses) =>
            _responsesNamespace.Name;

        protected virtual NameSyntax GetSchemaNamespace(ILocatedOpenApiElement<OpenApiSchema> schema) =>
            SyntaxFactory.QualifiedName(_rootNamespace.Name, SyntaxFactory.IdentifierName("Models"));

        protected virtual NameSyntax GetTagNamespace(ILocatedOpenApiElement<OpenApiTag> tag) =>
            SyntaxFactory.QualifiedName(_rootNamespace.Name, SyntaxFactory.IdentifierName("Api"));

        protected virtual NameSyntax GetUnknownResponseNamespace(ILocatedOpenApiElement<OpenApiUnknownResponse> responses) =>
            _responsesNamespace.Name;
    }
}
