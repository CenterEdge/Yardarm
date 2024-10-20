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

        [Fact]
        public void Serialize_SimpleString_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", "test", PathSegmentStyle.Simple);

            // Assert

            result.Should().Be("test");
        }

        [Fact]
        public void Serialize_SimpleInteger_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 105, PathSegmentStyle.Simple);

            // Assert

            result.Should().Be("105");
        }

        [Fact]
        public void Serialize_SimpleLong_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 105L, PathSegmentStyle.Simple);

            // Assert

            result.Should().Be("105");
        }

        [Fact]
        public void Serialize_SimpleFloat_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 1.05f, PathSegmentStyle.Simple);

            // Assert

            result.Should().Be("1.05");
        }

        [Fact]
        public void Serialize_SimpleDouble_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 1.05, PathSegmentStyle.Simple);

            // Assert

            result.Should().Be("1.05");
        }

        [Fact]
        public void Serialize_SimpleTrue_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", true, PathSegmentStyle.Simple);

            // Assert

            result.Should().Be("true");
        }

        [Fact]
        public void Serialize_SimpleFalse_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", false, PathSegmentStyle.Simple);

            // Assert

            result.Should().Be("false");
        }

        [Fact]
        public void Serialize_SimpleDate_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id",
                new DateTime(2020, 1, 2), PathSegmentStyle.Simple, "date");

            // Assert

            result.Should().Be("2020-01-02");
        }

        [Fact]
        public void Serialize_SimpleDateTime_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id",
                new DateTime(2020, 1, 2, 3, 4, 5), PathSegmentStyle.Simple, "date-time");

            // Assert

            result.Should().Be("2020-01-02T03:04:05.0000000");
        }

        [Theory]
        [InlineData(null)]
        [InlineData( "date-time")]
        public void Serialize_SimpleDateTimeOffset_ReturnsString(string format)
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id",
                new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(-4)),
                PathSegmentStyle.Simple,  format);

            // Assert

            result.Should().Be("2020-01-02T03:04:05.0000000-04:00");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_SimpleList_ReturnsCommaDelimitedString(bool explode)
        {
            // Act

            string result = PathSegmentSerializer.SerializeList("id",
                new List<string> { "abc", "def", "ghi" }, PathSegmentStyle.Simple, explode);

            // Assert

            result.Should().Be("abc,def,ghi");
        }

        #endregion

        #region Label

        [Fact]
        public void Serialize_LabelString_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", "test", PathSegmentStyle.Label);

            // Assert

            result.Should().Be(".test");
        }

        [Fact]
        public void Serialize_LabelInteger_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 105, PathSegmentStyle.Label);

            // Assert

            result.Should().Be(".105");
        }

        [Fact]
        public void Serialize_LabelLong_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 105L, PathSegmentStyle.Label);

            // Assert

            result.Should().Be(".105");
        }

        [Fact]
        public void Serialize_LabelFloat_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 1.05f, PathSegmentStyle.Label);

            // Assert

            result.Should().Be(".1.05");
        }

        [Fact]
        public void Serialize_LabelDouble_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 1.05, PathSegmentStyle.Label);

            // Assert

            result.Should().Be(".1.05");
        }

        [Fact]
        public void Serialize_LabelTrue_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", true, PathSegmentStyle.Label);

            // Assert

            result.Should().Be(".true");
        }

        [Fact]
        public void Serialize_LabelFalse_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", false, PathSegmentStyle.Label);

            // Assert

            result.Should().Be(".false");
        }

        [Fact]
        public void Serialize_LabelDateTime_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id",
                new DateTime(2020, 1, 2), PathSegmentStyle.Label, "date");

            // Assert

            result.Should().Be(".2020-01-02");
        }

        [Theory]
        [InlineData( null)]
        [InlineData("date-time")]
        public void Serialize_LabelDateTimeOffset_ReturnsString(string format)
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id",
                new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(-4)),
                PathSegmentStyle.Label, format);

            // Assert

            result.Should().Be(".2020-01-02T03:04:05.0000000-04:00");
        }

        [Fact]
        public void Serialize_LabelListExplodeFalse_ReturnsCommaDelimitedString()
        {
            // Act

            string result = PathSegmentSerializer.SerializeList("id",
                new List<string> { "abc", "def", "ghi" }, PathSegmentStyle.Label, false);

            // Assert

            result.Should().Be(".abc.def.ghi");
        }

        [Fact]
        public void Serialize_LabelListExplodeTrue_ReturnsPeriodDelimitedString()
        {
            // Act

            string result = PathSegmentSerializer.SerializeList("id",
                new List<string> { "abc", "def", "ghi" }, PathSegmentStyle.Label, true);

            // Assert

            result.Should().Be(".abc.def.ghi");
        }

        #endregion

        #region Matrix

        [Fact]
        public void Serialize_MatrixString_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", "test", PathSegmentStyle.Matrix);

            // Assert

            result.Should().Be(";id=test");
        }

        [Fact]
        public void Serialize_MatrixInteger_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 105, PathSegmentStyle.Matrix);

            // Assert

            result.Should().Be(";id=105");
        }

        [Fact]
        public void Serialize_MatrixLong_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 105L, PathSegmentStyle.Matrix);

            // Assert

            result.Should().Be(";id=105");
        }

        [Fact]
        public void Serialize_MatrixFloat_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 1.05f, PathSegmentStyle.Matrix);

            // Assert

            result.Should().Be(";id=1.05");
        }

        [Fact]
        public void Serialize_MatrixDouble_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", 1.05, PathSegmentStyle.Matrix);

            // Assert

            result.Should().Be(";id=1.05");
        }

        [Fact]
        public void Serialize_MatrixTrue_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", true, PathSegmentStyle.Matrix);

            // Assert

            result.Should().Be(";id=true");
        }

        [Fact]
        public void Serialize_MatrixFalse_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id", false, PathSegmentStyle.Matrix);

            // Assert

            result.Should().Be(";id=false");
        }

        [Fact]
        public void Serialize_MatrixDateTime_ReturnsString()
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id",
                new DateTime(2020, 1, 2), PathSegmentStyle.Matrix, "date");

            // Assert

            result.Should().Be(";id=2020-01-02");
        }

        [Theory]
        [InlineData( null)]
        [InlineData("date-time")]
        public void Serialize_MatrixDateTimeOffset_ReturnsString(string format)
        {
            // Act

            string result = PathSegmentSerializer.Serialize("id",
                new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(-4)),
                PathSegmentStyle.Matrix, format);

            // Assert

            result.Should().Be(";id=2020-01-02T03:04:05.0000000-04:00");
        }

        [Fact]
        public void Serialize_MatrixListExplodeFalse_ReturnsCommaDelimitedString()
        {
            // Act

            string result = PathSegmentSerializer.SerializeList("id",
                new List<string> { "abc", "def", "ghi" }, PathSegmentStyle.Matrix, false);

            // Assert

            result.Should().Be(";id=abc,def,ghi");
        }

        [Fact]
        public void Serialize_MatrixListExplodeTrue_ReturnsMultipleIds()
        {
            // Act

            string result = PathSegmentSerializer.SerializeList("id",
                new List<string> { "abc", "def", "ghi" }, PathSegmentStyle.Matrix, true);

            // Assert

            result.Should().Be(";id=abc;id=def;id=ghi");
        }

        #endregion

        #region Build

        [Fact]
        public void Build_Simple_ExpectedResult()
        {
            // Arrange

            var id = 5;
            var str = "test";

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{id}/{str}");

            // Assert

            result.Should().Be("/api/widget/5/test");
        }

        [Fact]
        public void Build_SimpleWithFormat_ExpectedResult()
        {
            // Arrange

            var dateTime = new DateTime(2020, 1, 2);
            var str = "test";

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{dateTime:date}/{str}");

            // Assert

            result.Should().Be("/api/widget/2020-01-02/test");
        }

        [Fact]
        public void Build_Label_ExpectedResult()
        {
            // Arrange

            var id = 5;
            var str = "test";

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{id,1}/{str,1}");

            // Assert

            result.Should().Be("/api/widget/.5/.test");
        }

        [Fact]
        public void Build_LabelWithFormat_ExpectedResult()
        {
            // Arrange

            var dateTime = new DateTime(2020, 1, 2);
            var str = "test";

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{dateTime,1:date}/{str,1}");

            // Assert

            result.Should().Be("/api/widget/.2020-01-02/.test");
        }

        [Fact]
        public void Build_Matrix_ExpectedResult()
        {
            // Arrange

            var id = 5;
            var str = "test";

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{id,4:id}/{str,5:str}");

            // Assert

            result.Should().Be("/api/widget/;id=5/;str=test");
        }

        [Fact]
        public void Build_MatrixWithFormat_ExpectedResult()
        {
            // Arrange

            var dateTime = new DateTime(2020, 1, 2);
            var str = "test";

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{dateTime,4:dtdate}/{str,5:str}");

            // Assert

            result.Should().Be("/api/widget/;dt=2020-01-02/;str=test");
        }

        #endregion

        #region Build List

        [Fact]
        public void Build_SimpleList_ExpectedResult()
        {
            // Arrange

            List<int> id = [5, 6];
            List<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{id}/{str}");

            // Assert

            result.Should().Be("/api/widget/5,6/test,foo");
        }

        [Fact]
        public void Build_SimpleListWithFormat_ExpectedResult()
        {
            // Arrange

            List<DateTime> dateTime = [new DateTime(2020, 1, 2), new DateTime(2020, 1, 3)];
            List<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{dateTime:date}/{str}");

            // Assert

            result.Should().Be("/api/widget/2020-01-02,2020-01-03/test,foo");
        }

        [Fact]
        public void Build_LabelList_ExpectedResult()
        {
            // Arrange

            List<int> id = [5, 6];
            List<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{id,1}/{str,1}");

            // Assert

            result.Should().Be("/api/widget/.5.6/.test.foo");
        }

        [Fact]
        public void Build_LabelListWithFormat_ExpectedResult()
        {
            // Arrange

            List<DateTime> dateTime = [new DateTime(2020, 1, 2), new DateTime(2020, 1, 3)];
            List<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{dateTime,1:date}/{str,1}");

            // Assert

            result.Should().Be("/api/widget/.2020-01-02.2020-01-03/.test.foo");
        }

        [Fact]
        public void Build_MatrixList_ExpectedResult()
        {
            // Arrange

            List<int> id = [5, 6];
            List<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{id,4:id}/{str,5:str}");

            // Assert

            result.Should().Be("/api/widget/;id=5,6/;str=test,foo");
        }

        [Fact]
        public void Build_MatrixListWithFormat_ExpectedResult()
        {
            // Arrange

            List<DateTime> dateTime = [new DateTime(2020, 1, 2), new DateTime(2020, 1, 3)];
            List<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{dateTime,4:dtdate}/{str,5:str}");

            // Assert

            result.Should().Be("/api/widget/;dt=2020-01-02,2020-01-03/;str=test,foo");
        }

        [Fact]
        public void Build_MatrixListExploded_ExpectedResult()
        {
            // Arrange

            List<int> id = [5, 6];
            List<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{id,-4:id}/{str,-5:str}");

            // Assert

            result.Should().Be("/api/widget/;id=5;id=6/;str=test;str=foo");
        }

        [Fact]
        public void Build_MatrixListExplodedWithFormat_ExpectedResult()
        {
            // Arrange

            List<DateTime> dateTime = [new DateTime(2020, 1, 2), new DateTime(2020, 1, 3)];
            List<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{dateTime,-4:dtdate}/{str,-5:str}");

            // Assert

            result.Should().Be("/api/widget/;dt=2020-01-02;dt=2020-01-03/;str=test;str=foo");
        }

        #endregion

        #region Build IEnumerable<T>

        [Fact]
        public void Build_SimpleIEnumerable_ExpectedResult()
        {
            // Arrange

            IEnumerable<int> id = [5, 6];
            IEnumerable<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{id}/{str}");

            // Assert

            result.Should().Be("/api/widget/5,6/test,foo");
        }

        [Fact]
        public void Build_SimpleIEnumerableWithFormat_ExpectedResult()
        {
            // Arrange

            IEnumerable<DateTime> dateTime = [new DateTime(2020, 1, 2), new DateTime(2020, 1, 3)];
            IEnumerable<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{dateTime:date}/{str}");

            // Assert

            result.Should().Be("/api/widget/2020-01-02,2020-01-03/test,foo");
        }

        [Fact]
        public void Build_LabelIEnumerable_ExpectedResult()
        {
            // Arrange

            IEnumerable<int> id = [5, 6];
            IEnumerable<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{id,1}/{str,1}");

            // Assert

            result.Should().Be("/api/widget/.5.6/.test.foo");
        }

        [Fact]
        public void Build_LabelIEnumerableWithFormat_ExpectedResult()
        {
            // Arrange

            IEnumerable<DateTime> dateTime = [new DateTime(2020, 1, 2), new DateTime(2020, 1, 3)];
            IEnumerable<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{dateTime,1:date}/{str,1}");

            // Assert

            result.Should().Be("/api/widget/.2020-01-02.2020-01-03/.test.foo");
        }

        [Fact]
        public void Build_MatrixIEnumerable_ExpectedResult()
        {
            // Arrange

            IEnumerable<int> id = [5, 6];
            IEnumerable<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{id,4:id}/{str,5:str}");

            // Assert

            result.Should().Be("/api/widget/;id=5,6/;str=test,foo");
        }

        [Fact]
        public void Build_MatrixIEnumerableWithFormat_ExpectedResult()
        {
            // Arrange

            IEnumerable<DateTime> dateTime = [new DateTime(2020, 1, 2), new DateTime(2020, 1, 3)];
            IEnumerable<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{dateTime,4:dtdate}/{str,5:str}");

            // Assert

            result.Should().Be("/api/widget/;dt=2020-01-02,2020-01-03/;str=test,foo");
        }

        [Fact]
        public void Build_MatrixIEnumerableExploded_ExpectedResult()
        {
            // Arrange

            IEnumerable<int> id = [5, 6];
            IEnumerable<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{id,-4:id}/{str,-5:str}");

            // Assert

            result.Should().Be("/api/widget/;id=5;id=6/;str=test;str=foo");
        }

        [Fact]
        public void Build_MatrixIEnumerableExplodedWithFormat_ExpectedResult()
        {
            // Arrange

            IEnumerable<DateTime> dateTime = [new DateTime(2020, 1, 2), new DateTime(2020, 1, 3)];
            IEnumerable<string> str = ["test", "foo"];

            // Act

            var result = PathSegmentSerializer.Build($"/api/widget/{dateTime,-4:dtdate}/{str,-5:str}");

            // Assert

            result.Should().Be("/api/widget/;dt=2020-01-02;dt=2020-01-03/;str=test;str=foo");
        }

        #endregion
    }
}
