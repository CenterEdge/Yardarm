using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RootNamespace.Authentication;
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

    [Fact]
    public async Task BuildRequestAsync_Default_ForwardsToBuildRequest()
    {
        // Arrange

        var request = new TestOperationRequest();
        var context = new BuildRequestContext(TypeSerializerRegistry.CreateDefaultRegistry());

        // Act

        var requestMessage = await request.BuildRequestAsync(context, TestContext.Current.CancellationToken);

        // Assert

        Assert.Same(request.BuiltRequest, requestMessage);
        Assert.Same(request.BuiltContent, requestMessage.Content);
    }

    [Fact]
    public async Task BuildRequestAsync_Canceled_ReturnsCanceledWithoutBuilding()
    {
        // Arrange

        var request = new TestOperationRequest();
        var context = new BuildRequestContext(TypeSerializerRegistry.CreateDefaultRegistry());
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.Cancel();

        // Act

        var result = request.BuildRequestAsync(context, cts.Token);

        // Assert

        Assert.True(result.IsCanceled);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () => await result);
        Assert.Null(request.BuiltRequest);
    }

    [Fact]
    public async Task BuildRequestWithAsyncContentAsync_UsesAsyncContent()
    {
        // Arrange

        var request = new AsyncContentOperationRequest();
        request.Options.Set(s_testOptionKey, "TestValue");
        var context = new BuildRequestContext(TypeSerializerRegistry.CreateDefaultRegistry());

        // Act

        var requestMessage = await request.BuildRequestAsync(context, TestContext.Current.CancellationToken);

        // Assert

        Assert.Same(request.AsyncContent, requestMessage.Content);
        Assert.Equal(new Uri("https://example.com"), requestMessage.RequestUri);
        Assert.True(requestMessage.Options.TryGetValue(s_testOptionKey, out string value));
        Assert.Equal("TestValue", value);
        Assert.Equal("value", Assert.Single(requestMessage.Headers.GetValues("X-Test")));
    }

    [Fact]
    public async Task BuildContentAsync_DefaultCanceled_ReturnsCanceledWithoutBuilding()
    {
        // Arrange

        var request = new DefaultContentAsyncOperationRequest();
        var context = new BuildRequestContext(TypeSerializerRegistry.CreateDefaultRegistry());
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cts.Cancel();

        // Act

        Func<Task> action = async () => await request.BuildRequestAsync(context, cts.Token);

        // Assert

        await Assert.ThrowsAnyAsync<OperationCanceledException>(action);
        Assert.Equal(0, request.BuildContentCalls);
    }

    [Fact]
    public async Task BuildContentAsync_Default_ForwardsToBuildContent()
    {
        // Arrange

        var request = new DefaultContentAsyncOperationRequest();
        var context = new BuildRequestContext(TypeSerializerRegistry.CreateDefaultRegistry());

        // Act

        var requestMessage = await request.BuildRequestAsync(context, TestContext.Current.CancellationToken);

        // Assert

        Assert.Equal(1, request.BuildContentCalls);
        Assert.Same(request.Content, requestMessage.Content);
    }

    private sealed class TestOperationRequest : OperationRequest
    {
        public HttpRequestMessage BuiltRequest { get; private set; }

        public HttpContent BuiltContent { get; } = new StringContent("sync");

        protected override HttpMethod Method => HttpMethod.Get;
        protected override Uri BuildUri(BuildRequestContext context) => new("https://example.com");

        protected override HttpContent BuildContent(BuildRequestContext context) => BuiltContent;

        public override HttpRequestMessage BuildRequest(BuildRequestContext context)
        {
            BuiltRequest = base.BuildRequest(context);
            return BuiltRequest;
        }
    }

    /// <summary>
    /// Builds its content asynchronously.
    /// </summary>
    private sealed class AsyncContentOperationRequest : OperationRequest
    {
        public HttpContent AsyncContent { get; } = new StringContent("async");

        protected override HttpMethod Method => HttpMethod.Post;
        protected override Uri BuildUri(BuildRequestContext context) => new("https://example.com");

        protected override void AddHeaders(BuildRequestContext context, HttpRequestMessage requestMessage) =>
            requestMessage.Headers.Add("X-Test", "value");

        protected override HttpContent BuildContent(BuildRequestContext context) =>
            throw new InvalidOperationException("Content must be built asynchronously.");

        protected override async ValueTask<HttpContent> BuildContentAsync(BuildRequestContext context,
            CancellationToken cancellationToken = default)
        {
            await Task.Yield();
            return AsyncContent;
        }

        public override ValueTask<HttpRequestMessage> BuildRequestAsync(BuildRequestContext context,
            CancellationToken cancellationToken = default) =>
            BuildRequestWithAsyncContentAsync(context, cancellationToken);
    }

    /// <summary>
    /// Builds the request with <c>BuildRequestWithAsyncContentAsync</c> but uses the default <c>BuildContentAsync</c>.
    /// </summary>
    private sealed class DefaultContentAsyncOperationRequest : OperationRequest
    {
        public int BuildContentCalls { get; private set; }

        public HttpContent Content { get; } = new StringContent("sync");

        protected override HttpMethod Method => HttpMethod.Post;
        protected override Uri BuildUri(BuildRequestContext context) => new("https://example.com");

        protected override HttpContent BuildContent(BuildRequestContext context)
        {
            BuildContentCalls++;
            return Content;
        }

        public override ValueTask<HttpRequestMessage> BuildRequestAsync(BuildRequestContext context,
            CancellationToken cancellationToken = default) =>
            BuildRequestWithAsyncContentAsync(context, cancellationToken);
    }

    private sealed class DimOperationRequest : IOperationRequest
    {
        public IAuthenticator Authenticator { get; set; }
        public bool EnableResponseStreaming { get; set; }
    }

    private static readonly HttpRequestOptionsKey<string> s_testOptionKey = new("TestOption");
}
