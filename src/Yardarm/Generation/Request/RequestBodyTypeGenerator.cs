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
    public class RequestBodyTypeGenerator : ITypeGenerator
    {
        private TypeSyntax? _nameCache;

        public TypeSyntax TypeName => _nameCache ??= GetTypeName();

        protected ILocatedOpenApiElement<OpenApiRequestBody> RequestBodyElement { get; }
        protected GenerationContext Context { get; }
        protected IMediaTypeSelector MediaTypeSelector { get; }

        protected OpenApiRequestBody RequestBody => RequestBodyElement.Element;

        public RequestBodyTypeGenerator(ILocatedOpenApiElement<OpenApiRequestBody> requestBodyElement,
            GenerationContext context, IMediaTypeSelector mediaTypeSelector)
        {
            RequestBodyElement = requestBodyElement ?? throw new ArgumentNullException(nameof(requestBodyElement));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
        }

        protected virtual TypeSyntax GetTypeName()
        {
            ILocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(RequestBodyElement);
            if (mediaType == null)
            {
                throw new InvalidOperationException("No valid media type for this request");
            }

            ILocatedOpenApiElement<OpenApiSchema>? schemaElement = mediaType.GetSchema();
            if (schemaElement != null &&
                (schemaElement.Element.Type != "object" || schemaElement.Element.Reference != null))
            {
                return Context.SchemaGeneratorRegistry.Get(schemaElement).TypeName;
            }

            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(RequestBodyElement);

            if (RequestBody.Reference != null)
            {
                // We're in the components section

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(RequestBody.Reference.Id + "RequestBody")));
            }
            else
            {
                // We're in an operation

                var operation = RequestBodyElement.Parents().OfType<LocatedOpenApiElement<OpenApiOperation>>().First().Element;

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
            ILocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(RequestBodyElement);

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
