using System;
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

            string result = HeaderSerializer.SerializePrimitive("test");

            // Assert

            result.Should().Be("test");
        }

        [Fact]
        public void SerializePrimitive_Integer_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.SerializePrimitive(105);

            // Assert

            result.Should().Be("105");
        }

        [Fact]
        public void SerializePrimitive_Long_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.SerializePrimitive(105L);

            // Assert

            result.Should().Be("105");
        }

        [Fact]
        public void SerializePrimitive_Float_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.SerializePrimitive(1.05f);

            // Assert

            result.Should().Be("1.05");
        }

        [Fact]
        public void SerializePrimitive_Double_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.SerializePrimitive(1.05);

            // Assert

            result.Should().Be("1.05");
        }

        [Fact]
        public void SerializePrimitive_True_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.SerializePrimitive(true);

            // Assert

            result.Should().Be("true");
        }

        [Fact]
        public void SerializePrimitive_False_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.SerializePrimitive(false);

            // Assert

            result.Should().Be("false");
        }

        [Fact]
        public void SerializePrimitive_DateTime_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.SerializePrimitive(new DateTime(2020, 1, 2, 3, 4, 5));

            // Assert

            result.Should().Be("2020-01-02T03:04:05.0000000");
        }

        [Fact]
        public void SerializePrimitive_Date_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.SerializePrimitive(new DateTime(2020, 1, 2, 3, 4, 5), "date");

            // Assert

            result.Should().Be("2020-01-02");
        }

        #endregion

        #region SerializeList

        [Fact]
        public void SerializeList_List_ReturnsCommaDelimitedString()
        {
            // Act

            string result = HeaderSerializer.SerializeList(
                new List<string> { "abc", "def", "ghi" });

            // Assert

            result.Should().Be("abc,def,ghi");
        }

        [Fact]
        public void SerializeList_DateList_ReturnsCommaDelimitedString()
        {
            // Act

            string result = HeaderSerializer.SerializeList(
                new List<DateTime> {
                    new(2020, 01, 02, 3, 4, 5),
                    new(2021, 01, 02, 3, 4, 5)
                }, "date");

            // Assert

            result.Should().Be("2020-01-02,2021-01-02");
        }

        #endregion

        #region DeserializePrimitive

        [Fact]
        public void DeserializePrimitive_String_ReturnsString()
        {
            // Act

            string result = HeaderSerializer.DeserializePrimitive<string>(
                ["test"]);

            // Assert

            result.Should().Be("test");
        }

        [Fact]
        public void DeserializePrimitive_MultipleStrings_ReturnsJoined()
        {
            // Act

            string result = HeaderSerializer.DeserializePrimitive<string>(
                ["test", "value"]);

            // Assert

            result.Should().Be("test,value");
        }

        [Fact]
        public void DeserializePrimitive_Integer_ReturnsString()
        {
            // Act

            int result = HeaderSerializer.DeserializePrimitive<int>(
                ["105"]);

            // Assert

            result.Should().Be(105);
        }

        [Fact]
        public void DeserializePrimitive_Long_ReturnsString()
        {
            // Act

            long result = HeaderSerializer.DeserializePrimitive<long>(
                ["105"]);

            // Assert

            result.Should().Be(105L);
        }

        [Fact]
        public void DeserializePrimitive_Float_ReturnsString()
        {
            // Act

            float result = HeaderSerializer.DeserializePrimitive<float>(
                ["1.05"]);

            // Assert

            result.Should().Be(1.05f);
        }

        [Fact]
        public void DeserializePrimitive_Double_ReturnsString()
        {
            // Act

            double result = HeaderSerializer.DeserializePrimitive<double>(
                ["1.05"]);

            // Assert

            result.Should().Be(1.05);
        }

        [Fact]
        public void DeserializePrimitive_True_ReturnsString()
        {
            // Act

            bool result = HeaderSerializer.DeserializePrimitive<bool>(
                ["true"]);

            // Assert

            result.Should().Be(true);
        }

        [Fact]
        public void DeserializePrimitive_False_ReturnsString()
        {
            // Act

            bool result = HeaderSerializer.DeserializePrimitive<bool>(
                ["false"]);

            // Assert

            result.Should().BeFalse();
        }

        [Fact]
        public void DeserializePrimitive_Date_ReturnsString()
        {
            // Act

            var result = HeaderSerializer.DeserializePrimitive<DateTime>(
                ["2020-01-02"], "date");

            // Assert

            result.Should().Be(new DateTime(2020, 01, 02));
        }

        [Fact]
        public void DeserializePrimitive_DateWithUnexpectedTime_FormatException()
        {
            // Act/Assert

            var action = () => HeaderSerializer.DeserializePrimitive<DateTime>(
                ["2020-01-02T03:04:05-04:00"], "date");
            action.Should().Throw<FormatException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("date-time")]
        public void DeserializePrimitive_DateTimeOffset_ReturnsString(string format)
        {
            // Act

            var result = HeaderSerializer.DeserializePrimitive<DateTimeOffset>(
                ["2020-01-02T03:04:05-04:00"], format);

            // Assert

            result.Should().Be(new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(-4)));
        }

        #endregion

        #region DeserializeList

        [Fact]
        public void DeserializeList_ListOfStrings_ReturnsStrings()
        {
            // Act

            List<string> result = HeaderSerializer.DeserializeList<string>(
                ["abc", "def", "ghi"]);

            // Assert

            result.Should().BeEquivalentTo("abc", "def", "ghi");
        }

        [Fact]
        public void DeserializeList_ListOfIntegers_ReturnsIntegers()
        {
            // Act

            List<int> result = HeaderSerializer.DeserializeList<int>(
                ["1", "3", "5"]);

            // Assert

            result.Should().BeEquivalentTo([1, 3, 5]);
        }

        #endregion
    }
}
