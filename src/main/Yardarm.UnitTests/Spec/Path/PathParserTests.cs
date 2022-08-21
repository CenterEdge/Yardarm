using System;
using FluentAssertions;
using Xunit;
using Yardarm.Spec.Path;

namespace Yardarm.UnitTests.Spec.Path
{
    public class PathParserTests
    {
        [Fact]
        public void Parse_Null_ArgumentNullException()
        {
            // Act/Assert

            Action action = () => PathParser.Parse(null);
            action.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Parse_EmptyString_Empty()
        {
            // Act

            var result = PathParser.Parse("");

            // Assert

            result.Should().BeEmpty();
        }

        [Fact]
        public void Parse_JustText_ExpectedResult()
        {
            // Act

            var result = PathParser.Parse("/some/path");

            // Assert

            result.Should().BeEquivalentTo(new[]
            {
                new PathSegment("some/path", PathSegmentType.Text)
            });
        }

        [Fact]
        public void Parse_HasParameterWithHeader_ExpectedResult()
        {
            // Act

            var result = PathParser.Parse("/some/{path}");

            // Assert

            result.Should().BeEquivalentTo(new[]
            {
                new PathSegment("some/", PathSegmentType.Text),
                new PathSegment("path", PathSegmentType.Parameter)
            });
        }

        [Fact]
        public void Parse_HasParameterWithHeaderAndTrailer_ExpectedResult()
        {
            // Act

            var result = PathParser.Parse("/some/{path}/withtrailer");

            // Assert

            result.Should().BeEquivalentTo(new[]
            {
                new PathSegment("some/", PathSegmentType.Text),
                new PathSegment("path", PathSegmentType.Parameter),
                new PathSegment("/withtrailer", PathSegmentType.Text),
            });
        }

        [Fact]
        public void Parse_HasParameterWithTrailer_ExpectedResult()
        {
            // Act

            var result = PathParser.Parse("{path}/withtrailer");

            // Assert

            result.Should().BeEquivalentTo(new[]
            {
                new PathSegment("path", PathSegmentType.Parameter),
                new PathSegment("/withtrailer", PathSegmentType.Text),
            });
        }

        [Fact]
        public void Parse_MultipleParameters_ExpectedResult()
        {
            // Act

            var result = PathParser.Parse("/some/{path}/and/{someotherpath}/withtrailer");

            // Assert

            result.Should().BeEquivalentTo(new[]
            {
                new PathSegment("some/", PathSegmentType.Text),
                new PathSegment("path", PathSegmentType.Parameter),
                new PathSegment("/and/", PathSegmentType.Text),
                new PathSegment("someotherpath", PathSegmentType.Parameter),
                new PathSegment("/withtrailer", PathSegmentType.Text),
            });
        }

        [Fact]
        public void Parse_InvalidPath_InvalidOperationException()
        {
            // Act/Assert

            Action action = () => PathParser.Parse("/some/{path}/and/{someotherpath");
            action.Should().Throw<InvalidOperationException>();
        }
    }
}
