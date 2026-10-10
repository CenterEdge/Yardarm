using System.IO;
using System.Net.Http;
using System.Net.Mime;
using System.Threading.Tasks;
using FluentAssertions;
using RootNamespace.Serialization;
using Xunit;

namespace Yardarm.Client.UnitTests.Serialization
{
    public class BinaryStreamSerializerTests
    {
        private static readonly byte[] TestBytes = [1, 2, 3, 4, 5];

        #region Serialize

        [Fact]
        public async Task Serialize_ByteArray_ReturnsContentWithBytes()
        {
            // Arrange

            var serializer = new BinaryStreamSerializer();

            // Act

            using HttpContent content = serializer.Serialize(TestBytes, MediaTypeNames.Application.Octet, null);

            // Assert

            content.Headers.ContentType!.MediaType.Should().Be(MediaTypeNames.Application.Octet);
            (await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(TestBytes);
        }

        [Fact]
        public async Task Serialize_Stream_ReturnsContentWithBytes()
        {
            // Arrange

            var serializer = new BinaryStreamSerializer();
            using var stream = new MemoryStream(TestBytes);

            // Act

            using HttpContent content = serializer.Serialize<Stream>(stream, MediaTypeNames.Application.Octet, null);

            // Assert

            content.Headers.ContentType!.MediaType.Should().Be(MediaTypeNames.Application.Octet);
            (await content.ReadAsByteArrayAsync(TestContext.Current.CancellationToken)).Should().Equal(TestBytes);
        }

        #endregion

        #region DeserializeAsync

        [Fact]
        public async Task DeserializeAsync_ByteArray_ReturnsBytes()
        {
            // Arrange

            var serializer = new BinaryStreamSerializer();
            using var content = new ByteArrayContent(TestBytes);

            // Act

            byte[] result = await serializer.DeserializeAsync<byte[]>(content, cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            result.Should().Equal(TestBytes);
        }

        [Fact]
        public async Task DeserializeAsync_Stream_ReturnsStream()
        {
            // Arrange

            var serializer = new BinaryStreamSerializer();
            using var content = new ByteArrayContent(TestBytes);

            // Act

            using Stream result = await serializer.DeserializeAsync<Stream>(content, cancellationToken: TestContext.Current.CancellationToken);

            // Assert

            using var buffer = new MemoryStream();
            await result.CopyToAsync(buffer, TestContext.Current.CancellationToken);
            buffer.ToArray().Should().Equal(TestBytes);
        }

        #endregion
    }
}
