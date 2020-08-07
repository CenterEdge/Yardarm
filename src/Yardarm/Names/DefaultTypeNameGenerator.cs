using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Generation.Api;

namespace Yardarm.Names
{
    public class DefaultTypeNameGenerator : ITypeNameGenerator
    {
        private readonly ITypeGeneratorRegistry<OpenApiSchema> _typeGeneratorRegistry;
        private readonly IRequestBodySchemaGenerator _requestBodySchemaGenerator;
        private readonly IResponseSchemaGenerator _responseSchemaGenerator;

        public DefaultTypeNameGenerator(ITypeGeneratorRegistry<OpenApiSchema> typeGeneratorRegistry, IRequestBodySchemaGenerator requestBodySchemaGenerator,
            IResponseSchemaGenerator responseSchemaGenerator)
        {
            _typeGeneratorRegistry = typeGeneratorRegistry ?? throw new ArgumentNullException(nameof(typeGeneratorRegistry));
            _requestBodySchemaGenerator = requestBodySchemaGenerator ?? throw new ArgumentNullException(nameof(requestBodySchemaGenerator));
            _responseSchemaGenerator = responseSchemaGenerator ?? throw new ArgumentNullException(nameof(responseSchemaGenerator));
        }

        public TypeSyntax GetName(LocatedOpenApiElement element)
        {
            return GetNameInternal(element)
                         ?? throw new InvalidOperationException("Element does not have a type name.");
        }

        protected virtual TypeSyntax? GetNameInternal(LocatedOpenApiElement element) =>
            element switch
            {
                LocatedOpenApiElement<OpenApiRequestBody> requestBodyElement => GetRequestBodyName(requestBodyElement),
                LocatedOpenApiElement<OpenApiResponse> responseElement => GetResponseName(responseElement),
                LocatedOpenApiElement<OpenApiSchema> schemaElement => GetSchemaName(schemaElement),
                _ => element.Parents.Count > 0 ? GetNameInternal(element.Parents[0]) : null
            };

        protected virtual TypeSyntax GetRequestBodyName(LocatedOpenApiElement<OpenApiRequestBody> element) =>
            _requestBodySchemaGenerator.GetTypeName(element);

        protected virtual TypeSyntax GetResponseName(LocatedOpenApiElement<OpenApiResponse> element) =>
            _responseSchemaGenerator.GetTypeName(element);

        protected virtual TypeSyntax GetSchemaName(LocatedOpenApiElement<OpenApiSchema> element) =>
            _typeGeneratorRegistry.Get(element).GetTypeName();
    }
}
