using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public class DefaultSchemaGeneratorFactory : ISchemaGeneratorFactory
    {
        private readonly Lazy<GenerationContext> _context;

        public DefaultSchemaGeneratorFactory(IServiceProvider serviceProvider)
        {
            // Use lazy initialization to avoid a circular reference within the DI container
            _context = new Lazy<GenerationContext>(serviceProvider.GetRequiredService<GenerationContext>);
        }

        public virtual ISchemaGenerator Create(LocatedOpenApiElement<OpenApiSchema> element)
        {
            OpenApiSchema schema = element.Element;

            if (schema.AllOf.Count > 0)
            {
                return new AllOfSchemaGenerator(element, _context.Value);
            }

            if (schema.OneOf.Count > 0)
            {
                return new OneOfSchemaGenerator(element, _context.Value);
            }

            return schema.Type switch
            {
                "object" => GetObjectGenerator(element),
                "string" => GetStringGenerator(element),
                "number" => GetNumberGenerator(element),
                "boolean" => GetBooleanGenerator(element),
                "array" => GetArrayGenerator(element),
                _ => NullSchemaGenerator.Instance
            };
        }

        protected virtual ISchemaGenerator GetArrayGenerator(LocatedOpenApiElement<OpenApiSchema> element) =>
            new ArraySchemaGenerator(element, _context.Value);

        protected virtual ISchemaGenerator GetBooleanGenerator(LocatedOpenApiElement<OpenApiSchema> element) =>
            BooleanSchemaGenerator.Instance;

        protected virtual ISchemaGenerator GetNumberGenerator(LocatedOpenApiElement<OpenApiSchema> element) =>
            NumberSchemaGenerator.Instance;

        protected virtual ISchemaGenerator GetObjectGenerator(LocatedOpenApiElement<OpenApiSchema> element) =>
            new ObjectSchemaGenerator(element, _context.Value);

        protected virtual ISchemaGenerator GetStringGenerator(LocatedOpenApiElement<OpenApiSchema> element) =>
            element.Element.Enum?.Count > 0
                ? (ISchemaGenerator) new EnumSchemaGenerator(element, _context.Value)
                : StringSchemaGenerator.Instance;
    }
}
