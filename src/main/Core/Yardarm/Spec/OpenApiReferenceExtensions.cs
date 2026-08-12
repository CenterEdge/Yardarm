using Microsoft.OpenApi;

namespace Yardarm.Spec;

/// <summary>
/// Extension methods for working with the new reference proxy model in Microsoft.OpenApi 3.x.
/// Reference proxies (e.g. OpenApiSchemaReference) implement IOpenApiReferenceHolder and
/// transparently delegate to their Target.
/// </summary>
public static class OpenApiReferenceExtensions
{
    extension(IOpenApiElement element)
    {
        /// <summary>
        /// Gets the reference ID from an element if it is a reference holder, or null if not a reference.
        /// </summary>
        public string? GetReferenceId() =>
            element.GetBaseReference()?.Id;

        /// <summary>
        /// Gets the ReferenceV3 string from an element if it is a reference holder, or null if not a reference.
        /// </summary>
        public string? GetReferenceV3() =>
            element.GetBaseReference()?.ReferenceV3;

        /// <summary>
        /// Gets the BaseOpenApiReference from an element if it is a reference holder.
        /// Uses reflection to access the Reference property since the generic interface
        /// requires knowing the concrete reference type at compile time.
        /// </summary>
        public BaseOpenApiReference? GetBaseReference()
        {
            if (element is not IOpenApiReferenceHolder)
            {
                return null;
            }

            // TODO: Redesign this to avoid reflection when retrieving the reference.
            // All reference holders in Microsoft.OpenApi derive from BaseOpenApiReferenceHolder<T,U,V>
            // which has a public property Reference of type V : BaseOpenApiReference.
            var refProp = element.GetType().GetProperty("Reference");
            return refProp?.GetValue(element) as BaseOpenApiReference;
        }
    }
}
