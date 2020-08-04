using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public interface ISchemaGeneratorFactory
    {
        ISchemaGenerator Create(LocatedOpenApiElement<OpenApiSchema> schema);
    }
}
