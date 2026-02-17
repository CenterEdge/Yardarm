using System;
using System.Net.Http;
using RootNamespace.Requests;
using RootNamespace.Serialization;
using Xunit;

namespace Yardarm.Client.UnitTests.Requests;

public class OperationRequestTests
{
    [Fact]
    public void BuildRequest_AppliesOptions()
    {
        // Arrange

        var request = new TestOperationRequest();
        request.Options.Set(s_testOptionKey, "TestValue");

        var context = new BuildRequestContext(TypeSerializerRegistry.CreateDefaultRegistry());

        // Act

        var requestMessage = request.BuildRequest(context);

        // Assert

        Assert.True(requestMessage.Options.TryGetValue(s_testOptionKey, out string value));
        Assert.Equal("TestValue", value);
    }

    [Fact]
    public void BuildRequest_SucceedsWithNoOptions()
    {
        // Arrange

        var request = new TestOperationRequest();
        var context = new BuildRequestContext(TypeSerializerRegistry.CreateDefaultRegistry());

        // Act

        var requestMessage = request.BuildRequest(context);

        // Assert

        Assert.False(requestMessage.Options.TryGetValue(s_testOptionKey, out _));
    }

    [Fact]
    public void Options_DefaultInterfaceMember_UsesDimImplementation()
    {
        // Arrange
        IOperationRequest request = new DimOperationRequest();

        // Act
        request.Options.Set(s_testOptionKey, "TestValue");

        // Assert
        Assert.True(request.Options.TryGetValue(s_testOptionKey, out var value));
        Assert.Equal("TestValue", value);
    }

    private sealed class TestOperationRequest : OperationRequest
    {
        protected override HttpMethod Method => HttpMethod.Get;
        protected override Uri BuildUri(BuildRequestContext context) => new("https://example.com");
    }

    private sealed class DimOperationRequest : IOperationRequest
    {
    }
    private static readonly HttpRequestOptionsKey<string> s_testOptionKey = new("TestOption");
}
