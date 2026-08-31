using System.Linq;
using FluentAssertions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using NuGet.Frameworks;
using Xunit;
using Yardarm.Generation.Request;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.Request
{
    public class HttpMethodPropertyGeneratorTests
    {
        [Theory]
        [InlineData("net10.0", "QUERY", "global::System.Net.Http.HttpMethod.Query")]
        [InlineData("netstandard2.0", "QUERY", "new global::System.Net.Http.HttpMethod(\"QUERY\")")]
        [InlineData("net10.0", "LINK", "new global::System.Net.Http.HttpMethod(\"LINK\")")]
        public void Generate_ExtendedMethod_ExpectedHttpMethod(string targetFramework, string method, string expectedExpression)
        {
            // Arrange

            var pathItem = new OpenApiPathItem();
            var operation = new OpenApiOperation();
            pathItem.AddOperation(new System.Net.Http.HttpMethod(method), operation);
            ILocatedOpenApiElement<OpenApiOperation> locatedOperation =
                pathItem.CreateRoot("/things").GetOperations().Single();

            // Act

            var services = new ServiceCollection();
            services.AddOptions();
            services.AddSingleton(new YardarmGenerationSettings());
            var context = new GenerationContext(services.BuildServiceProvider())
            {
                CurrentTargetFramework = NuGetFramework.Parse(targetFramework)
            };
            var result = new HttpMethodPropertyGenerator(context).Generate(locatedOperation, null).Single();

            // Assert

            result.NormalizeWhitespace().ToFullString().Should()
                .Be($"protected override global::System.Net.Http.HttpMethod Method => {expectedExpression};");
        }
    }
}
