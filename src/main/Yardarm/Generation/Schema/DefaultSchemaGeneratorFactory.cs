using Microsoft.OpenApi;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    public class DefaultSchemaGeneratorFactory(GenerationContext context) : ITypeGeneratorFactory<IOpenApiSchema>
    {
        public virtual ITypeGenerator Create(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
            element.Element switch
            {
                { AllOf.Count: > 0 } => new AllOfSchemaGenerator(element, context, parent),
                { OneOf.Count: > 0 } => new OneOfSchemaGenerator(element, context, parent),
                {
                    AdditionalPropertiesAllowed: true,
                    Properties: null or { Count: 0 },
                    AnyOf: null or { Count: 0 } // AllOf and OneOf are handled above, they don't need to be tested here
                } when element.Element.HasType(JsonSchemaType.Object) => GetDictionaryGenerator(element, parent),
                _ when element.Element.HasType(JsonSchemaType.Object) => GetObjectGenerator(element, parent),
                _ when element.Element.HasType(JsonSchemaType.String) => GetStringGenerator(element, parent),
                _ when element.Element.HasType(JsonSchemaType.Number) || element.Element.HasType(JsonSchemaType.Integer) => GetNumberGenerator(element, parent),
                _ when element.Element.HasType(JsonSchemaType.Boolean) => GetBooleanGenerator(element),
                _ when element.Element.HasType(JsonSchemaType.Array) => GetArrayGenerator(element, parent),
                _ => new DynamicSchemaGenerator(element, context, parent)
            };

        protected virtual ITypeGenerator GetArrayGenerator(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
            new ArraySchemaGenerator(element, context, parent);

        protected virtual ITypeGenerator GetBooleanGenerator(ILocatedOpenApiElement<IOpenApiSchema> element) =>
            BooleanSchemaGenerator.Instance;

        protected virtual ITypeGenerator GetNumberGenerator(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
            new NumberSchemaGenerator(element, context, parent);

        protected virtual ITypeGenerator GetObjectGenerator(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
            new ObjectSchemaGenerator(element, context, parent);

        protected virtual ITypeGenerator GetStringGenerator(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
            element.Element.Enum is { Count: > 0 }
                ? new EnumSchemaGenerator(element, context, parent)
                : new StringSchemaGenerator(element, context, parent);

        protected virtual ITypeGenerator GetDictionaryGenerator(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
            new DictionarySchemaGenerator(element, context, parent);
    }
}
