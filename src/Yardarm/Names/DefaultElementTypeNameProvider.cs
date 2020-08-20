using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.Names
{
    public class DefaultElementTypeNameProvider : IElementTypeNameProvider
    {
        private readonly ITypeGeneratorRegistry _typeGeneratorRegistry;

        public DefaultElementTypeNameProvider(ITypeGeneratorRegistry typeGeneratorRegistry)
        {
            _typeGeneratorRegistry = typeGeneratorRegistry ?? throw new ArgumentNullException(nameof(typeGeneratorRegistry));
        }

        public TypeSyntax GetName(ILocatedOpenApiElement element)
        {
            return GetNameInternal(element)
                   ?? throw new InvalidOperationException("Element does not have a type name.");
        }

        protected virtual TypeSyntax? GetNameInternal(ILocatedOpenApiElement element) =>
            element switch
            {
                ILocatedOpenApiElement<OpenApiOperation> operationElement => GetOperationName(operationElement),
                ILocatedOpenApiElement<OpenApiRequestBody> requestBodyElement => GetRequestBodyName(requestBodyElement),
                ILocatedOpenApiElement<OpenApiUnknownResponse> responseElement => GetUnknownResponseName(responseElement),
                ILocatedOpenApiElement<OpenApiResponse> responseElement => GetResponseName(responseElement),
                ILocatedOpenApiElement<OpenApiResponses> responsesElement => GetResponsesName(responsesElement),
                ILocatedOpenApiElement<OpenApiSchema> schemaElement => GetSchemaName(schemaElement),
                ILocatedOpenApiElement<OpenApiTag> tagElement => GetTagName(tagElement),
                _ => element.Parents.Count > 0 ? GetNameInternal(element.Parents[0]) : null
            };

        protected virtual TypeSyntax GetOperationName(ILocatedOpenApiElement<OpenApiOperation> element) =>
            _typeGeneratorRegistry.Get(element).TypeName;

        protected virtual TypeSyntax GetRequestBodyName(ILocatedOpenApiElement<OpenApiRequestBody> element) =>
            _typeGeneratorRegistry.Get(element).TypeName;

        protected virtual TypeSyntax GetResponseName(ILocatedOpenApiElement<OpenApiResponse> element) =>
            _typeGeneratorRegistry.Get(element).TypeName;

        protected virtual TypeSyntax GetResponsesName(ILocatedOpenApiElement<OpenApiResponses> element) =>
            _typeGeneratorRegistry.Get(element).TypeName;

        protected virtual TypeSyntax GetSchemaName(ILocatedOpenApiElement<OpenApiSchema> element) =>
            _typeGeneratorRegistry.Get(element).TypeName;

        protected virtual TypeSyntax GetTagName(ILocatedOpenApiElement<OpenApiTag> element) =>
            _typeGeneratorRegistry.Get(element).TypeName;

        protected virtual TypeSyntax GetUnknownResponseName(ILocatedOpenApiElement<OpenApiUnknownResponse> element) =>
            _typeGeneratorRegistry.Get(element).TypeName;
    }
}
