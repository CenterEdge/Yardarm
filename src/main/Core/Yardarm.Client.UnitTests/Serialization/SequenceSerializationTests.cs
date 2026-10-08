using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
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

        #region Default implementation

        [Fact]
        public void SerializeSequence_DefaultImplementationWithList_SerializesSequenceType()
        {
            // Arrange

            var serializer = new RecordingSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            var items = new List<int> { 1, 2 };

            // Act

            registry.SerializeSequence<List<int>, int>(items, MediaType);

            // Assert

            serializer.SerializedType.Should().Be(typeof(List<int>));
            serializer.SerializedValue.Should().BeSameAs(items);
        }

        [Fact]
        public void SerializeSequence_DefaultImplementationWithHashSet_SerializesSequenceType()
        {
            // Arrange

            var serializer = new RecordingSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            var items = new HashSet<int> { 1, 2 };

            // Act

            registry.SerializeSequence<HashSet<int>, int>(items, MediaType);

            // Assert

            serializer.SerializedType.Should().Be(typeof(HashSet<int>));
            serializer.SerializedValue.Should().BeSameAs(items);
        }

        [Fact]
        public void SerializeSequence_DefaultImplementationWithArray_SerializesSequenceType()
        {
            // Arrange

            var serializer = new RecordingSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            int[] items = [1, 2];

            // Act

            registry.SerializeSequence<int[], int>(items, MediaType);

            // Assert

            serializer.SerializedType.Should().Be(typeof(int[]));
            serializer.SerializedValue.Should().BeSameAs(items);
        }

        [Fact]
        public void SerializeSequence_DefaultImplementationWithImmutableArray_SerializesSequenceType()
        {
            // Arrange

            var serializer = new RecordingSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            ImmutableArray<int> items = [1, 2];

            // Act

            registry.SerializeSequence<ImmutableArray<int>, int>(items, MediaType);

            // Assert

            serializer.SerializedType.Should().Be(typeof(ImmutableArray<int>));
            serializer.SerializedValue.Should().BeOfType<ImmutableArray<int>>().Which.Should().Equal(1, 2);
        }

        [Fact]
        public async Task DeserializeSequenceAsync_DefaultImplementationWithList_DeserializesSequenceType()
        {
            // Arrange

            var items = new List<int> { 1, 2 };
            var serializer = new RecordingSerializer { DeserializedValue = items };
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

            // Act

            var result = await registry.DeserializeSequenceAsync<List<int>, int>(CreateContent(),
                cancellationToken: cts.Token);

            // Assert

            serializer.DeserializedType.Should().Be(typeof(List<int>));
            serializer.CancellationToken.Should().Be(cts.Token);
            result.Should().BeSameAs(items);
        }

        [Fact]
        public async Task DeserializeSequenceAsync_DefaultImplementationWithHashSet_DeserializesSequenceType()
        {
            // Arrange

            var items = new HashSet<int> { 1, 2 };
            var serializer = new RecordingSerializer { DeserializedValue = items };
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);

            // Act

            var result = await registry.DeserializeSequenceAsync<HashSet<int>, int>(CreateContent(),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            serializer.DeserializedType.Should().Be(typeof(HashSet<int>));
            result.Should().BeSameAs(items);
        }

        #endregion

        #region Overridden implementation

        [Fact]
        public void SerializeSequence_OverriddenImplementation_PassesSequenceAndElementTypes()
        {
            // Arrange

            var serializer = new SequenceRecordingSerializer();
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            var items = new List<int> { 1, 2 };

            // Act

            registry.SerializeSequence<List<int>, int>(items, MediaType);

            // Assert

            serializer.SequenceType.Should().Be(typeof(List<int>));
            serializer.ElementType.Should().Be(typeof(int));
            serializer.SerializedValue.Should().BeSameAs(items);
        }

        [Fact]
        public async Task DeserializeSequenceAsync_OverriddenImplementation_PassesSequenceAndElementTypes()
        {
            // Arrange

            var items = new List<int> { 1, 2 };
            var serializer = new SequenceRecordingSerializer { DeserializedValue = items };
            var registry = new TypeSerializerRegistry().Add([MediaType], serializer);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

            // Act

            var result = await registry.DeserializeSequenceAsync<List<int>, int>(CreateContent(),
                cancellationToken: cts.Token);

            // Assert

            serializer.SequenceType.Should().Be(typeof(List<int>));
            serializer.ElementType.Should().Be(typeof(int));
            serializer.CancellationToken.Should().Be(cts.Token);
            result.Should().BeSameAs(items);
        }

        #endregion

        #region Lookup

        [Fact]
        public void SerializeSequence_SchemaTypeFallback_UsesSequenceType()
        {
            // Arrange

            var serializer = new RecordingSerializer();
            var registry = new TypeSerializerRegistry().Add([typeof(List<int>)], serializer);
            var items = new List<int> { 1 };

            // Act

            registry.SerializeSequence<List<int>, int>(items, "application/unknown");

            // Assert

            serializer.SerializedValue.Should().BeSameAs(items);
        }

        [Fact]
        public async Task DeserializeSequenceAsync_SchemaTypeFallback_UsesSequenceType()
        {
            // Arrange

            var items = new List<int> { 1 };
            var registry = new TypeSerializerRegistry()
                .Add([typeof(List<int>)], new RecordingSerializer { DeserializedValue = items });

            // Act

            var result = await registry.DeserializeSequenceAsync<List<int>, int>(CreateContent("application/unknown"),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.Should().BeSameAs(items);
        }

        [Fact]
        public void SerializeSequence_UnknownMediaType_Throws()
        {
            // Arrange

            var registry = new TypeSerializerRegistry();

            // Act

            Action action = () => registry.SerializeSequence<List<int>, int>([], MediaType);

            // Assert

            action.Should().Throw<UnknownMediaTypeException>();
        }

        [Fact]
        public void DeserializeSequenceAsync_UnknownMediaType_Throws()
        {
            // Arrange

            var registry = new TypeSerializerRegistry();

            // Act

            Action action = () => registry.DeserializeSequenceAsync<List<int>, int>(CreateContent(),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            action.Should().Throw<UnknownMediaTypeException>();
        }

        #endregion

        #region Built-in serializers

        [Fact]
        public async Task PlainTextSerializer_SequenceMethods_ForwardToSerializeAndDeserialize()
        {
            // Arrange

            // A string is a sequence of chars
            var serializer = new PlainTextSerializer();

            // Act

            var content = serializer.SerializeSequence<string, char>("abc", "text/plain");
            var result = await serializer.DeserializeSequenceAsync<string, char>(content,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.Should().Be("abc");
        }

        [Fact]
        public async Task BinaryStreamSerializer_SequenceMethods_ForwardToSerializeAndDeserialize()
        {
            // Arrange

            // A byte array is a sequence of bytes
            var serializer = new BinaryStreamSerializer();
            byte[] bytes = [1, 2, 3];

            // Act

            var content = serializer.SerializeSequence<byte[], byte>(bytes, "application/octet-stream");
            var result = await serializer.DeserializeSequenceAsync<byte[], byte>(content,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.Should().Equal(bytes);
        }

        [Fact]
        public void MultipartFormDataSerializer_SerializeSequenceWithoutMultipartData_Throws()
        {
            // Arrange

            var serializer = new MultipartFormDataSerializer(new TypeSerializerRegistry());

            // Act

            Action action = () => serializer.SerializeSequence<List<int>, int>([1], "multipart/form-data");

            // Assert

            // Forwards to Serialize, which requires MultipartFormDataSerializationData
            action.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void MultipartFormDataSerializer_DeserializeSequenceAsync_Throws()
        {
            // Arrange

            var serializer = new MultipartFormDataSerializer(new TypeSerializerRegistry());

            // Act

            Action action = () => serializer.DeserializeSequenceAsync<List<int>, int>(CreateContent(),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            // Forwards to DeserializeAsync, which multipart does not support
            action.Should().Throw<NotImplementedException>();
        }

        #endregion

        #region Helpers

        private static HttpContent CreateContent(string mediaType = MediaType)
            => new ByteArrayContent(Encoding.UTF8.GetBytes("")) { Headers = { ContentType = new MediaTypeHeaderValue(mediaType) } };

        /// <summary>
        /// Implements only <see cref="ITypeSerializer.Serialize{T}"/> and <see cref="ITypeSerializer.DeserializeAsync{T}(HttpContent, ISerializationData?, CancellationToken)"/>,
        /// so the sequence methods use the default implementation.
        /// </summary>
        private sealed class RecordingSerializer : ITypeSerializer
        {
            public Type SerializedType { get; private set; }

            public object SerializedValue { get; private set; }

            public Type DeserializedType { get; private set; }

            public object DeserializedValue { get; set; }

            public CancellationToken CancellationToken { get; private set; }

            public HttpContent Serialize<T>(T value, string mediaType, ISerializationData serializationData = null)
            {
                SerializedType = typeof(T);
                SerializedValue = value;
                return new ByteArrayContent([]);
            }

            public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData serializationData)
                => DeserializeAsync<T>(content, serializationData, default);

            public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData serializationData = null,
                CancellationToken cancellationToken = default)
            {
                DeserializedType = typeof(T);
                CancellationToken = cancellationToken;
                return new ValueTask<T>((T)DeserializedValue);
            }
        }

        /// <summary>
        /// Implements the sequence methods.
        /// </summary>
        private sealed class SequenceRecordingSerializer : ITypeSerializer
        {
            public Type SequenceType { get; private set; }

            public Type ElementType { get; private set; }

            public object SerializedValue { get; private set; }

            public object DeserializedValue { get; set; }

            public CancellationToken CancellationToken { get; private set; }

            public HttpContent SerializeSequence<TSequence, TElement>(TSequence value, string mediaType,
                ISerializationData serializationData = null)
                where TSequence : IEnumerable<TElement>
            {
                SequenceType = typeof(TSequence);
                ElementType = typeof(TElement);
                SerializedValue = value;
                return new ByteArrayContent([]);
            }

            public ValueTask<TSequence> DeserializeSequenceAsync<TSequence, TElement>(HttpContent content,
                ISerializationData serializationData = null, CancellationToken cancellationToken = default)
                where TSequence : IEnumerable<TElement>
            {
                SequenceType = typeof(TSequence);
                ElementType = typeof(TElement);
                CancellationToken = cancellationToken;
                return new ValueTask<TSequence>((TSequence)DeserializedValue);
            }

            public HttpContent Serialize<T>(T value, string mediaType, ISerializationData serializationData = null)
                => throw new NotSupportedException();

            public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData serializationData)
                => throw new NotSupportedException();

            public ValueTask<T> DeserializeAsync<T>(HttpContent content, ISerializationData serializationData = null,
                CancellationToken cancellationToken = default)
                => throw new NotSupportedException();
        }

        #endregion
    }
}
