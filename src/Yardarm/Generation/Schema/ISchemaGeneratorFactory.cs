using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public interface ISchemaGeneratorFactory
    {
        ISchemaGenerator Get(LocatedOpenApiElement<OpenApiSchema> schema);
    }
}
