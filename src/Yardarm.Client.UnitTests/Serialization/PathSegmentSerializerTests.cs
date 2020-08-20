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
