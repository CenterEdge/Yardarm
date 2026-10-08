using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RootNamespace.Serialization;
using RootNamespace.Serialization.Json;
using Xunit;

namespace Yardarm.SystemTextJson.Client.UnitTests
{
    public class JsonStreamingSequenceTests
    {
        private static readonly JsonSerializerOptions s_options = new(JsonSerializerDefaults.Web);

        [Fact]
        public async Task JsonLines_AsyncEnumerable_ReadsEachRecord()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var result = await serializer.DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(
                new StringContent("{\"id\":1}\n{\"id\":2}\n"),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            (await ToListAsync(result)).Should().BeEquivalentTo([new Item { Id = 1 }, new Item { Id = 2 }]);
        }

        [Fact]
        public async Task Json_AsyncEnumerable_ReadsEachArrayElement()
        {
            // Arrange

            var serializer = new JsonTypeSerializer(s_options);

            // Act

            var result = await serializer.DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(
                new StringContent("[{\"id\":1},{\"id\":2}]"),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            (await ToListAsync(result)).Should().BeEquivalentTo([new Item { Id = 1 }, new Item { Id = 2 }]);
        }

        [Fact]
        public async Task Registry_AsyncEnumerable_UsesSerializerForMediaType()
        {
            // Arrange

            var registry = new TypeSerializerRegistry()
                .Add(JsonTypeSerializer.SupportedMediaTypes, new JsonTypeSerializer(s_options))
                .Add(JsonLinesTypeSerializer.SupportedMediaTypes, new JsonLinesTypeSerializer(s_options));

            var lines = new StringContent("{\"id\":1}\n{\"id\":2}\n", Encoding.UTF8, "application/jsonl");
            var array = new StringContent("[{\"id\":3}]", Encoding.UTF8, "application/json");

            // Act

            var linesResult = await registry.DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(lines,
                cancellationToken: TestContext.Current.CancellationToken);
            var arrayResult = await registry.DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(array,
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            (await ToListAsync(linesResult)).Select(p => p.Id).Should().Equal(1, 2);
            (await ToListAsync(arrayResult)).Select(p => p.Id).Should().Equal(3);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task AsyncEnumerable_NotEnumerated_DoesNotReadContent(bool jsonLines)
        {
            // Arrange

            using var content = new PipeContent();

            // Act

            var result = jsonLines
                ? new JsonLinesTypeSerializer(s_options).DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(
                    content, cancellationToken: TestContext.Current.CancellationToken)
                : new JsonTypeSerializer(s_options).DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(
                    content, cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.IsCompletedSuccessfully.Should().BeTrue();
            content.StreamRequested.Should().BeFalse();

            var sequence = await result;
            await using var enumerator = sequence.GetAsyncEnumerator(TestContext.Current.CancellationToken);
            content.StreamRequested.Should().BeFalse("the stream isn't requested until the first item is enumerated");

            var moveNext = enumerator.MoveNextAsync();
            await content.StreamRequestedTask.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            moveNext.IsCompleted.Should().BeFalse("no data has been written");

            if (!jsonLines)
            {
                // An empty body isn't a valid JSON array
                await WriteAsync(content.Pipe.Writer, "[]");
            }

            await content.Pipe.Writer.CompleteAsync();
            (await moveNext).Should().BeFalse();
        }

        [Fact]
        public async Task JsonLines_AsyncEnumerable_YieldsItemsBeforeContentCompletes()
        {
            // Arrange

            using var content = new PipeContent();
            var serializer = new JsonLinesTypeSerializer(s_options);

            var result = await serializer.DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(content,
                cancellationToken: TestContext.Current.CancellationToken);
            await using var enumerator = result.GetAsyncEnumerator(TestContext.Current.CancellationToken);

            // Act

            await WriteAsync(content.Pipe.Writer, "{\"id\":1}\n");
            (await enumerator.MoveNextAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10),
                TestContext.Current.CancellationToken)).Should().BeTrue();
            int first = enumerator.Current.Id;

            await WriteAsync(content.Pipe.Writer, "{\"id\":2}\n");
            (await enumerator.MoveNextAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10),
                TestContext.Current.CancellationToken)).Should().BeTrue();
            int second = enumerator.Current.Id;

            await content.Pipe.Writer.CompleteAsync();

            // Assert

            first.Should().Be(1);
            second.Should().Be(2);
            (await enumerator.MoveNextAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task Json_AsyncEnumerable_YieldsElementsBeforeContentCompletes()
        {
            // Arrange

            using var content = new PipeContent();
            var serializer = new JsonTypeSerializer(s_options);

            var result = await serializer.DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(content,
                cancellationToken: TestContext.Current.CancellationToken);
            await using var enumerator = result.GetAsyncEnumerator(TestContext.Current.CancellationToken);

            // Act

            await WriteAsync(content.Pipe.Writer, "[{\"id\":1},");
            (await enumerator.MoveNextAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(10),
                TestContext.Current.CancellationToken)).Should().BeTrue();
            int first = enumerator.Current.Id;

            await WriteAsync(content.Pipe.Writer, "{\"id\":2}]");
            await content.Pipe.Writer.CompleteAsync();

            // Assert

            first.Should().Be(1);
            (await enumerator.MoveNextAsync()).Should().BeTrue();
            enumerator.Current.Id.Should().Be(2);
            (await enumerator.MoveNextAsync()).Should().BeFalse();
        }

        [Fact]
        public async Task AsyncEnumerable_EnumeratedTwice_ThrowsInvalidOperationException()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);
            var result = await serializer.DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(
                new StringContent("{\"id\":1}\n"), cancellationToken: TestContext.Current.CancellationToken);
            await ToListAsync(result);

            // Act

            Func<Task> action = async () => await ToListAsync(result);

            // Assert

            await action.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task JsonLines_AsyncEnumerable_MalformedRecord_YieldsEarlierRecordsThenThrows()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);
            var result = await serializer.DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(
                new StringContent("{\"id\":1}\n{\"id\":\n"), cancellationToken: TestContext.Current.CancellationToken);
            var items = new List<Item>();

            // Act

            Func<Task> action = async () =>
            {
                await foreach (Item item in result)
                {
                    items.Add(item);
                }
            };

            // Assert

            await action.Should().ThrowAsync<JsonException>();
            items.Should().ContainSingle().Which.Id.Should().Be(1);
        }

        [Fact]
        public async Task AsyncEnumerable_EnumerationTokenCanceled_Throws()
        {
            // Arrange

            using var content = new PipeContent();
            var serializer = new JsonLinesTypeSerializer(s_options);
            var result = await serializer.DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(content,
                cancellationToken: TestContext.Current.CancellationToken);
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);

            // Act

            Func<Task> action = async () =>
            {
                await foreach (Item _ in result.WithCancellation(cts.Token))
                {
                }
            };
            var task = action();
            await content.StreamRequestedTask.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            cts.Cancel();

            // Assert

            await task.Invoking(p => p).Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task AsyncEnumerable_BodyTokenCanceled_Throws()
        {
            // Arrange

            using var content = new PipeContent();
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            var serializer = new JsonTypeSerializer(s_options);
            var result = await serializer.DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(content,
                cancellationToken: cts.Token);

            // Act

            Func<Task> action = async () => await ToListAsync(result);
            var task = action();
            await content.StreamRequestedTask.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            cts.Cancel();

            // Assert

            await task.Invoking(p => p).Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task AsyncEnumerable_BothTokensCancelable_EitherCancels()
        {
            // Arrange

            using var content = new PipeContent();
            using var bodyCts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            using var enumerationCts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
            var serializer = new JsonLinesTypeSerializer(s_options);
            var result = await serializer.DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(content,
                cancellationToken: bodyCts.Token);

            // Act

            Func<Task> action = async () =>
            {
                await foreach (Item _ in result.WithCancellation(enumerationCts.Token))
                {
                }
            };
            var task = action();
            await content.StreamRequestedTask.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
            bodyCts.Cancel();

            // Assert

            await task.Invoking(p => p).Should().ThrowAsync<OperationCanceledException>();
        }

        [Fact]
        public async Task JsonLines_OtherSequenceTypes_StillSupported()
        {
            // Arrange

            var serializer = new JsonLinesTypeSerializer(s_options);

            // Act

            var list = await serializer.DeserializeSequenceAsync<List<Item>, Item>(new StringContent("{\"id\":1}\n"),
                cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            list.Should().ContainSingle().Which.Id.Should().Be(1);
        }

        [Theory]
        [InlineData("utf-16", true)]
        [InlineData("utf-16", false)]
        [InlineData("utf-16BE", true)]
        [InlineData("utf-16BE", false)]
        [InlineData("utf-32", false)]
        [InlineData("\"utf-16\"", true)]
        public async Task AsyncEnumerable_NonUtf8Charset_IsTranscoded(string charset, bool jsonLines)
        {
            // Arrange

            Encoding encoding = Encoding.GetEncoding(charset.Trim('"'));
            string body = jsonLines
                ? "{\"id\":1,\"name\":\"é€\"}\n{\"id\":2,\"name\":\"b\"}\n"
                : "[{\"id\":1,\"name\":\"é€\"},{\"id\":2,\"name\":\"b\"}]";
            var content = new ByteArrayContent(encoding.GetBytes(body));
            content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(
                $"{(jsonLines ? "application/jsonl" : "application/json")}; charset={charset}");

            // Act

            var result = jsonLines
                ? await new JsonLinesTypeSerializer(s_options).DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(
                    content, cancellationToken: TestContext.Current.CancellationToken)
                : await new JsonTypeSerializer(s_options).DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(
                    content, cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            (await ToListAsync(result)).Should().BeEquivalentTo(
                [new Item { Id = 1, Name = "é€" }, new Item { Id = 2, Name = "b" }]);
        }

        [Fact]
        public async Task AsyncEnumerable_Utf16LargeBody_IsTranscodedAcrossReads()
        {
            // Arrange

            var items = Enumerable.Range(1, 5000).Select(p => new Item { Id = p, Name = new string('é', 20) }).ToList();
            string body = string.Join("\n", items.Select(p => JsonSerializer.Serialize(p, s_options))) + "\n";
            var content = new ByteArrayContent(Encoding.Unicode.GetBytes(body));
            content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(
                "application/jsonl; charset=utf-16");

            // Act

            var result = await new JsonLinesTypeSerializer(s_options).DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(
                content, cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            (await ToListAsync(result)).Should().BeEquivalentTo(items, o => o.WithStrictOrdering());
        }

        [Fact]
        public async Task AsyncEnumerable_UnsupportedCharset_ThrowsNotSupportedException()
        {
            // Arrange

            var content = new StringContent("[]");
            content.Headers.ContentType = System.Net.Http.Headers.MediaTypeHeaderValue.Parse(
                "application/json; charset=not-a-charset");
            var result = await new JsonTypeSerializer(s_options).DeserializeSequenceAsync<IAsyncEnumerable<Item>, Item>(
                content, cancellationToken: TestContext.Current.CancellationToken);

            // Act

            Func<Task> action = async () => await ToListAsync(result);

            // Assert

            await action.Should().ThrowAsync<NotSupportedException>();
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

        private static async Task WriteAsync(PipeWriter writer, string value)
        {
            await writer.WriteAsync(Encoding.UTF8.GetBytes(value));
            await writer.FlushAsync();
        }

        /// <summary>
        /// Content which is read from a pipe, records when the stream is requested.
        /// </summary>
        private sealed class PipeContent : HttpContent
        {
            private readonly TaskCompletionSource _streamRequested = new(TaskCreationOptions.RunContinuationsAsynchronously);

            public Pipe Pipe { get; } = new();

            public bool StreamRequested => _streamRequested.Task.IsCompleted;

            public Task StreamRequestedTask => _streamRequested.Task;

            protected override Task<Stream> CreateContentReadStreamAsync(CancellationToken cancellationToken)
            {
                _streamRequested.TrySetResult();
                return Task.FromResult(Pipe.Reader.AsStream());
            }

            protected override Task SerializeToStreamAsync(Stream stream, TransportContext context) =>
                throw new NotSupportedException();

            protected override bool TryComputeLength(out long length)
            {
                length = 0;
                return false;
            }
        }

        public class Item
        {
            public int Id { get; set; }

            public string Name { get; set; }
        }
    }
}
