using Microsoft.OpenApi.Interfaces;

namespace Yardarm.Generation
{
    public struct OpenApiPathElement
    {
        public IOpenApiReferenceable Parent { get; set; }
        public string Key { get; set; }

        public OpenApiPathElement(IOpenApiReferenceable parent, string key)
        {
            Parent = parent;
            Key = key;
        }
    }
}
