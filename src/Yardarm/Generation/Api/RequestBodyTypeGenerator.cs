using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;

namespace Yardarm.Generation.Api
{
    public class RequestBodyTypeGenerator : ITypeGenerator
    {
        protected LocatedOpenApiElement<OpenApiRequestBody> RequestBodyElement { get; }
        protected GenerationContext Context { get; }
        protected IMediaTypeSelector MediaTypeSelector { get; }

        protected OpenApiRequestBody RequestBody => RequestBodyElement.Element;

        public RequestBodyTypeGenerator(LocatedOpenApiElement<OpenApiRequestBody> requestBodyElement,
            GenerationContext context, IMediaTypeSelector mediaTypeSelector)
        {
            RequestBodyElement = requestBodyElement ?? throw new ArgumentNullException(nameof(requestBodyElement));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
        }

        public virtual void Preprocess() =>
            GetSchemaGenerator()?.Preprocess();

        public virtual TypeSyntax GetTypeName()
        {
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(RequestBodyElement);
            if (mediaType == null)
            {
                throw new InvalidOperationException("No valid media type for this request");
            }
            else if (mediaType?.Element.Schema?.Type != "object" || mediaType?.Element.Schema.Reference != null)
            {
                var schemaElement = mediaType!.CreateChild(mediaType.Element.Schema!, "");
                return Context.SchemaGeneratorRegistry.Get(schemaElement).GetTypeName();
            }

            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetRequestBodyNamespace(RequestBodyElement);

            if (RequestBody.Reference != null)
            {
                // We're in the components section

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(RequestBody.Reference.Id + "RequestBody")));
            }
            else
            {
                // We're in an operation

                var operation = RequestBodyElement.Parents.OfType<LocatedOpenApiElement<OpenApiOperation>>().First().Element;

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(operation.OperationId + "RequestBody")));
            }
        }

        public SyntaxTree? GenerateSyntaxTree() =>
            GetSchemaGenerator()?.GenerateSyntaxTree();

        public IEnumerable<MemberDeclarationSyntax> Generate() =>
            GetSchemaGenerator()?.Generate() ?? Enumerable.Empty<MemberDeclarationSyntax>();

        private ITypeGenerator? GetSchemaGenerator()
        {
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(RequestBodyElement);
            if (mediaType?.Element.Schema?.Type != "object" || mediaType.Element.Schema.Reference != null)
            {
                return null;
            }

            var schemaElement = mediaType.CreateChild(mediaType.Element.Schema, "");
            return Context.SchemaGeneratorRegistry.Get(schemaElement);
        }
    }
}
