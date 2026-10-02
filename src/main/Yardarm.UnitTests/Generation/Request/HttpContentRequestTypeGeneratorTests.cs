using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Xunit;
using Yardarm.Generation;
using Yardarm.Generation.Request;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.Request;

public class HttpContentRequestTypeGeneratorTests
{
    [Fact]
    public void Generate_UnsupportedRequestMediaType_GeneratesHttpContentRequest()
    {
        // Arrange

        var document = new OpenApiDocument
        {
            Info = new OpenApiInfo { Title = "Test", Version = "1.0" },
            Paths = new OpenApiPaths
            {
                ["/things"] = new OpenApiPathItem
                {
                    Operations =
                    {
                        [OperationType.Post] = new OpenApiOperation
                        {
                            OperationId = "addThing",
                            RequestBody = new OpenApiRequestBody
                            {
                                Content =
                                {
                                    ["application/xml"] = new OpenApiMediaType
                                    {
                                        Schema = new OpenApiSchema { Type = "string" }
                                    }
                                }
                            },
                            Responses = new OpenApiResponses
                            {
                                ["204"] = new OpenApiResponse { Description = "No Content" }
                            }
                        }
                    }
                }
            }
        };
        var registry = new YardarmGenerationSettings()
            .BuildServiceProvider(document)
            .GetRequiredService<ITypeGeneratorRegistry>();
        var operation = document.Paths.ToLocatedElements().GetOperations().Single();

        // Act

        var declarations = registry.Get(operation).Generate().OfType<ClassDeclarationSyntax>().ToList();

        // Assert

        var httpContentRequest = declarations.Should()
            .ContainSingle(p => p.Identifier.ValueText == "AddThingHttpContentRequest").Subject;
        httpContentRequest.Members.OfType<MethodDeclarationSyntax>()
            .Single(p => p.Identifier.ValueText == BuildContentMethodGenerator.BuildContentMethodName)
            .NormalizeWhitespace().ToFullString().Should()
            .Be("protected override global::System.Net.Http.HttpContent? BuildContent(Yardarm.Sdk.Requests.BuildRequestContext context) => Body;");
    }
}
