using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.NodaTime.Internal;

public sealed class NodaTimeSchemaGeneratorFactory(GenerationContext context) : ITypeGeneratorFactory<OpenApiSchema>
{
    public ITypeGenerator? Create(ILocatedOpenApiElement<OpenApiSchema> element, ITypeGenerator? parent)
    {
        if (element.Element is { Type: "string", Format: not null }
            && NodaTimeSchemaGenerator.SupportedFormats.Contains(element.Element.Format))
        {
            return new NodaTimeSchemaGenerator(element, context, parent);
        }

        return null;
    }
}
