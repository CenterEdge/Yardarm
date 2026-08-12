using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema;

public class DefaultSchemaGeneratorFactory(GenerationContext context) : ITypeGeneratorFactory<IOpenApiSchema>
{
    private ObjectFactory<ExternallyDiscriminatedUnionSchemaGenerator> ExternallyDiscriminatedUnionFactory => field ??=
        ActivatorUtilities.CreateFactory<ExternallyDiscriminatedUnionSchemaGenerator>([ typeof(ILocatedOpenApiElement<IOpenApiSchema>), typeof(GenerationContext), typeof(ITypeGenerator) ]);

    private ObjectFactory<ExtensibleEnumSchemaGenerator> ExtensibleEnumFactory => field ??=
        ActivatorUtilities.CreateFactory<ExtensibleEnumSchemaGenerator>([typeof(ILocatedOpenApiElement<IOpenApiSchema>), typeof(ITypeGenerator), typeof(List<string>)]);

    public virtual ITypeGenerator Create(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent)
    {
        if (context.Options.ExternallyDiscriminatedUnions
            && ExternallyDiscriminatedUnionSchemaGenerator.IsEligible(element, context.TypeGeneratorRegistry))
        {
            return GetExternallyDiscriminatedUnionGenerator(element, parent);
        }

        if (element.Element.AllOf is { Count: > 0 })
        {
            return new AllOfSchemaGenerator(element, context, parent);
        }

        if (element.Element.OneOf is { Count: > 0 })
        {
            return new OneOfSchemaGenerator(element, context, parent);
        }

        return element.Element switch
        {
            _ when element.Element.HasType(JsonSchemaType.Object)
                   && element.Element.AdditionalPropertiesAllowed
                   && (element.Element.Properties is null or { Count: 0 })
                   && (element.Element.AnyOf is null or { Count: 0 }) => GetDictionaryGenerator(element, parent),
            _ when element.Element.HasType(JsonSchemaType.Object) => GetObjectGenerator(element, parent),
            _ when element.Element.HasType(JsonSchemaType.String) => GetStringGenerator(element, parent),
            _ when element.Element.HasType(JsonSchemaType.Number) || element.Element.HasType(JsonSchemaType.Integer) => GetNumberGenerator(element, parent),
            _ when element.Element.HasType(JsonSchemaType.Boolean) => GetBooleanGenerator(element),
            _ when element.Element.HasType(JsonSchemaType.Array) => GetArrayGenerator(element, parent),
            _ => new DynamicSchemaGenerator(element, context, parent)
        };
    }

    protected virtual ITypeGenerator GetArrayGenerator(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
        new ArraySchemaGenerator(element, context, parent);

    protected virtual ITypeGenerator GetBooleanGenerator(ILocatedOpenApiElement<IOpenApiSchema> element) =>
        BooleanSchemaGenerator.Instance;

    protected virtual ITypeGenerator GetNumberGenerator(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
        new NumberSchemaGenerator(element, context, parent);

    protected virtual ITypeGenerator GetObjectGenerator(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
        new ObjectSchemaGenerator(element, context, parent);

    protected virtual ITypeGenerator GetExternallyDiscriminatedUnionGenerator(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
        ExternallyDiscriminatedUnionFactory(context.GenerationServices, [element, context, parent]);

    protected virtual ITypeGenerator GetStringGenerator(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent)
    {
        if (element.Element.Enum is { Count: > 0 })
        {
            return new EnumSchemaGenerator(element, context, parent);
        }

        if (element.Element.Extensions.TryGetValue("x-extensible-enum", out IOpenApiExtension? extension)
            && extension is JsonNodeExtension { Node: JsonArray { Count: > 0 } array })
        {
            List<string> values = [.. array.OfType<JsonValue>().Select(p => p.GetValue<string>())];

            if (values.Count > 0)
            {
                return ExtensibleEnumFactory.Invoke(context.GenerationServices, [element, parent, values]);
            }
        }

        return new StringSchemaGenerator(element, context, parent);
    }

    protected virtual ITypeGenerator GetDictionaryGenerator(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
        new DictionarySchemaGenerator(element, context, parent);
}
