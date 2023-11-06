using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public interface ITypeSerializer
    {
        HttpContent Serialize<T>(T value, string mediaType, ISerializationData? serializationData = null);

        // Retained for backward compatibility of the public API surface in the generated SDK. The overload
        // with a CancellationToken simply forwards to this implementation by default if not explicitly implemented.
        // Implementations which support cancellation should implement the overload with a CancellationToken and
        // forward calls from this method to that overload. Note that .NET Standard 2.0 does not support default
        // interface implementations, so this is a breaking change for SDKs targeting .NET Standard 2.0 which must
        // implement both variants.
        ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData);

        ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData? serializationData = null,
            // ReSharper disable once MethodOverloadWithOptionalParameter
            CancellationToken cancellationToken = default)
#if NETCOREAPP3_1_OR_GREATER
        {
            // We can only provide a default implementation for .NET 6 and later
            return DeserializeAsync<T>(content, serializationData);
        }
#else
        ;
#endif
    }
}
