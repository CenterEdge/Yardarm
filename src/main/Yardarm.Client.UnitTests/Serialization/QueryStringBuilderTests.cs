using FluentAssertions;
using RootNamespace.Serialization;
using Xunit;

namespace Yardarm.Client.UnitTests.Serialization
{
    public class QueryStringBuilderTests
    {
        [Fact]
        public void ToString_NoParameters_ReturnsUri()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri");
        }

        [Fact]
        public void ToString_OnePrimitive_AddsWithQuestionMark()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendPrimitive("name", "value", false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value");
        }

        [Fact]
        public void ToString_TwoPrimitives_AddsWithQuestionMarkAndAmpersand()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendPrimitive("name", "value", false);
            builder.AppendPrimitive("name2", "value2", false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value&name2=value2");
        }

        [Fact]
        public void ToString_PrimitiveAllowReserved_DoesNotEscape()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendPrimitive("name", "value ", true);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value ");
        }

        [Fact]
        public void ToString_PrimitiveNoAllowReserved_Escapes()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendPrimitive("name", "value ", false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value%20");
        }

        [Fact]
        public void ToString_NullRefType_Skips()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendPrimitive("name", (string) null, false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri");
        }

        [Fact]
        public void ToString_NullValueType_Skips()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendPrimitive("name", (long?) null, false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri");
        }

        [Fact]
        public void ToString_ListExplode_MultipleValues()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendList("name", new object[] { "value1", "value2" }, true, ",", false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value1&name=value2");
        }

        [Fact]
        public void ToString_ListExplode_SkipsNulls()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendList("name", new object[] { "value1", null, "value2" }, true, ",", false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value1&name=value2");
        }

        [Fact]
        public void ToString_ListExplodeAllowReserved_DoesNotEscape()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendList("name", new object[] { "value1", "value2 " }, true, ",", true);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value1&name=value2 ");
        }

        [Fact]
        public void ToString_ListExplodeNoAllowReserved_Escapes()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendList("name", new object[] { "value1", "value2 " }, true, ",", false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value1&name=value2%20");
        }

        [Fact]
        public void ToString_ListDelimited_Concatenates()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendList("name", new object[] { "value1", "value2" }, false, "%20", false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value1%20value2");
        }

        [Fact]
        public void ToString_ListDelimitedAllowReserved_DoesNotEscape()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendList("name", new object[] { "value1", "value2 " }, false, "%20", true);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value1%20value2 ");
        }

        [Fact]
        public void ToString_ListDelimitedNoAllowReserved_Escapes()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendList("name", new object[] { "value1", "value2 " }, false, "%20", false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value1%20value2%20");
        }
    }
}
