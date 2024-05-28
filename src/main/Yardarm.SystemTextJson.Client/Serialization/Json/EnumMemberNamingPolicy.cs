using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using System.Text.Json;

namespace RootNamespace.Serialization.Json
{
    // Note: This class should only be used from a source that caches the results,
    // such as the JsonStringEnumConverter<TEnum> class.
    internal sealed class EnumMemberNamingPolicy<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields)] TEnum>
        : JsonNamingPolicy
    {
        public static EnumMemberNamingPolicy<TEnum> Instance { get; } = new();

        override public string ConvertName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var attributes = typeof(TEnum).GetField(name)?.GetCustomAttributes(false);
            if (attributes is not null)
            {
                foreach (object attribute in attributes)
                {
                    if (attribute is EnumMemberAttribute { Value: { Length: > 0 } value })
                    {
                        return value;
                    }
                }
            }

            return name;
        }
    }
}
