using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.Names
{
    public class DefaultElementTypeInfoProvider : IElementTypeInfoProvider
    {
        private readonly ITypeGeneratorRegistry _typeGeneratorRegistry;

        public DefaultElementTypeInfoProvider(ITypeGeneratorRegistry typeGeneratorRegistry)
        {
            _typeGeneratorRegistry = typeGeneratorRegistry ?? throw new ArgumentNullException(nameof(typeGeneratorRegistry));
        }

        public YardarmTypeInfo Get(ILocatedOpenApiElement element)
        {
            return GetNameInternal(element)
                   ?? throw new InvalidOperationException("Element does not have a type name.");
        }

        protected virtual YardarmTypeInfo? GetNameInternal(ILocatedOpenApiElement element) =>
            element switch
            {
                ILocatedOpenApiElement<OpenApiOperation> operationElement => GetOperationName(operationElement),
                ILocatedOpenApiElement<OpenApiRequestBody> requestBodyElement => GetRequestBodyName(requestBodyElement),
                ILocatedOpenApiElement<OpenApiUnknownResponse> responseElement => GetUnknownResponseName(responseElement),
                ILocatedOpenApiElement<OpenApiResponse> responseElement => GetResponseName(responseElement),
                ILocatedOpenApiElement<OpenApiResponses> responsesElement => GetResponsesName(responsesElement),
                ILocatedOpenApiElement<OpenApiSchema> schemaElement => GetSchemaName(schemaElement),
                ILocatedOpenApiElement<OpenApiSecurityScheme> securitySchemeElement => GetSecuritySchemeName(securitySchemeElement),
                ILocatedOpenApiElement<OpenApiTag> tagElement => GetTagName(tagElement),
                _ => element.Parent != null ? GetNameInternal(element.Parent) : null
            };

        protected virtual YardarmTypeInfo GetOperationName(ILocatedOpenApiElement<OpenApiOperation> element) =>
            _typeGeneratorRegistry.Get(element).TypeInfo;

        protected virtual YardarmTypeInfo GetRequestBodyName(ILocatedOpenApiElement<OpenApiRequestBody> element) =>
            _typeGeneratorRegistry.Get(element).TypeInfo;

        protected virtual YardarmTypeInfo GetResponseName(ILocatedOpenApiElement<OpenApiResponse> element) =>
            _typeGeneratorRegistry.Get(element).TypeInfo;

        protected virtual YardarmTypeInfo GetResponsesName(ILocatedOpenApiElement<OpenApiResponses> element) =>
            _typeGeneratorRegistry.Get(element).TypeInfo;

        protected virtual YardarmTypeInfo GetSchemaName(ILocatedOpenApiElement<OpenApiSchema> element) =>
            _typeGeneratorRegistry.Get(element).TypeInfo;

        protected virtual YardarmTypeInfo GetSecuritySchemeName(ILocatedOpenApiElement<OpenApiSecurityScheme> element) =>
            _typeGeneratorRegistry.Get(element).TypeInfo;

        protected virtual YardarmTypeInfo GetTagName(ILocatedOpenApiElement<OpenApiTag> element) =>
            _typeGeneratorRegistry.Get(element).TypeInfo;

        protected virtual YardarmTypeInfo GetUnknownResponseName(ILocatedOpenApiElement<OpenApiUnknownResponse> element) =>
            _typeGeneratorRegistry.Get(element).TypeInfo;
    }
}
