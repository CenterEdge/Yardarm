using System;
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
/// A single-pass <see cref="IAsyncEnumerable{T}"/> over the JSON values in an <see cref="HttpContent"/>. The content
/// stream is not requested until the first item is enumerated, so creating the sequence never waits on data.
/// </summary>
/// <typeparam name="T">The type of each item.</typeparam>
internal sealed class JsonStreamingEnumerable<T> : IAsyncEnumerable<T>
{
    private readonly HttpContent _content;
    private readonly JsonTypeInfo<T> _typeInfo;
    private readonly bool _topLevelValues;
    private readonly CancellationToken _bodyCancellationToken;
    private int _enumerated;

    /// <param name="content">The content to read.</param>
    /// <param name="typeInfo">Type info for each item.</param>
    /// <param name="topLevelValues">
    /// If true, each item is a top level value (JSON Lines). If false, the items are the elements of a JSON array.
    /// </param>
    /// <param name="bodyCancellationToken">
    /// A token supplied when the sequence was requested. It is combined with any token passed to
    /// <see cref="GetAsyncEnumerator"/>.
    /// </param>
    public JsonStreamingEnumerable(HttpContent content, JsonTypeInfo<T> typeInfo, bool topLevelValues,
        CancellationToken bodyCancellationToken)
    {
        ArgumentNullException.ThrowIfNull(content);
        ArgumentNullException.ThrowIfNull(typeInfo);

        _content = content;
        _typeInfo = typeInfo;
        _topLevelValues = topLevelValues;
        _bodyCancellationToken = bodyCancellationToken;
    }

    /// <exception cref="InvalidOperationException">The sequence has already been enumerated.</exception>
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        if (Interlocked.Exchange(ref _enumerated, 1) != 0)
        {
            ThrowHelper.ThrowInvalidOperationException(
                "The response body is a stream which can only be enumerated once.");
        }

        return EnumerateAsync(cancellationToken).GetAsyncEnumerator(CancellationToken.None);
    }

    private async IAsyncEnumerable<T> EnumerateAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        using CancellationTokenSource? linkedSource =
            cancellationToken.CanBeCanceled && _bodyCancellationToken.CanBeCanceled
                ? CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _bodyCancellationToken)
                : null;

        CancellationToken token = linkedSource?.Token
            ?? (cancellationToken.CanBeCanceled ? cancellationToken : _bodyCancellationToken);

        // Don't touch the content until the first item is requested
        using Stream stream = await JsonContentStream.ReadAsUtf8StreamAsync(_content, token).ConfigureAwait(false);

        await foreach (T? item in JsonSerializer.DeserializeAsyncEnumerable(stream, _typeInfo, _topLevelValues, token)
            .ConfigureAwait(false))
        {
            yield return item!;
        }
    }
}
