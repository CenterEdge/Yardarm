using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public class DefaultSchemaGeneratorFactory : ISchemaGeneratorFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public DefaultSchemaGeneratorFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public virtual ISchemaGenerator Get(LocatedOpenApiElement<OpenApiSchema> element)
        {
            OpenApiSchema schema = element.Element;

            if (schema.AllOf.Count > 0)
            {
                return _serviceProvider.GetRequiredService<AllOfSchemaGenerator>();
            }

            if (schema.OneOf.Count > 0)
            {
                return _serviceProvider.GetRequiredService<OneOfSchemaGenerator>();
            }

            return schema.Type switch
            {
                "object" => _serviceProvider.GetRequiredService<ObjectSchemaGenerator>(),
                "string" => GetStringGenerator(element),
                "number" => _serviceProvider.GetRequiredService<NumberSchemaGenerator>(),
                "boolean" => _serviceProvider.GetRequiredService<BooleanSchemaGenerator>(),
                "array" => _serviceProvider.GetRequiredService<ArraySchemaGenerator>(),
                _ => NullSchemaGenerator.Instance
            };
        }

        public virtual ISchemaGenerator GetStringGenerator(LocatedOpenApiElement<OpenApiSchema> element) =>
            element.Element.Enum?.Count > 0
                ? _serviceProvider.GetRequiredService<EnumSchemaGenerator>()
                : (ISchemaGenerator) _serviceProvider.GetRequiredService<StringSchemaGenerator>();
    }
}
