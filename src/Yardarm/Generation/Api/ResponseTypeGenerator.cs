using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;

namespace Yardarm.Generation.Api
{
    internal class ResponseTypeGenerator : ITypeGenerator
    {
        protected LocatedOpenApiElement<OpenApiResponse> ResponseElement { get; }
        protected GenerationContext Context { get; }
        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected IHttpResponseCodeNameProvider HttpResponseCodeNameProvider { get; }

        protected OpenApiResponse Response => ResponseElement.Element;

        public ResponseTypeGenerator(LocatedOpenApiElement<OpenApiResponse> responseElement, GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider)
        {
            ResponseElement = responseElement ?? throw new ArgumentNullException(nameof(responseElement));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            HttpResponseCodeNameProvider = httpResponseCodeNameProvider ?? throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
        }

        public virtual void Preprocess() =>
            GetSchemaGenerator()?.Preprocess();

        public virtual TypeSyntax GetTypeName()
        {
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(ResponseElement);
            if (mediaType?.Element.Schema?.Type != "object" || mediaType.Element.Schema.Reference != null)
            {
                throw new InvalidOperationException("No valid media type for this request");
            }

            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(ResponseElement);

            if (Response.Reference != null)
            {
                // We're in the components section

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(Response.Reference.Id + "Response")));
            }
            else
            {
                // We're in an operation

                var operation = ResponseElement.Parents.OfType<LocatedOpenApiElement<OpenApiOperation>>().First().Element;

                var responseCode = Enum.TryParse<HttpStatusCode>(ResponseElement.Key, out var statusCode)
                    ? HttpResponseCodeNameProvider.GetName(statusCode)
                    : ResponseElement.Key;

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format($"{operation.OperationId}{responseCode}Response")));
            }
        }

        public SyntaxTree? GenerateSyntaxTree() =>
            GetSchemaGenerator()?.GenerateSyntaxTree();

        public IEnumerable<MemberDeclarationSyntax> Generate() =>
            GetSchemaGenerator()?.Generate() ?? Enumerable.Empty<MemberDeclarationSyntax>();

        private ITypeGenerator? GetSchemaGenerator()
        {
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(ResponseElement);
            if (mediaType?.Element.Schema?.Type != "object" || mediaType.Element.Schema.Reference != null)
            {
                return null;
            }

            var schemaElement = mediaType.CreateChild(mediaType.Element.Schema, "");
            return Context.SchemaGeneratorRegistry.Get(schemaElement);
        }
    }
}
