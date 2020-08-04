using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public interface ISchemaGeneratorRegistry
    {
        public ISchemaGenerator Get(LocatedOpenApiElement<OpenApiSchema> schemaElement);
    }
}
