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
        /// </summary>
        private BaseOpenApiReference? GetBaseReference()
        {
            return element is IOpenApiReferenceHolder referenceHolder
                ? OpenApiReferenceHolderAccessor.GetReference(referenceHolder)
                : null;
        }
    }
}
