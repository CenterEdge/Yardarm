using FluentAssertions;
using Microsoft.OpenApi;
using Xunit;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Spec
{
    public class LocatedElementEqualityComparerTests
    {
        [Fact]
        public void IsReferenceEqualDefault_ResponseTypes_False()
        {
            LocatedElementEqualityComparer<IOpenApiResponse>.IsReferenceEqualDefault.Should().BeFalse();
            LocatedElementEqualityComparer<OpenApiResponse>.IsReferenceEqualDefault.Should().BeFalse();
        }

        [Fact]
        public void IsReferenceEqualDefault_RequestBodyTypes_False()
        {
            LocatedElementEqualityComparer<IOpenApiRequestBody>.IsReferenceEqualDefault.Should().BeFalse();
            LocatedElementEqualityComparer<OpenApiRequestBody>.IsReferenceEqualDefault.Should().BeFalse();
        }
    }
}
