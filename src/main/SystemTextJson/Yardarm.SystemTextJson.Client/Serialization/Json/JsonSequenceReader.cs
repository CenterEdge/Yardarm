using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization.Json;

/// <summary>
/// Reads a sequence of JSON values from <see cref="HttpContent"/> as they are enumerated.
/// </summary>
internal static class JsonSequenceReader
{
    /// <summary>
    /// Reads a sequence of JSON values.
    /// </summary>
    /// <param name="content">The content to read.</param>
    /// <param name="typeInfo">The metadata for each item.</param>
    /// <param name="topLevelValues"><c>true</c> to read whitespace-separated top-level values, such as JSON Lines,
    /// or <c>false</c> to read the elements of a root JSON array.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    public static async IAsyncEnumerable<TItem> ReadAsync<TItem>(HttpContent content, JsonTypeInfo<TItem> typeInfo,
        bool topLevelValues, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
#if NET5_0_OR_GREATER
        Stream stream = await content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
#else
        cancellationToken.ThrowIfCancellationRequested();
        Stream stream = await content.ReadAsStreamAsync().ConfigureAwait(false);
#endif

        await foreach (TItem? item in JsonSerializer
                           .DeserializeAsyncEnumerable(stream, typeInfo, topLevelValues, cancellationToken)
                           .ConfigureAwait(false))
        {
            yield return item!;
        }
    }
}
