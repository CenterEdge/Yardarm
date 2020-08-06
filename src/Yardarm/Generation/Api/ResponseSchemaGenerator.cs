using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation.Schema;
using Yardarm.Names;

namespace Yardarm.Generation.Api
{
    internal class ResponseSchemaGenerator : IResponseSchemaGenerator
    {
        private readonly INamespaceProvider _namespaceProvider;
        private readonly INameFormatterSelector _nameFormatterSelector;
        private readonly ISchemaGeneratorRegistry _schemaGeneratorRegistry;
        private readonly IMediaTypeSelector _mediaTypeSelector;
        private readonly IHttpResponseCodeNameProvider _httpResponseCodeNameProvider;

        protected IList<ISchemaClassEnricher> ClassEnrichers { get; }
        protected IList<IPropertyEnricher> PropertyEnrichers { get; }

        public ResponseSchemaGenerator(INamespaceProvider namespaceProvider, INameFormatterSelector nameFormatterSelector,
            ISchemaGeneratorRegistry schemaGeneratorRegistry, IMediaTypeSelector mediaTypeSelector,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider,
            IEnumerable<ISchemaClassEnricher> classEnrichers, IEnumerable<IPropertyEnricher> propertyEnrichers)
        {
            _namespaceProvider = namespaceProvider ?? throw new ArgumentNullException(nameof(namespaceProvider));
            _nameFormatterSelector = nameFormatterSelector ?? throw new ArgumentNullException(nameof(nameFormatterSelector));
            _schemaGeneratorRegistry = schemaGeneratorRegistry ?? throw new ArgumentNullException(nameof(schemaGeneratorRegistry));
            _mediaTypeSelector = mediaTypeSelector;
            _httpResponseCodeNameProvider = httpResponseCodeNameProvider ?? throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));
            ClassEnrichers = classEnrichers.ToArray();
            PropertyEnrichers = propertyEnrichers.ToArray();
        }

        public virtual void Preprocess(LocatedOpenApiElement<OpenApiResponse> element) =>
            GetSchemaGenerator(element)?.Preprocess();

        public virtual TypeSyntax GetTypeName(LocatedOpenApiElement<OpenApiResponse> element)
        {
            OpenApiResponse response = element.Element;
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = _mediaTypeSelector.Select(element);
            if (mediaType?.Element.Schema?.Type != "object" || mediaType.Element.Schema.Reference != null)
            {
                throw new InvalidOperationException("No valid media type for this request");
            }

            INameFormatter formatter = _nameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = _namespaceProvider.GetResponseNamespace(element);

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
                    ? _httpResponseCodeNameProvider.GetName(statusCode)
                    : element.Key;

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format($"{operation.OperationId}{responseCode}Response")));
            }
        }

        public SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiResponse> element) =>
            GetSchemaGenerator(element)?.GenerateSyntaxTree();

        private ISchemaGenerator? GetSchemaGenerator(LocatedOpenApiElement<OpenApiResponse> element)
        {
            LocatedOpenApiElement<OpenApiMediaType>? mediaType = _mediaTypeSelector.Select(element);
            if (mediaType?.Element.Schema?.Type != "object" || mediaType.Element.Schema.Reference != null)
            {
                return null;
            }

            var schemaElement = mediaType.CreateChild(mediaType.Element.Schema, "");
            return _schemaGeneratorRegistry.Get(schemaElement);
        }
    }
}
