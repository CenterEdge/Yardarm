using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Generation.Schema;

namespace Yardarm.Names
{
    public class DefaultTypeNameGenerator : ITypeNameGenerator
    {
        private readonly ISchemaGeneratorFactory _schemaGeneratorFactory;

        public DefaultTypeNameGenerator(ISchemaGeneratorFactory schemaGeneratorFactory)
        {
            _schemaGeneratorFactory = schemaGeneratorFactory ?? throw new ArgumentNullException(nameof(schemaGeneratorFactory));
        }

        public virtual TypeSyntax GetName(LocatedOpenApiElement element) =>
            element switch
            {
                LocatedOpenApiElement<OpenApiSchema> schemaElement => GetSchemaName(schemaElement),
                _ => throw new InvalidOperationException($"Invalid component type {element.Element.GetType().FullName}")
            };

        protected virtual TypeSyntax GetSchemaName(LocatedOpenApiElement<OpenApiSchema> element) =>
            _schemaGeneratorFactory.Get(element).GetTypeName(element);
    }
}
