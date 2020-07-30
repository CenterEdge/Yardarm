using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public interface ISchemaGenerator
    {
        SyntaxTree? Generate(string name, OpenApiSchema schema);
    }
}
