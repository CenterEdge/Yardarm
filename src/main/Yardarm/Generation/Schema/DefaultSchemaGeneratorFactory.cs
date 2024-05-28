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
                {
                    Type: "object",
                    AdditionalPropertiesAllowed: true,
                    Properties: null or { Count: 0 },
                    AnyOf: null or { Count: 0 } // AllOf and OneOf are handled above, they don't need to be tested here
                } => GetDictionaryGenerator(element, parent),
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

        protected virtual ITypeGenerator GetDictionaryGenerator(ILocatedOpenApiElement<OpenApiSchema> element, ITypeGenerator? parent) =>
            new DictionarySchemaGenerator(element, context, parent);
    }
}
