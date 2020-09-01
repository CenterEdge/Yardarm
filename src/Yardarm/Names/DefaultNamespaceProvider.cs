using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Names
{
    public class DefaultNamespaceProvider : INamespaceProvider
    {
        private readonly IResponsesNamespace _responsesNamespace;
        private readonly IRequestsNamespace _requestsNamespace;
        private readonly IAuthenticationNamespace _authenticationNamespace;

        private readonly NameSyntax _apiNamespace;
        private readonly NameSyntax _headersNamespace;
        private readonly NameSyntax _modelsNamespace;
        private readonly NameSyntax _parametersNamespace;

        public DefaultNamespaceProvider(IRootNamespace rootNamespace, IResponsesNamespace responsesNamespace,
            IAuthenticationNamespace authenticationNamespace, IRequestsNamespace requestsNamespace)
        {
            if (rootNamespace == null)
            {
                throw new ArgumentNullException(nameof(rootNamespace));
            }

            _responsesNamespace = responsesNamespace ?? throw new ArgumentNullException(nameof(responsesNamespace));
            _authenticationNamespace = authenticationNamespace ??
                                       throw new ArgumentNullException(nameof(authenticationNamespace));
            _requestsNamespace = requestsNamespace ?? throw new ArgumentNullException(nameof(requestsNamespace));

            _apiNamespace = SyntaxFactory.QualifiedName(rootNamespace.Name, SyntaxFactory.IdentifierName("Api"));
            _headersNamespace = SyntaxFactory.QualifiedName(_responsesNamespace.Name, SyntaxFactory.IdentifierName("Headers"));
            _modelsNamespace = SyntaxFactory.QualifiedName(rootNamespace.Name, SyntaxFactory.IdentifierName("Models"));
            _parametersNamespace = SyntaxFactory.QualifiedName(_requestsNamespace.Name, SyntaxFactory.IdentifierName("Parameters"));
        }

        public NameSyntax GetNamespace(ILocatedOpenApiElement element) =>
            element switch
            {
                ILocatedOpenApiElement<OpenApiHeader> header => GetHeaderNamespace(header),
                ILocatedOpenApiElement<OpenApiMediaType> mediaType => GetMediaTypeNamespace(mediaType),
                ILocatedOpenApiElement<OpenApiOperation> operation => GetOperationNamespace(operation),
                ILocatedOpenApiElement<OpenApiParameter> parameter => GetParameterNamespace(parameter),
                ILocatedOpenApiElement<OpenApiUnknownResponse> response => GetUnknownResponseNamespace(response),
                ILocatedOpenApiElement<OpenApiResponse> response => GetResponseNamespace(response),
                ILocatedOpenApiElement<OpenApiResponses> responses => GetResponsesNamespace(responses),
                ILocatedOpenApiElement<OpenApiSchema> schema => GetSchemaNamespace(schema),
                ILocatedOpenApiElement<OpenApiSecurityScheme> securityScheme => GetSecuritySchemeNamespace(securityScheme),
                ILocatedOpenApiElement<OpenApiTag> tag => GetTagNamespace(tag),
                _ => throw new InvalidOperationException($"Element type {element.Element.GetType()} doesn't have a namespace.")
            };

        protected virtual NameSyntax GetHeaderNamespace(ILocatedOpenApiElement<OpenApiHeader> header) =>
            _headersNamespace;

        protected virtual NameSyntax GetMediaTypeNamespace(ILocatedOpenApiElement<OpenApiMediaType> mediaType) =>
            _requestsNamespace.Name;

        protected virtual NameSyntax GetOperationNamespace(ILocatedOpenApiElement<OpenApiOperation> operation) =>
            _requestsNamespace.Name;

        protected virtual NameSyntax GetParameterNamespace(ILocatedOpenApiElement<OpenApiParameter> operation) =>
            _parametersNamespace;

        protected virtual NameSyntax GetResponseNamespace(ILocatedOpenApiElement<OpenApiResponse> response) =>
            _responsesNamespace.Name;

        protected virtual NameSyntax GetResponsesNamespace(ILocatedOpenApiElement<OpenApiResponses> responses) =>
            _responsesNamespace.Name;

        protected virtual NameSyntax GetSchemaNamespace(ILocatedOpenApiElement<OpenApiSchema> schema) =>
            _modelsNamespace;

        protected virtual NameSyntax GetSecuritySchemeNamespace(ILocatedOpenApiElement<OpenApiSecurityScheme> schema) =>
            _authenticationNamespace.Name;

        protected virtual NameSyntax GetTagNamespace(ILocatedOpenApiElement<OpenApiTag> tag) =>
            _apiNamespace;

        protected virtual NameSyntax GetUnknownResponseNamespace(ILocatedOpenApiElement<OpenApiUnknownResponse> responses) =>
            _responsesNamespace.Name;
    }
}
