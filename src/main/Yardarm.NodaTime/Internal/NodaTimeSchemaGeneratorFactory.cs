using Microsoft.OpenApi;
using Yardarm.Generation;
using Yardarm.Spec;

namespace Yardarm.NodaTime.Internal;

public sealed class NodaTimeSchemaGeneratorFactory(GenerationContext context) : ITypeGeneratorFactory<IOpenApiSchema>
{
    public ITypeGenerator? Create(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent)
    {
        if (element.Element is { Format: not null } && element.Element.HasType(JsonSchemaType.String)
            && NodaTimeSchemaGenerator.SupportedFormats.Contains(element.Element.Format))
        {
            return new NodaTimeSchemaGenerator(element, context, parent);
        }

        return null;
    }
}
