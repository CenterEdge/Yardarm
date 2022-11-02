using System.Collections.Generic;
using FluentAssertions;
using RootNamespace.Serialization;
using Xunit;

namespace Yardarm.Client.UnitTests.Serialization
{
    public class HeaderSerializerTests
    {
        #region SerializePrimitive

        [Fact]
        public void SerializePrimitive_String_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.Instance.SerializePrimitive("test");

            // Assert

            result.Should().Be("test");
        }

        [Fact]
        public void SerializePrimitive_Integer_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.Instance.SerializePrimitive(105);

            // Assert

            result.Should().Be("105");
        }

        [Fact]
        public void SerializePrimitive_Long_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.Instance.SerializePrimitive(105L);

            // Assert

            result.Should().Be("105");
        }

        [Fact]
        public void SerializePrimitive_Float_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.Instance.SerializePrimitive(1.05f);

            // Assert

            result.Should().Be("1.05");
        }

        [Fact]
        public void SerializePrimitive_Double_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.Instance.SerializePrimitive(1.05);

            // Assert

            result.Should().Be("1.05");
        }

        [Fact]
        public void SerializePrimitive_True_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.Instance.SerializePrimitive(true);

            // Assert

            result.Should().Be("true");
        }

        [Fact]
        public void SerializePrimitive_False_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.Instance.SerializePrimitive(false);

            // Assert

            result.Should().Be("false");
        }

        #endregion

        #region SerializeList

        [Fact]
        public void SerializeList_List_ReturnsCommaDelimitedString()
        {
            // Act

            string result = HeaderSerializer.Instance.SerializeList(
                new List<string> { "abc", "def", "ghi" });

            // Assert

            result.Should().Be("abc,def,ghi");
        }

        #endregion

        #region DeserializePrimitive

        [Fact]
        public void DeserializePrimitive_String_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.Instance.DeserializePrimitive<string>(
                new [] {"test"});

            // Assert

            result.Should().Be("test");
        }

        [Fact]
        public void DeserializePrimitive_MultipleStrings_ReturnsJoined()
        {
            // Act

            string result = HeaderSerializer.Instance.DeserializePrimitive<string>(
                new [] {"test", "value"});

            // Assert

            result.Should().Be("test,value");
        }

        [Fact]
        public void DeserializePrimitive_Integer_ReturnsString()
        {
            // Act

            int result = HeaderSerializer.Instance.DeserializePrimitive<int>(
                new [] {"105"});

            // Assert

            result.Should().Be(105);
        }

        [Fact]
        public void DeserializePrimitive_Long_ReturnsString()
        {
            // Act

            long result = HeaderSerializer.Instance.DeserializePrimitive<long>(
                new [] {"105"});

            // Assert

            result.Should().Be(105L);
        }

        [Fact]
        public void DeserializePrimitive_Float_ReturnsString()
        {
            // Act

            float result = HeaderSerializer.Instance.DeserializePrimitive<float>(
                new [] {"1.05"});

            // Assert

            result.Should().Be(1.05f);
        }

        [Fact]
        public void DeserializePrimitive_Double_ReturnsString()
        {
            // Act

            double result = HeaderSerializer.Instance.DeserializePrimitive<double>(
                new [] {"1.05"});

            // Assert

            result.Should().Be(1.05);
        }

        [Fact]
        public void DeserializePrimitive_True_ReturnsString()
        {
            // Act

            bool result = HeaderSerializer.Instance.DeserializePrimitive<bool>(
                new [] {"true"});

            // Assert

            result.Should().Be(true);
        }

        [Fact]
        public void DeserializePrimitive_False_ReturnsString()
        {
            // Act

            bool result = HeaderSerializer.Instance.DeserializePrimitive<bool>(
                new[] {"false"});

            // Assert

            result.Should().BeFalse();
        }

        #endregion

        #region DeserializeList

        [Fact]
        public void DeserializeList_ListOfStrings_ReturnsStrings()
        {
            // Act

            List<string> result = HeaderSerializer.Instance.DeserializeList<string>(
                new[] { "abc", "def", "ghi" });

            // Assert

            result.Should().BeEquivalentTo("abc", "def", "ghi");
        }

        [Fact]
        public void DeserializeList_ListOfIntegers_ReturnsIntegers()
        {
            // Act

            List<int> result = HeaderSerializer.Instance.DeserializeList<int>(
                new[] { "1", "3", "5" });

            // Assert

            result.Should().BeEquivalentTo(new[] { 1, 3, 5 });
        }

        #endregion
    }
}
