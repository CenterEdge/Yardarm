using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RootNamespace.Serialization;
using Xunit;

namespace Yardarm.Client.UnitTests.Serialization
{
    public class SequenceSerializationTests
    {
        private const string MediaType = "application/jsonl";

        #region SerializeSequence

        [Fact]
        public async Task SerializeSequence_SequenceSerializerWithEnumerable_PassesItemsAsSequence()
        {
            // Arrange

            var serializer = new SequenceSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);

            // Act

            registry.SerializeSequence(EnumerableItems(1, 2), MediaType);

            // Assert

            serializer.SerializedValue.Should().BeNull();
            (await ToListAsync((IAsyncEnumerable<int>)serializer.SerializedItems)).Should().Equal(1, 2);
        }

        [Fact]
        public void SerializeSequence_SequenceSerializerWithList_SerializesSameList()
        {
            // Arrange

            var serializer = new SequenceSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            var items = new List<int> { 1, 2 };

            // Act

            registry.SerializeSequence(items, MediaType);

            // Assert

            serializer.SerializedItems.Should().BeNull();
            serializer.SerializedValue.Should().BeSameAs(items);
        }

        [Fact]
        public void SerializeSequence_SequenceSerializerWithReadOnlyCollection_SerializesList()
        {
            // Arrange

            var serializer = new SequenceSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);

            // Act

            registry.SerializeSequence(new[] { 1, 2 }, MediaType);

            // Assert

            serializer.SerializedItems.Should().BeNull();
            serializer.SerializedValue.Should().BeOfType<List<int>>().Which.Should().Equal(1, 2);
        }

        [Fact]
        public void SerializeSequence_ListSerializerWithEnumerable_SerializesList()
        {
            // Arrange

            var serializer = new ListSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);

            // Act

            registry.SerializeSequence(EnumerableItems(1, 2), MediaType);

            // Assert

            serializer.SerializedValue.Should().BeOfType<List<int>>().Which.Should().Equal(1, 2);
        }

        [Fact]
        public void SerializeSequence_ListSerializerWithList_PassesSameList()
        {
            // Arrange

            var serializer = new ListSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            var items = new List<int> { 1, 2 };

            // Act

            registry.SerializeSequence(items, MediaType);

            // Assert

            serializer.SerializedValue.Should().BeSameAs(items);
        }

        [Fact]
        public void SerializeSequence_ListSerializerWithArray_CopiesToList()
        {
            // Arrange

            var serializer = new ListSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);

            // Act

            registry.SerializeSequence(new[] { 1, 2 }, MediaType);

            // Assert

            serializer.SerializedValue.Should().BeOfType<List<int>>().Which.Should().Equal(1, 2);
        }

        [Fact]
        public void SerializeSequence_ListSerializerWithAsyncEnumerable_Throws()
        {
            // Arrange

            var registry = new TypeSerializerRegistry().Add([MediaType], new ListSerializer());

            // Act

            Action action = () => registry.SerializeSequence(AsyncItems(1, 2), MediaType);

            // Assert

            action.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public async Task SerializeSequence_SequenceSerializerWithAsyncEnumerable_PassesSameSequence()
        {
            // Arrange

            var serializer = new SequenceSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            var items = AsyncItems(1, 2);

            // Act

            registry.SerializeSequence(items, MediaType);

            // Assert

            serializer.SerializedItems.Should().BeSameAs(items);
            (await ToListAsync(items)).Should().Equal(1, 2);
        }

        [Fact]
        public void SerializeSequence_SchemaTypeFallback_UsesListSchemaType()
        {
            // Arrange

            var serializer = new ListSerializer();
            var registry = new TypeSerializerRegistry().Add([typeof(List<int>)], serializer);
            var items = new List<int> { 1 };

            // Act

            registry.SerializeSequence(items, "application/unknown");

            // Assert

            serializer.SerializedValue.Should().BeSameAs(items);
        }

        [Fact]
        public void SerializeSequence_UnknownMediaType_Throws()
        {
            // Arrange

            var registry = new TypeSerializerRegistry();

            // Act

            Action action = () => registry.SerializeSequence(new List<int>(), MediaType);

            // Assert

            action.Should().Throw<UnknownMediaTypeException>();
        }

        [Fact]
        public void SerializeSequence_BothInterfaces_PrefersAsyncEnumerableOverload()
        {
            // Arrange

            // The IEnumerable<T> overload would succeed with a List<T>, the IAsyncEnumerable<T> overload throws
            var registry = new TypeSerializerRegistry().Add([MediaType], new ListSerializer());

            // Act

            Action action = () => registry.SerializeSequence(new DualSequence(), MediaType);

            // Assert

            action.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void SerializeSequence_BothInterfacesStaticCall_PrefersAsyncEnumerableOverload()
        {
            // Arrange

            var registry = new TypeSerializerRegistry().Add([MediaType], new ListSerializer());

            // Act

            Action action = () => TypeSerializerRegistryExtensions.SerializeSequence(registry, new DualSequence(), MediaType);

            // Assert

            action.Should().Throw<NotSupportedException>();
        }

        [Fact]
        public void SerializeSequence_ListSerializerWithAsyncList_SerializesSameList()
        {
            // Arrange

            var serializer = new ListSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            var items = new AsyncList { 1, 2 };

            // Act

            registry.SerializeSequence(items, MediaType);

            // Assert

            serializer.SerializedValue.Should().BeSameAs(items);
        }

        [Fact]
        public void SerializeSequence_ListSerializerWithAsyncCollection_SerializesList()
        {
            // Arrange

            var serializer = new ListSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);

            // Act

            registry.SerializeSequence(new AsyncCollection { 1, 2 }, MediaType);

            // Assert

            serializer.SerializedValue.Should().BeOfType<List<int>>().Which.Should().Equal(1, 2);
        }

        [Fact]
        public void SerializeSequence_ListSerializerWithAsyncReadOnlyCollection_SerializesList()
        {
            // Arrange

            var serializer = new ListSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);

            // Act

            registry.SerializeSequence(new AsyncReadOnlyCollection(1, 2), MediaType);

            // Assert

            serializer.SerializedValue.Should().BeOfType<List<int>>().Which.Should().Equal(1, 2);
        }

        [Fact]
        public void SerializeSequence_SequenceSerializerWithAsyncList_SerializesSameList()
        {
            // Arrange

            var serializer = new SequenceSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            var items = new AsyncList { 1, 2 };

            // Act

            registry.SerializeSequence(items, MediaType);

            // Assert

            serializer.SerializedItems.Should().BeNull();
            serializer.SerializedValue.Should().BeSameAs(items);
        }

        #endregion

        #region DeserializeSequenceAsync

        [Fact]
        public async Task DeserializeSequenceAsync_SequenceSerializer_ReturnsItems()
        {
            // Arrange

            var serializer = new SequenceSerializer { DeserializedItems = [1, 2] };
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);

            // Act

            var result = await ToListAsync(registry.DeserializeSequenceAsync<int>(CreateContent(), cancellationToken: TestContext.Current.CancellationToken));

            // Assert

            result.Should().Equal(1, 2);
        }

        [Fact]
        public async Task DeserializeSequenceAsync_ListSerializer_DeserializesListOnce()
        {
            // Arrange

            var serializer = new ListSerializer { DeserializedValue = new List<int> { 1, 2 } };
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);

            // Act

            var sequence = registry.DeserializeSequenceAsync<int>(CreateContent(), cancellationToken: TestContext.Current.CancellationToken);
            int callsBeforeEnumeration = serializer.DeserializeCalls;
            var result = await ToListAsync(sequence);

            // Assert

            callsBeforeEnumeration.Should().Be(0);
            serializer.DeserializeCalls.Should().Be(1);
            result.Should().Equal(1, 2);
        }

        [Fact]
        public async Task DeserializeSequenceAsync_ListSerializerReturnsNull_Empty()
        {
            // Arrange

            var registry = new TypeSerializerRegistry().Add([MediaType], new ListSerializer());

            // Act

            var result = await ToListAsync(registry.DeserializeSequenceAsync<int>(CreateContent(), cancellationToken: TestContext.Current.CancellationToken));

            // Assert

            result.Should().BeEmpty();
        }

        [Fact]
        public void DeserializeSequenceAsync_UnknownMediaType_Throws()
        {
            // Arrange

            var registry = new TypeSerializerRegistry();

            // Act

            Action action = () => registry.DeserializeSequenceAsync<int>(CreateContent(), cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            action.Should().Throw<UnknownMediaTypeException>();
        }

        #endregion

        #region DeserializeSequenceToListAsync

        [Fact]
        public async Task DeserializeSequenceToListAsync_SequenceSerializer_CollectsItems()
        {
            // Arrange

            var serializer = new SequenceSerializer { DeserializedItems = [1, 2] };
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

            // Act

            var result = await registry.DeserializeSequenceToListAsync<int>(CreateContent(), cancellationToken: cts.Token);

            // Assert

            result.Should().Equal(1, 2);
            serializer.CancellationToken.Should().Be(cts.Token);
        }

        [Fact]
        public async Task DeserializeSequenceToListAsync_ListSerializer_ReturnsDeserializedList()
        {
            // Arrange

            var list = new List<int> { 1, 2 };
            var serializer = new ListSerializer { DeserializedValue = list };
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

            // Act

            var result = await registry.DeserializeSequenceToListAsync<int>(CreateContent(), cancellationToken: cts.Token);

            // Assert

            result.Should().BeSameAs(list);
            serializer.DeserializeCalls.Should().Be(1);
            serializer.CancellationToken.Should().Be(cts.Token);
        }

        [Fact]
        public async Task DeserializeSequenceToListAsync_SchemaTypeFallback_UsesListSchemaType()
        {
            // Arrange

            var list = new List<int> { 1 };
            var registry = new TypeSerializerRegistry()
                .Add([typeof(List<int>)], new ListSerializer { DeserializedValue = list });

            // Act

            var result = await registry.DeserializeSequenceToListAsync<int>(CreateContent("application/unknown"), cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.Should().BeSameAs(list);
        }

        #endregion

        #region Helpers

        private static HttpContent CreateContent(string mediaType = MediaType) =>
            new ByteArrayContent([]) { Headers = { ContentType = new MediaTypeHeaderValue(mediaType) } };

        private static async IAsyncEnumerable<int> AsyncItems(params int[] items)
        {
            foreach (int item in items)
            {
                await Task.Yield();
                yield return item;
            }
        }

        // An enumerable that is not a collection
        private static IEnumerable<int> EnumerableItems(params int[] items)
        {
            foreach (int item in items)
            {
                yield return item;
            }
        }

        private static async Task<List<T>> ToListAsync<T>(IAsyncEnumerable<T> source)
        {
            var list = new List<T>();
            await foreach (T item in source)
            {
                list.Add(item);
            }

            return list;
        }

        private sealed class ListSerializer : ITypeSerializer
        {
            public object SerializedValue { get; private set; }

            public object DeserializedValue { get; set; }

            public int DeserializeCalls { get; private set; }

            public CancellationToken CancellationToken { get; private set; }

            public const string SerializedBody = "serialized";

            public HttpContent Serialize<T>(T value, string mediaType, ISerializationData serializationData = null)
            {
                SerializedValue = value;
                return new StringContent(SerializedBody);
            }

            public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData serializationData) =>
                DeserializeAsync<T>(content, serializationData, default);

            public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData serializationData = null,
                CancellationToken cancellationToken = default)
            {
                DeserializeCalls++;
                CancellationToken = cancellationToken;
                return new ValueTask<T>((T)DeserializedValue);
            }
        }

        private sealed class SequenceSerializer : ISequenceTypeSerializer
        {
            public object SerializedItems { get; private set; }

            public object SerializedValue { get; private set; }

            public int[] DeserializedItems { get; set; } = [];

            public CancellationToken CancellationToken { get; private set; }

            public HttpContent SerializeSequence<TItem>(IAsyncEnumerable<TItem> items, string mediaType,
                ISerializationData serializationData = null)
            {
                SerializedItems = items;
                return new ByteArrayContent([]);
            }

            public IAsyncEnumerable<TItem> DeserializeSequenceAsync<TItem>(HttpContent content,
                ISerializationData serializationData = null, CancellationToken cancellationToken = default)
            {
                CancellationToken = cancellationToken;
                return (IAsyncEnumerable<TItem>)AsyncItems(DeserializedItems);
            }

            public HttpContent Serialize<T>(T value, string mediaType, ISerializationData serializationData = null)
            {
                SerializedValue = value;
                return new ByteArrayContent([]);
            }

            public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData serializationData) =>
                throw new NotSupportedException();

            public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData serializationData = null,
                CancellationToken cancellationToken = default) =>
                throw new NotSupportedException();
        }

        /// <summary>
        /// A <see cref="List{T}"/> that also implements <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        private sealed class AsyncList : List<int>, IAsyncEnumerable<int>
        {
            public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
                throw new InvalidOperationException("In-memory collections must be enumerated synchronously.");
        }

        /// <summary>
        /// An <see cref="ICollection{T}"/>, but not an <see cref="IReadOnlyCollection{T}"/>, that also implements
        /// <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        private sealed class AsyncCollection : ICollection<int>, IAsyncEnumerable<int>
        {
            private readonly List<int> _items = [];

            public int Count => _items.Count;
            public bool IsReadOnly => false;
            public void Add(int item) => _items.Add(item);
            public void Clear() => _items.Clear();
            public bool Contains(int item) => _items.Contains(item);
            public void CopyTo(int[] array, int arrayIndex) => _items.CopyTo(array, arrayIndex);
            public bool Remove(int item) => _items.Remove(item);
            public IEnumerator<int> GetEnumerator() => _items.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
                throw new InvalidOperationException("In-memory collections must be enumerated synchronously.");
        }

        /// <summary>
        /// An <see cref="IReadOnlyCollection{T}"/>, but not an <see cref="ICollection{T}"/>, that also implements
        /// <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        private sealed class AsyncReadOnlyCollection(params int[] items) : IReadOnlyCollection<int>, IAsyncEnumerable<int>
        {
            public int Count => items.Length;
            public IEnumerator<int> GetEnumerator() => ((IEnumerable<int>)items).GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
                throw new InvalidOperationException("In-memory collections must be enumerated synchronously.");
        }

        /// <summary>
        /// Implements both <see cref="IEnumerable{T}"/> and <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        private sealed class DualSequence : IEnumerable<int>, IAsyncEnumerable<int>
        {
            public IEnumerator<int> GetEnumerator() => new List<int> { 1 }.GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

            public IAsyncEnumerator<int> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
                AsyncItems(1).GetAsyncEnumerator(cancellationToken);
        }

        #endregion
    }
}
