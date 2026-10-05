using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization;

/// <summary>
/// Conversions between <see cref="IEnumerable{T}"/>, <see cref="IAsyncEnumerable{T}"/> and <see cref="List{T}"/>.
/// </summary>
/// <remarks>
/// These are static methods rather than extension methods to avoid ambiguity with System.Linq.AsyncEnumerable.
/// </remarks>
internal static class SequenceHelpers
{
    /// <summary>
    /// Wraps an <see cref="IEnumerable{T}"/> as an <see cref="IAsyncEnumerable{T}"/> that completes synchronously.
    /// </summary>
    public static IAsyncEnumerable<T> FromEnumerable<T>(IEnumerable<T> source) =>
        source as IAsyncEnumerable<T> ?? new SyncAsyncEnumerable<T>(source);

    /// <summary>
    /// Gets a <see cref="List{T}"/> for a sequence that is a collection already in memory, which can be enumerated
    /// synchronously. A <see cref="List{T}"/> is returned as is, other collections are copied.
    /// </summary>
    /// <param name="source">The sequence, which may be an <see cref="IEnumerable{T}"/> or an
    /// <see cref="IAsyncEnumerable{T}"/>.</param>
    /// <param name="list">The list, or <c>null</c> if the sequence is not a collection already in memory.</param>
    /// <returns><c>true</c> if the sequence is a collection already in memory.</returns>
    public static bool TryGetInMemoryList<T>(object source, [NotNullWhen(true)] out List<T>? list)
    {
        switch (source)
        {
            case List<T> sourceList:
                list = sourceList;
                return true;

            case ICollection<T> collection:
                list = new List<T>(collection);
                return true;

            case IReadOnlyCollection<T> readOnlyCollection:
                list = new List<T>(readOnlyCollection.Count);
                list.AddRange(readOnlyCollection);
                return true;

            default:
                list = null;
                return false;
        }
    }

    /// <summary>
    /// Collects an <see cref="IAsyncEnumerable{T}"/> into a <see cref="List{T}"/>.
    /// </summary>
    public static async ValueTask<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        var list = source is IReadOnlyCollection<T> collection
            ? new List<T>(collection.Count)
            : new List<T>();

        // Check the token directly, since the source may not observe the token passed to its enumerator
        cancellationToken.ThrowIfCancellationRequested();

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
            cancellationToken.ThrowIfCancellationRequested();

            list.Add(item);
        }

        return list;
    }

    private sealed class SyncAsyncEnumerable<T>(IEnumerable<T> source) : IAsyncEnumerable<T>
    {
        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
            new Enumerator(source.GetEnumerator(), cancellationToken);

        private sealed class Enumerator(IEnumerator<T> enumerator, CancellationToken cancellationToken)
            : IAsyncEnumerator<T>
        {
            public T Current => enumerator.Current;

            public ValueTask<bool> MoveNextAsync()
            {
                cancellationToken.ThrowIfCancellationRequested();

                return new ValueTask<bool>(enumerator.MoveNext());
            }

            public ValueTask DisposeAsync()
            {
                enumerator.Dispose();

                return default;
            }
        }
    }
}
