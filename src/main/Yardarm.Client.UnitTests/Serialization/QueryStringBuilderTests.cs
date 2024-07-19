using System;
using FluentAssertions;
using RootNamespace.Serialization;
using Xunit;

namespace Yardarm.Client.UnitTests.Serialization
{
    public class QueryStringBuilderTests
    {
        #region ToString

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
        public void ToString_Date_AddsWithDateFormat()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendPrimitive("name", new DateTime(2020, 1, 2), false, "date");

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=2020-01-02");
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
        public void ToString_NonNullValueType_Serializes()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendPrimitive("name", (long?) 10, false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=10");
        }

        [Fact]
        public void ToString_ListExplode_MultipleValues()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendList("name", new[] { "value1", "value2" }, true, ",", false);

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
            builder.AppendList("name", new[] { "value1", null, "value2" }, true, ",", false);

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
            builder.AppendList("name", new[] { "value1", "value2 " }, true, ",", true);

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
            builder.AppendList("name", new[] { "value1", "value2 " }, true, ",", false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value1&name=value2%20");
        }

        [Fact]
        public void ToString_DateListExplode_MultipleValuesWithDateFormat()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendList("name", new[] { new DateTime(2020, 1, 2), new DateTime(2021, 3, 4) }, true, ",", false, "date");

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=2020-01-02&name=2021-03-04");
        }

        [Fact]
        public void ToString_ListDelimited_Concatenates()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendList("name", new[] { "value1", "value2" }, false, "%20", false);

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
            builder.AppendList("name", new[] { "value1", "value2 " }, false, "%20", true);

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
            builder.AppendList("name", new[] { "value1", "value2 " }, false, "%20", false);

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=value1%20value2%20");
        }

        [Fact]
        public void ToString_DateListDelimited_ConcatenatesWithDateFormat()
        {
            // Arrange

            var builder = new QueryStringBuilder("base/uri");
            builder.AppendList("name", new[] { new DateTime(2020, 1, 2), new DateTime(2021, 3, 4) }, false, "%20", false, "date");

            // Act

            var result = builder.ToString();

            // Assert

            result.Should().Be("base/uri?name=2020-01-02%202021-03-04");
        }

        #endregion

        #region AppendQueryParameter

        [Fact]
        public void AppendQueryParameter_NullUri_ReturnsNull()
        {
            // Act

            var result = QueryStringBuilder.AppendQueryParameter(null, "name", "value");

            // Assert

            result.Should().BeNull();
        }

        [Fact]
        public void AppendQueryParameter_UriWithoutQuery_AppendsWithQuestionMark()
        {
            // Act

            var result = QueryStringBuilder.AppendQueryParameter(new Uri("http://localhost"), "name", "value");

            // Assert

            result.Query.Should().Be("?name=value");
        }

        [Fact]
        public void AppendQueryParameter_UriWithQuery_AppendsWithAmpersand()
        {
            // Act

            var result = QueryStringBuilder.AppendQueryParameter(new Uri("http://localhost?foo=bar"), "name", "value");

            // Assert

            result.Query.Should().Be("?foo=bar&name=value");
        }

        [Fact]
        public void AppendQueryParameter_SpecialCharacters_AppendsWithEscaping()
        {
            // Act

            var result = QueryStringBuilder.AppendQueryParameter(new Uri("http://localhost"), "my name", "foo bar");

            // Assert

            result.Query.Should().Be("?my%20name=foo%20bar");
        }

        #endregion
    }
}
