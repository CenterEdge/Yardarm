using System;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Schema;
using Yardarm.Names;

namespace Yardarm.Generation.Api
{
    internal class ResponseSchemaGenerator : IResponseSchemaGenerator
    {
        protected GenerationContext Context { get; }
        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected IHttpResponseCodeNameProvider HttpResponseCodeNameProvider { get; }

        public ResponseSchemaGenerator(GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider)
        {
            Context = context;
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            HttpResponseCodeNameProvider = httpResponseCodeNameProvider ?? throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
        }

        public virtual void Preprocess(LocatedOpenApiElement<OpenApiResponse> element) =>
            GetSchemaGenerator(element)?.Preprocess();

        public virtual TypeSyntax GetTypeName(LocatedOpenApiElement<OpenApiResponse> element)
        {
            OpenApiResponse response = element.Element;
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(element);
            if (mediaType?.Element.Schema?.Type != "object" || mediaType.Element.Schema.Reference != null)
            {
                throw new InvalidOperationException("No valid media type for this request");
            }

            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetResponseNamespace(element);

            if (response.Reference != null)
            {
                // We're in the components section

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(response.Reference.Id + "Response")));
            }
            else
            {
                // We're in an operation

                var operation = element.Parents.OfType<LocatedOpenApiElement<OpenApiOperation>>().First().Element;

                var responseCode = Enum.TryParse<HttpStatusCode>(element.Key, out var statusCode)
                    ? HttpResponseCodeNameProvider.GetName(statusCode)
                    : element.Key;

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format($"{operation.OperationId}{responseCode}Response")));
            }
        }

        public SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiResponse> element) =>
            GetSchemaGenerator(element)?.GenerateSyntaxTree();

        private ITypeGenerator? GetSchemaGenerator(LocatedOpenApiElement<OpenApiResponse> element)
        {
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(element);
            if (mediaType?.Element.Schema?.Type != "object" || mediaType.Element.Schema.Reference != null)
            {
                return null;
            }

            var schemaElement = mediaType.CreateChild(mediaType.Element.Schema, "");
            return Context.SchemaGeneratorRegistry.Get(schemaElement);
        }
    }
}
