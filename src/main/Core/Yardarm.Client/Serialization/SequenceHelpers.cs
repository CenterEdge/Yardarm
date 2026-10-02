using System.Collections.Generic;
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
    /// Collects an <see cref="IAsyncEnumerable{T}"/> into a <see cref="List{T}"/>.
    /// </summary>
    public static async ValueTask<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source,
        CancellationToken cancellationToken = default)
    {
        var list = new List<T>();

        await foreach (T item in source.WithCancellation(cancellationToken).ConfigureAwait(false))
        {
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
