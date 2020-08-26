using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class RequestBodyTypeGenerator : TypeGeneratorBase<OpenApiRequestBody>
    {
        protected IMediaTypeSelector MediaTypeSelector { get; }

        protected OpenApiRequestBody RequestBody => Element.Element;

        public RequestBodyTypeGenerator(ILocatedOpenApiElement<OpenApiRequestBody> requestBodyElement,
            GenerationContext context, IMediaTypeSelector mediaTypeSelector)
            : base(requestBodyElement, context)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
        }

        protected override YardarmTypeInfo GetTypeInfo()
        {
            ILocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(Element);
            if (mediaType == null)
            {
                throw new InvalidOperationException("No valid media type for this request");
            }

            ILocatedOpenApiElement<OpenApiSchema>? schemaElement = mediaType.GetSchema();
            if (schemaElement != null &&
                (schemaElement.Element.Type != "object" || schemaElement.Element.Reference != null))
            {
                return Context.SchemaGeneratorRegistry.Get(schemaElement).TypeInfo;
            }

            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            if (RequestBody.Reference != null)
            {
                // We're in the components section

                TypeSyntax name = SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(RequestBody.Reference.Id + "RequestBody")));

                return new YardarmTypeInfo(name);
            }
            else
            {
                // We're in an operation

                var operation = Element.Parents().OfType<LocatedOpenApiElement<OpenApiOperation>>().First().Element;

                TypeSyntax name = SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(operation.OperationId + "RequestBody")));

                return new YardarmTypeInfo(name);
            }
        }

        public override SyntaxTree? GenerateSyntaxTree() =>
            GetSchemaGenerator()?.GenerateSyntaxTree();

        public override IEnumerable<MemberDeclarationSyntax> Generate() =>
            GetSchemaGenerator()?.Generate() ?? Enumerable.Empty<MemberDeclarationSyntax>();

        private ITypeGenerator? GetSchemaGenerator()
        {
            ILocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(Element);

            ILocatedOpenApiElement<OpenApiSchema>? schemaElement = mediaType?.GetSchema();
            if (schemaElement == null || schemaElement.Element.Type != "object" ||
                schemaElement.Element.Reference != null)
            {
                return null;
            }

            return Context.SchemaGeneratorRegistry.Get(schemaElement);
        }
    }
}
