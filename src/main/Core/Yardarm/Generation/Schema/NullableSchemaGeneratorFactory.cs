using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema;

internal sealed class NullableSchemaGeneratorFactory(GenerationContext context) : ITypeGeneratorFactory<IOpenApiSchema>
{
    public int Priority => -100;

    public ITypeGenerator? Create(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
        TryCreate(context, element, parent);

    internal static ITypeGenerator? TryCreate(
        GenerationContext context,
        ILocatedOpenApiElement<IOpenApiSchema> element,
        ITypeGenerator? parent)
    {
        if (!element.Element.TryGetNullableUnderlyingSchema(out var underlyingSchema))
        {
            return null;
        }

        ITypeGenerator underlyingGenerator = context.TypeGeneratorRegistry.Get(
            new LocatedOpenApiElement<IOpenApiSchema>(underlyingSchema, element.Key, element.Parent));

        // Component references are generated independently; inline schemas must generate their declarations here.
        return underlyingSchema is IOpenApiReferenceHolder
            ? new TypeInfoAliasGenerator(underlyingGenerator, parent)
            : underlyingGenerator;
    }
}
