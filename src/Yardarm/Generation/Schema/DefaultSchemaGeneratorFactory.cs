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

        public virtual ISchemaGenerator Create(string name, OpenApiSchema schema) =>
            schema.Type switch
            {
                "object" => _serviceProvider.GetRequiredService<ObjectSchemaGenerator>(),
                _ => NullSchemaGenerator.Instance
            };
    }
}
