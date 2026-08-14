using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.OpenApi;
using Xunit;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Spec;

public class TagFilteringTests
{
    [Theory]
    [InlineData("badge")]
    [InlineData("audience")]
    public void FilterDocumentationTags_TopLevelDocumentationTag_ExcludesOperationTag(string kind)
    {
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>
            {
                new() { Name = "Documentation", Kind = kind }
            }
        };
        var tags = new[] { new OpenApiTag { Name = "Documentation", Kind = kind }.CreateRoot("0") };

        IEnumerable<ILocatedOpenApiElement<IOpenApiTag>> result = tags.FilterDocumentationTags(document);

        result.Should().BeEmpty();
    }

    [Fact]
    public void FilterDocumentationTags_HasTopLevelNavigationTag_IncludesOnlyNavigationTags()
    {
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>
            {
                new() { Name = "API", Kind = "nav" },
                new() { Name = "Other", Kind = "other" }
            }
        };
        var tags = new ILocatedOpenApiElement<IOpenApiTag>[]
        {
            new OpenApiTag { Name = "API", Kind = "nav" }.CreateRoot("0"),
            new OpenApiTag { Name = "Other", Kind = "other" }.CreateRoot("1"),
            new OpenApiTag { Name = "Unspecified" }.CreateRoot("2")
        };

        IEnumerable<ILocatedOpenApiElement<IOpenApiTag>> result = tags.FilterDocumentationTags(document);

        result.Select(p => p.Element.Name).Should().Equal("API");
    }

    [Fact]
    public void FilterDocumentationTags_WithoutNavigationTag_IncludesNonDocumentationTags()
    {
        var document = new OpenApiDocument
        {
            Tags = new HashSet<OpenApiTag>
            {
                new() { Name = "Other", Kind = "other" }
            }
        };
        var tags = new ILocatedOpenApiElement<IOpenApiTag>[]
        {
            new OpenApiTag { Name = "Other", Kind = "other" }.CreateRoot("0"),
            new OpenApiTag { Name = "Unspecified" }.CreateRoot("1")
        };

        IEnumerable<ILocatedOpenApiElement<IOpenApiTag>> result = tags.FilterDocumentationTags(document);

        result.Select(p => p.Element.Name).Should().Equal("Other", "Unspecified");
    }
}
