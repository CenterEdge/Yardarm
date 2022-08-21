using System.Collections.Generic;
using FluentAssertions;
using RootNamespace.Serialization;
using Xunit;

namespace Yardarm.Client.UnitTests.Serialization
{
    public class HeaderSerializerTests
    {
        #region Serialize

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_String_ReturnsString(bool explode)
        {
            // Act

            string result = HeaderSerializer.Instance.Serialize("test", explode);

            // Assert

            result.Should().Be("test");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_Integer_ReturnsString(bool explode)
        {
            // Act

            string result = HeaderSerializer.Instance.Serialize(105, explode);

            // Assert

            result.Should().Be("105");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_Long_ReturnsString(bool explode)
        {
            // Act

            string result = HeaderSerializer.Instance.Serialize(105L, explode);

            // Assert

            result.Should().Be("105");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_Float_ReturnsString(bool explode)
        {
            // Act

            string result = HeaderSerializer.Instance.Serialize(1.05f, explode);

            // Assert

            result.Should().Be("1.05");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_Double_ReturnsString(bool explode)
        {
            // Act

            string result = HeaderSerializer.Instance.Serialize(1.05, explode);

            // Assert

            result.Should().Be("1.05");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_True_ReturnsString(bool explode)
        {
            // Act

            string result = HeaderSerializer.Instance.Serialize(true, explode);

            // Assert

            result.Should().Be("true");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_False_ReturnsString(bool explode)
        {
            // Act

            string result = HeaderSerializer.Instance.Serialize(false, explode);

            // Assert

            result.Should().Be("false");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Serialize_List_ReturnsCommaDelimitedString(bool explode)
        {
            // Act

            string result = HeaderSerializer.Instance.Serialize(
                new List<string> { "abc", "def", "ghi" }, explode);

            // Assert

            result.Should().Be("abc,def,ghi");
        }

        #endregion

        #region Serialize

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Deserialize_String_ReturnsString(bool explode)
        {
            // Act

            string result = HeaderSerializer.Instance.Deserialize<string>(
                new [] {"test"}, explode);

            // Assert

            result.Should().Be("test");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Deserialize_MultipleStrings_ReturnsJoined(bool explode)
        {
            // Act

            string result = HeaderSerializer.Instance.Deserialize<string>(
                new [] {"test", "value"}, explode);

            // Assert

            result.Should().Be("test,value");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Deserialize_Integer_ReturnsString(bool explode)
        {
            // Act

            int result = HeaderSerializer.Instance.Deserialize<int>(
                new [] {"105"}, explode);

            // Assert

            result.Should().Be(105);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Deserialize_Long_ReturnsString(bool explode)
        {
            // Act

            long result = HeaderSerializer.Instance.Deserialize<long>(
                new [] {"105"}, explode);

            // Assert

            result.Should().Be(105L);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Deserialize_Float_ReturnsString(bool explode)
        {
            // Act

            float result = HeaderSerializer.Instance.Deserialize<float>(
                new [] {"1.05"}, explode);

            // Assert

            result.Should().Be(1.05f);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Deserialize_Double_ReturnsString(bool explode)
        {
            // Act

            double result = HeaderSerializer.Instance.Deserialize<double>(
                new [] {"1.05"}, explode);

            // Assert

            result.Should().Be(1.05);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Deserialize_True_ReturnsString(bool explode)
        {
            // Act

            bool result = HeaderSerializer.Instance.Deserialize<bool>(
                new [] {"true"}, explode);

            // Assert

            result.Should().Be(true);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Deserialize_False_ReturnsString(bool explode)
        {
            // Act

            bool result = HeaderSerializer.Instance.Deserialize<bool>(
                new[] {"false"}, explode);

            // Assert

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Deserialize_ListOfStrings_ReturnsCommaDelimitedString(bool explode)
        {
            // Act

            List<string> result = HeaderSerializer.Instance.Deserialize<List<string>>(
                new[] { "abc", "def", "ghi" }, explode);

            // Assert

            result.Should().BeEquivalentTo("abc", "def", "ghi");
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Deserialize_ListOfIntegers_ReturnsCommaDelimitedString(bool explode)
        {
            // Act

            List<int> result = HeaderSerializer.Instance.Deserialize<List<int>>(
                new[] { "1", "3", "5" }, explode);

            // Assert

            result.Should().BeEquivalentTo(new[] { 1, 3, 5 });
        }

        #endregion
    }
}
