using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using RootNamespace.Serialization;
using Xunit;

namespace Yardarm.Client.UnitTests.Serialization
{
    public class LiteralSerializerTests
    {
        #region Serialize

        [Fact]
        public void Serialize_String_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Instance.Serialize("test");

            // Assert

            result.Should().Be("test");
        }

        [Fact]
        public void Serialize_Integer_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Instance.Serialize(105);

            // Assert

            result.Should().Be("105");
        }

        [Fact]
        public void Serialize_Long_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Instance.Serialize(105L);

            // Assert

            result.Should().Be("105");
        }

        [Fact]
        public void Serialize_Float_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Instance.Serialize(1.05f);

            // Assert

            result.Should().Be("1.05");
        }

        [Fact]
        public void Serialize_Double_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Instance.Serialize(1.05);

            // Assert

            result.Should().Be("1.05");
        }

        [Fact]
        public void Serialize_True_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Instance.Serialize(true);

            // Assert

            result.Should().Be("true");
        }

        [Fact]
        public void Serialize_False_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Instance.Serialize(false);

            // Assert

            result.Should().Be("false");
        }

        [Fact]
        public void Serialize_DateTime_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Instance.Serialize(new DateTime(2020, 1, 2, 3, 4, 5));

            // Assert

            result.Should().Be("2020-01-02T03:04:05.0000000");
        }

        [Fact]
        public void Serialize_Date_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Instance.Serialize(new DateTime(2020, 1, 2, 3, 4, 5), "date");

            // Assert

            result.Should().Be("2020-01-02");
        }

        [Fact]
        public void Serialize_DateTimeOffset_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Instance.Serialize(
                new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(-4)));

            // Assert

            result.Should().Be("2020-01-02T03:04:05.0000000-04:00");
        }

        #endregion

        #region Deserialize

        [Fact]
        public void Deserialize_String_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Instance.Deserialize<string>("test");

            // Assert

            result.Should().Be("test");
        }

        [Fact]
        public void Deserialize_Integer_ReturnsString()
        {
            // Act

            int result = LiteralSerializer.Instance.Deserialize<int>("105");

            // Assert

            result.Should().Be(105);
        }

        [Fact]
        public void Deserialize_Long_ReturnsString()
        {
            // Act

            long result = LiteralSerializer.Instance.Deserialize<long>("105");

            // Assert

            result.Should().Be(105L);
        }

        [Fact]
        public void Deserialize_Float_ReturnsString()
        {
            // Act

            float result = LiteralSerializer.Instance.Deserialize<float>("1.05");

            // Assert

            result.Should().Be(1.05f);
        }

        [Fact]
        public void Deserialize_Double_ReturnsString()
        {
            // Act

            double result = LiteralSerializer.Instance.Deserialize<double>("1.05");

            // Assert

            result.Should().Be(1.05);
        }

        [Fact]
        public void Deserialize_True_ReturnsString()
        {
            // Act

            bool result = LiteralSerializer.Instance.Deserialize<bool>("true");

            // Assert

            result.Should().Be(true);
        }

        [Fact]
        public void Deserialize_False_ReturnsString()
        {
            // Act

            bool result = LiteralSerializer.Instance.Deserialize<bool>("false");

            // Assert

            result.Should().BeFalse();
        }

        [Fact]
        public void Deserialize_Date_ReturnsString()
        {
            // Act

            var result = LiteralSerializer.Instance.Deserialize<DateTime>("2020-01-02", "date");

            // Assert

            result.Should().Be(new DateTime(2020, 01, 02));
        }

        [Fact]
        public void Deserialize_DateWithUnexpectedTime_FormatException()
        {
            // Act/Assert

            var action = () => LiteralSerializer.Instance.Deserialize<DateTime>(
                "2020-01-02T03:04:05-04:00", "date");
            action.Should().Throw<FormatException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("date-time")]
        public void Deserialize_DateTimeOffset_ReturnsString(string format)
        {
            // Act

            var result = LiteralSerializer.Instance.Deserialize<DateTimeOffset>(
                "2020-01-02T03:04:05-04:00", format);

            // Assert

            result.Should().Be(new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(-4)));
        }

        #endregion

        #region JoinList

        [Fact]
        public void JoinList_Date_Serializes()
        {
            // Act

            var result = LiteralSerializer.Instance.JoinList(",",
                new[] {new DateTime(2020, 1, 2), new DateTime(2021, 2, 3)},
                typeof(DateTime), "date");

            // Assert

            result.Should().Be("2020-01-02,2021-02-03");
        }

        #endregion

        #region JoinListT

        [Fact]
        public void JoinListT_Date_Serializes()
        {
            // Act

            var result = LiteralSerializer.Instance.JoinList(",",
                new[] {new DateTime(2020, 1, 2), new DateTime(2021, 2, 3)},
                "date");

            // Assert

            result.Should().Be("2020-01-02,2021-02-03");
        }

        #endregion
    }
}
