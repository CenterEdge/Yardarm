using System;
using Microsoft.OpenApi.Models;
using Yardarm.Names;

namespace Yardarm.Generation.Schema
{
    public class DefaultSchemaGeneratorFactory : ISchemaGeneratorFactory
    {
        private readonly INameFormatterSelector _nameFormatterSelector;

        public DefaultSchemaGeneratorFactory(INameFormatterSelector nameFormatterSelector)
        {
            _nameFormatterSelector = nameFormatterSelector ?? throw new ArgumentNullException(nameof(nameFormatterSelector));
        }

        public virtual ISchemaGenerator Create(string name, OpenApiSchema schema) =>
            schema.Type switch
            {
                "object" => new ObjectSchemaGenerator(_nameFormatterSelector),
                _ => NullSchemaGenerator.Instance
            };
    }
}
