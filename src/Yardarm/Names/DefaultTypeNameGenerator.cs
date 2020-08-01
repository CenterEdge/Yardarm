using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Generation.Api;
using Yardarm.Generation.Schema;

namespace Yardarm.Names
{
    public class DefaultTypeNameGenerator : ITypeNameGenerator
    {
        private readonly ISchemaGeneratorFactory _schemaGeneratorFactory;
        private readonly IRequestBodySchemaGenerator _requestBodyGenerator;

        public DefaultTypeNameGenerator(ISchemaGeneratorFactory schemaGeneratorFactory, IRequestBodySchemaGenerator requestBodyGenerator)
        {
            _schemaGeneratorFactory = schemaGeneratorFactory ?? throw new ArgumentNullException(nameof(schemaGeneratorFactory));
            _requestBodyGenerator = requestBodyGenerator ?? throw new ArgumentNullException(nameof(requestBodyGenerator));
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
                LocatedOpenApiElement<OpenApiSchema> schemaElement => GetSchemaName(schemaElement),
                _ => element.Parents.Count > 0 ? GetNameInternal(element.Parents[0]) : null
            };

        protected virtual TypeSyntax GetRequestBodyName(LocatedOpenApiElement<OpenApiRequestBody> element) =>
            _requestBodyGenerator.GetTypeName(element);

        protected virtual TypeSyntax GetSchemaName(LocatedOpenApiElement<OpenApiSchema> element) =>
            _schemaGeneratorFactory.Get(element).GetTypeName(element);
    }
}
