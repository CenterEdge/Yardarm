using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public interface ISchemaGeneratorFactory
    {
        ISchemaGenerator Create(string name, OpenApiSchema schema);
    }
}
