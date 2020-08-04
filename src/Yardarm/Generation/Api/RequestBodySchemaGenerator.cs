using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation.Schema;
using Yardarm.Names;

namespace Yardarm.Generation.Api
{
    internal class RequestBodySchemaGenerator : IRequestBodySchemaGenerator
    {
        private readonly INamespaceProvider _namespaceProvider;
        private readonly INameFormatterSelector _nameFormatterSelector;
        private readonly ISchemaGeneratorRegistry _schemaGeneratorRegistry;
        private readonly IMediaTypeSelector _mediaTypeSelector;

        protected IList<ISchemaClassEnricher> ClassEnrichers { get; }
        protected IList<IPropertyEnricher> PropertyEnrichers { get; }

        public RequestBodySchemaGenerator(INamespaceProvider namespaceProvider, INameFormatterSelector nameFormatterSelector,
            ISchemaGeneratorRegistry schemaGeneratorRegistry, IMediaTypeSelector mediaTypeSelector,
            IEnumerable<ISchemaClassEnricher> classEnrichers, IEnumerable<IPropertyEnricher> propertyEnrichers)
        {
            _namespaceProvider = namespaceProvider ?? throw new ArgumentNullException(nameof(namespaceProvider));
            _nameFormatterSelector = nameFormatterSelector ?? throw new ArgumentNullException(nameof(nameFormatterSelector));
            _schemaGeneratorRegistry = schemaGeneratorRegistry ?? throw new ArgumentNullException(nameof(schemaGeneratorRegistry));
            _mediaTypeSelector = mediaTypeSelector;
            ClassEnrichers = classEnrichers.ToArray();
            PropertyEnrichers = propertyEnrichers.ToArray();
        }

        public virtual TypeSyntax GetTypeName(LocatedOpenApiElement<OpenApiRequestBody> element)
        {
            OpenApiRequestBody requestBody = element.Element;
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = _mediaTypeSelector.Select(element);
            if (mediaType?.Element.Schema?.Type != "object" || mediaType.Element.Schema.Reference != null)
            {
                throw new InvalidOperationException("No valid media type for this request");
            }

            INameFormatter formatter = _nameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = _namespaceProvider.GetRequestBodyNamespace(element);

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

        public SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiRequestBody> element)
        {
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = _mediaTypeSelector.Select(element);
            if (mediaType?.Element.Schema?.Type != "object" || mediaType.Element.Schema.Reference != null)
            {
                return null;
            }

            var schemaElement = mediaType.CreateChild(mediaType.Element.Schema, "");
            var schemaGenerator = _schemaGeneratorRegistry.Get(schemaElement);

            return schemaGenerator.GenerateSyntaxTree();
        }
    }
}
