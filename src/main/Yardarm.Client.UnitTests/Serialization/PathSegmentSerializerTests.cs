using System;
using System.Collections.Generic;
using FluentAssertions;
using RootNamespace.Serialization;
using Xunit;

namespace Yardarm.Client.UnitTests.Serialization
{
    public class PathSegmentSerializerTests
    {
        #region Simple

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_SimpleString_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", "test", PathSegmentStyle.Simple, explode);

            // Assert

            result.Should().Be("test");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_SimpleInteger_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 105, PathSegmentStyle.Simple, explode);

            // Assert

            result.Should().Be("105");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_SimpleLong_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 105L, PathSegmentStyle.Simple, explode);

            // Assert

            result.Should().Be("105");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_SimpleFloat_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 1.05f, PathSegmentStyle.Simple, explode);

            // Assert

            result.Should().Be("1.05");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_SimpleDouble_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 1.05, PathSegmentStyle.Simple, explode);

            // Assert

            result.Should().Be("1.05");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_SimpleTrue_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", true, PathSegmentStyle.Simple, explode);

            // Assert

            result.Should().Be("true");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_SimpleFalse_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", false, PathSegmentStyle.Simple, explode);

            // Assert

            result.Should().Be("false");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_SimpleDate_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new DateTime(2020, 1, 2), PathSegmentStyle.Simple, explode, "date");

            // Assert

            result.Should().Be("2020-01-02");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_SimpleDateTime_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new DateTime(2020, 1, 2, 3, 4, 5), PathSegmentStyle.Simple, explode, "date-time");

            // Assert

            result.Should().Be("2020-01-02T03:04:05.0000000");
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(true, null)]
        [InlineData(false, "date-time")]
        [InlineData(true, "date-time")]
        public void Serialize_SimpleDateTimeOffset_ReturnsString(bool explode, string format)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(-4)),
                PathSegmentStyle.Simple, explode, format);

            // Assert

            result.Should().Be("2020-01-02T03:04:05.0000000-04:00");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_SimpleList_ReturnsCommaDelimitedString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new List<string> { "abc", "def", "ghi" }, PathSegmentStyle.Simple, explode);

            // Assert

            result.Should().Be("abc,def,ghi");
        }

        #endregion

        #region Label

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_LabelString_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", "test", PathSegmentStyle.Label, explode);

            // Assert

            result.Should().Be(".test");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_LabelInteger_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 105, PathSegmentStyle.Label, explode);

            // Assert

            result.Should().Be(".105");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_LabelLong_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 105L, PathSegmentStyle.Label, explode);

            // Assert

            result.Should().Be(".105");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_LabelFloat_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 1.05f, PathSegmentStyle.Label, explode);

            // Assert

            result.Should().Be(".1.05");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_LabelDouble_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 1.05, PathSegmentStyle.Label, explode);

            // Assert

            result.Should().Be(".1.05");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_LabelTrue_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", true, PathSegmentStyle.Label, explode);

            // Assert

            result.Should().Be(".true");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_LabelFalse_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", false, PathSegmentStyle.Label, explode);

            // Assert

            result.Should().Be(".false");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_LabelDateTime_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new DateTime(2020, 1, 2), PathSegmentStyle.Label, explode, "date");

            // Assert

            result.Should().Be(".2020-01-02");
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(true, null)]
        [InlineData(false, "date-time")]
        [InlineData(true, "date-time")]
        public void Serialize_LabelDateTimeOffset_ReturnsString(bool explode, string format)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(-4)),
                PathSegmentStyle.Label, explode, format);

            // Assert

            result.Should().Be(".2020-01-02T03:04:05.0000000-04:00");
        }

        [Fact]
        public void Serialize_LabelListExplodeFalse_ReturnsCommaDelimitedString()
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new List<string> { "abc", "def", "ghi" }, PathSegmentStyle.Label, false);

            // Assert

            result.Should().Be(".abc,def,ghi");
        }

        [Fact]
        public void Serialize_LabelListExplodeTrue_ReturnsPeriodDelimitedString()
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new List<string> { "abc", "def", "ghi" }, PathSegmentStyle.Label, true);

            // Assert

            result.Should().Be(".abc.def.ghi");
        }

        #endregion

        #region Matrix

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_MatrixString_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", "test", PathSegmentStyle.Matrix, explode);

            // Assert

            result.Should().Be(";id=test");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_MatrixInteger_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 105, PathSegmentStyle.Matrix, explode);

            // Assert

            result.Should().Be(";id=105");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_MatrixLong_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 105L, PathSegmentStyle.Matrix, explode);

            // Assert

            result.Should().Be(";id=105");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_MatrixFloat_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 1.05f, PathSegmentStyle.Matrix, explode);

            // Assert

            result.Should().Be(";id=1.05");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_MatrixDouble_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", 1.05, PathSegmentStyle.Matrix, explode);

            // Assert

            result.Should().Be(";id=1.05");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_MatrixTrue_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", true, PathSegmentStyle.Matrix, explode);

            // Assert

            result.Should().Be(";id=true");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_MatrixFalse_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id", false, PathSegmentStyle.Matrix, explode);

            // Assert

            result.Should().Be(";id=false");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_MatrixDateTime_ReturnsString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new DateTime(2020, 1, 2), PathSegmentStyle.Matrix, explode, "date");

            // Assert

            result.Should().Be(";id=2020-01-02");
        }

        [Theory]
        [InlineData(false, null)]
        [InlineData(true, null)]
        [InlineData(false, "date-time")]
        [InlineData(true, "date-time")]
        public void Serialize_MatrixDateTimeOffset_ReturnsString(bool explode, string format)
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(-4)),
                PathSegmentStyle.Matrix, explode, format);

            // Assert

            result.Should().Be(";id=2020-01-02T03:04:05.0000000-04:00");
        }

        [Fact]
        public void Serialize_MatrixListExplodeFalse_ReturnsCommaDelimitedString()
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new List<string> { "abc", "def", "ghi" }, PathSegmentStyle.Matrix, false);

            // Assert

            result.Should().Be(";id=abc,def,ghi");
        }

        [Fact]
        public void Serialize_MatrixListExplodeTrue_ReturnsMultipleIds()
        {
            // Act

            string result = PathSegmentSerializer.Instance.Serialize("id",
                new List<string> { "abc", "def", "ghi" }, PathSegmentStyle.Matrix, true);

            // Assert

            result.Should().Be(";id=abc;id=def;id=ghi");
        }

        #endregion
    }
}
