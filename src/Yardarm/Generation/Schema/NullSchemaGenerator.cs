using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Models;

namespace Yardarm.Generation.Schema
{
    public class NullSchemaGenerator : ISchemaGenerator
    {
        public static NullSchemaGenerator Instance { get; } = new NullSchemaGenerator();

        private NullSchemaGenerator()
        {
        }

        public SyntaxTree? Generate(string name, OpenApiSchema schema) => null;
    }
}
