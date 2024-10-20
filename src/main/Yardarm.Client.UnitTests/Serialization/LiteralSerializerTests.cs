﻿using System;
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

            string result = LiteralSerializer.Serialize("test");

            // Assert

            result.Should().Be("test");
        }

        [Fact]
        public void Serialize_Integer_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Serialize(105);

            // Assert

            result.Should().Be("105");
        }

        [Fact]
        public void Serialize_Long_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Serialize(105L);

            // Assert

            result.Should().Be("105");
        }

        [Fact]
        public void Serialize_Float_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Serialize(1.05f);

            // Assert

            result.Should().Be("1.05");
        }

        [Fact]
        public void Serialize_Double_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Serialize(1.05);

            // Assert

            result.Should().Be("1.05");
        }

        [Fact]
        public void Serialize_True_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Serialize(true);

            // Assert

            result.Should().Be("true");
        }

        [Fact]
        public void Serialize_False_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Serialize(false);

            // Assert

            result.Should().Be("false");
        }

        [Fact]
        public void Serialize_DateTime_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Serialize(new DateTime(2020, 1, 2, 3, 4, 5));

            // Assert

            result.Should().Be("2020-01-02T03:04:05.0000000");
        }

        [Theory]
        [InlineData("date")]
        [InlineData("full-date")]
        public void Serialize_Date_ReturnsString(string format)
        {
            // Act

            string result = LiteralSerializer.Serialize(new DateTime(2020, 1, 2, 3, 4, 5), format);

            // Assert

            result.Should().Be("2020-01-02");
        }

        [Fact]
        public void Serialize_DateTimeOffset_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Serialize(
                new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(-4)));

            // Assert

            result.Should().Be("2020-01-02T03:04:05.0000000-04:00");
        }

        [Theory]
        [InlineData("partial-time")]
        [InlineData("date-span")]
        public void Serialize_TimeSpan_ReturnsString(string format)
        {
            // Act

            string result = LiteralSerializer.Serialize(
                new TimeSpan(0, 3, 4, 5), format);

            // Assert

            result.Should().Be("03:04:05");
        }

        [Theory]
        [InlineData("partial-time")]
        [InlineData("date-span")]
        public void Serialize_TimeSpanMillis_ReturnsString(string format)
        {
            // Act

            string result = LiteralSerializer.Serialize(
                new TimeSpan(0, 3, 4, 5, 123), format);

            // Assert

            result.Should().Be("03:04:05.1230000");
        }

        [Fact]
        public void Serialize_Guid_ReturnsString()
        {
            // Arrange

            var guid = Guid.NewGuid();

            // Act

            string result = LiteralSerializer.Serialize(guid);

            // Assert

            result.Should().Be(guid.ToString());
        }

        [Fact]
        public void Serialize_Enum_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Serialize(StringComparison.Ordinal);

            // Assert

            result.Should().Be("Ordinal");
        }

        #endregion

        #region Deserialize

        [Fact]
        public void Deserialize_String_ReturnsString()
        {
            // Act

            string result = LiteralSerializer.Deserialize<string>("test");

            // Assert

            result.Should().Be("test");
        }

        [Fact]
        public void Deserialize_Integer_ReturnsString()
        {
            // Act

            int result = LiteralSerializer.Deserialize<int>("105");

            // Assert

            result.Should().Be(105);
        }

        [Fact]
        public void Deserialize_Long_ReturnsString()
        {
            // Act

            long result = LiteralSerializer.Deserialize<long>("105");

            // Assert

            result.Should().Be(105L);
        }

        [Fact]
        public void Deserialize_Float_ReturnsString()
        {
            // Act

            float result = LiteralSerializer.Deserialize<float>("1.05");

            // Assert

            result.Should().Be(1.05f);
        }

        [Fact]
        public void Deserialize_Double_ReturnsString()
        {
            // Act

            double result = LiteralSerializer.Deserialize<double>("1.05");

            // Assert

            result.Should().Be(1.05);
        }

        [Fact]
        public void Deserialize_True_ReturnsString()
        {
            // Act

            bool result = LiteralSerializer.Deserialize<bool>("true");

            // Assert

            result.Should().Be(true);
        }

        [Fact]
        public void Deserialize_False_ReturnsString()
        {
            // Act

            bool result = LiteralSerializer.Deserialize<bool>("false");

            // Assert

            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("date")]
        [InlineData("full-date")]
        public void Deserialize_Date_ReturnsString(string format)
        {
            // Act

            var result = LiteralSerializer.Deserialize<DateTime>("2020-01-02", format);

            // Assert

            result.Should().Be(new DateTime(2020, 01, 02));
        }

        [Theory]
        [InlineData("partial-time")]
        [InlineData("date-span")]
        public void Deserialize_TimeSpan_ReturnsString(string format)
        {
            // Act

            var result = LiteralSerializer.Deserialize<TimeSpan>("13:01:02", format);

            // Assert

            result.Should().Be(new TimeSpan(0, 13, 1, 2));
        }

        [Theory]
        [InlineData("partial-time")]
        [InlineData("date-span")]
        public void Deserialize_TimeSpanWithMillis_ReturnsString(string format)
        {
            // Act

            var result = LiteralSerializer.Deserialize<TimeSpan>("13:01:02.234000", format);

            // Assert

            result.Should().Be(new TimeSpan(0, 13, 1, 2, 234));
        }

        [Fact]
        public void Deserialize_DateWithUnexpectedTime_FormatException()
        {
            // Act/Assert

            var action = () => LiteralSerializer.Deserialize<DateTime>(
                "2020-01-02T03:04:05-04:00", "date");
            action.Should().Throw<FormatException>();
        }

        [Theory]
        [InlineData(null)]
        [InlineData("date-time")]
        public void Deserialize_DateTimeOffset_ReturnsString(string format)
        {
            // Act

            var result = LiteralSerializer.Deserialize<DateTimeOffset>(
                "2020-01-02T03:04:05-04:00", format);

            // Assert

            result.Should().Be(new DateTimeOffset(2020, 1, 2, 3, 4, 5, TimeSpan.FromHours(-4)));
        }

        [Fact]
        public void Deserialize_Guid_ReturnsValue()
        {
            // Range

            const string guid = "00000001-0002-0003-0004-000000000005";
            // Act

            Guid result = LiteralSerializer.Deserialize<Guid>(guid);

            // Assert

            result.Should().Be(Guid.Parse(guid));
        }

        [Fact]
        public void Deserialize_Enum_ReturnsValue()
        {
            // Act

            StringComparison result = LiteralSerializer.Deserialize<StringComparison>("Ordinal");

            // Assert

            result.Should().Be(StringComparison.Ordinal);
        }

        #endregion

        #region JoinListT

        [Fact]
        public void JoinListT_Date_Serializes()
        {
            // Act

            var result = LiteralSerializer.JoinList(",",
                new[] {new DateTime(2020, 1, 2), new DateTime(2021, 2, 3)},
                "date");

            // Assert

            result.Should().Be("2020-01-02,2021-02-03");
        }

        #endregion
    }
}
