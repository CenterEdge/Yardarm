using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Schema;
using Yardarm.Names;

namespace Yardarm.Generation.Api
{
    public class RequestBodySchemaGenerator : IRequestBodySchemaGenerator
    {
        protected GenerationContext Context { get; }
        protected IMediaTypeSelector MediaTypeSelector { get; }

        public RequestBodySchemaGenerator(GenerationContext context, IMediaTypeSelector mediaTypeSelector)
        {
            Context = context;
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
        }

        public virtual void Preprocess(LocatedOpenApiElement<OpenApiRequestBody> element) =>
            GetSchemaGenerator(element)?.Preprocess();

        public virtual TypeSyntax GetTypeName(LocatedOpenApiElement<OpenApiRequestBody> element)
        {
            OpenApiRequestBody requestBody = element.Element;
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(element);
            if (mediaType?.Element.Schema?.Type != "object" || mediaType.Element.Schema.Reference != null)
            {
                throw new InvalidOperationException("No valid media type for this request");
            }

            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetRequestBodyNamespace(element);

            if (requestBody.Reference != null)
            {
                // We're in the components section

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(requestBody.Reference.Id + "RequestBody")));
            }
            else
            {
                // We're in an operation

                var operation = element.Parents.OfType<LocatedOpenApiElement<OpenApiOperation>>().First().Element;

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(operation.OperationId + "RequestBody")));
            }
        }

        public SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiRequestBody> element) =>
            GetSchemaGenerator(element)?.GenerateSyntaxTree();

        private ISchemaGenerator? GetSchemaGenerator(LocatedOpenApiElement<OpenApiRequestBody> element)
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
