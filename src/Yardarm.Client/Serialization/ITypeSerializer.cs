using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    public interface ITypeSerializer
    {
        HttpContent Serialize<T>(T value, string mediaType);

        string Serialize<T>(T value);

        ValueTask<T> DeserializeAsync<T>(HttpContent content);

        [return: MaybeNull]
        T Deserialize<T>(IEnumerable<string> values);
    }
}
