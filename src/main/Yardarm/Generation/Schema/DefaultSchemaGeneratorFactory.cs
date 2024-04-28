using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    public class DefaultSchemaGeneratorFactory(GenerationContext context) : ITypeGeneratorFactory<OpenApiSchema>
    {
        public virtual ITypeGenerator Create(ILocatedOpenApiElement<OpenApiSchema> element, ITypeGenerator? parent) =>
            element.Element switch
            {
                { AllOf.Count: > 0 } => new AllOfSchemaGenerator(element, context, parent),
                { OneOf.Count: > 0 } => new OneOfSchemaGenerator(element, context, parent),
                { Type: "object" } => GetObjectGenerator(element, parent),
                { Type: "string" } => GetStringGenerator(element, parent),
                { Type: "number" or "integer" } => GetNumberGenerator(element, parent),
                { Type: "boolean" } => GetBooleanGenerator(element),
                { Type: "array" } => GetArrayGenerator(element, parent),
                _ => new DynamicSchemaGenerator(element, context, parent)
            };

        protected virtual ITypeGenerator GetArrayGenerator(ILocatedOpenApiElement<OpenApiSchema> element, ITypeGenerator? parent) =>
            new ArraySchemaGenerator(element, context, parent);

        protected virtual ITypeGenerator GetBooleanGenerator(ILocatedOpenApiElement<OpenApiSchema> element) =>
            BooleanSchemaGenerator.Instance;

        protected virtual ITypeGenerator GetNumberGenerator(ILocatedOpenApiElement<OpenApiSchema> element, ITypeGenerator? parent) =>
            new NumberSchemaGenerator(element, context, parent);

        protected virtual ITypeGenerator GetObjectGenerator(ILocatedOpenApiElement<OpenApiSchema> element, ITypeGenerator? parent) =>
            new ObjectSchemaGenerator(element, context, parent);

        protected virtual ITypeGenerator GetStringGenerator(ILocatedOpenApiElement<OpenApiSchema> element, ITypeGenerator? parent) =>
            element.Element.Enum is { Count: > 0 }
                ? new EnumSchemaGenerator(element, context, parent)
                : new StringSchemaGenerator(element, context, parent);
    }

    // For the most common case, this allows inlining of calls to the various Get methods without guarded devirtualization by PGO.
    internal sealed class DefaultSchemaGeneratorFactorySealed(GenerationContext context)
        : DefaultSchemaGeneratorFactory(context);
}
