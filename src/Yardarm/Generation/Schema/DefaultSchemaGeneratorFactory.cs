using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    public class DefaultSchemaGeneratorFactory : ITypeGeneratorFactory<OpenApiSchema>
    {
        private readonly Lazy<GenerationContext> _context;

        public DefaultSchemaGeneratorFactory(IServiceProvider serviceProvider)
        {
            // Use lazy initialization to avoid a circular reference within the DI container
            _context = new Lazy<GenerationContext>(serviceProvider.GetRequiredService<GenerationContext>);
        }

        public virtual ITypeGenerator Create(ILocatedOpenApiElement<OpenApiSchema> element)
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
                "integer" => GetNumberGenerator(element),
                "boolean" => GetBooleanGenerator(element),
                "array" => GetArrayGenerator(element),
                _ => NullSchemaGenerator.Instance
            };
        }

        protected virtual ITypeGenerator GetArrayGenerator(ILocatedOpenApiElement<OpenApiSchema> element) =>
            new ArraySchemaGenerator(element, _context.Value);

        protected virtual ITypeGenerator GetBooleanGenerator(ILocatedOpenApiElement<OpenApiSchema> element) =>
            BooleanSchemaGenerator.Instance;

        protected virtual ITypeGenerator GetNumberGenerator(ILocatedOpenApiElement<OpenApiSchema> element) =>
            new NumberSchemaGenerator(element);

        protected virtual ITypeGenerator GetObjectGenerator(ILocatedOpenApiElement<OpenApiSchema> element) =>
            new ObjectSchemaGenerator(element, _context.Value);

        protected virtual ITypeGenerator GetStringGenerator(ILocatedOpenApiElement<OpenApiSchema> element) =>
            element.Element.Enum?.Count > 0
                ? (ITypeGenerator) new EnumSchemaGenerator(element, _context.Value)
                : new StringSchemaGenerator(element);
    }
}
