# Mocking Yardarm SDKs for Unit Tests

## Moq

Given a service such as:

```cs
```cs
public class MyService
{
    private readonly IMyApi api;

    public MyService(IMyApi api)
    {
        _api = api;
    }

    public async Task DoSomethingAsync(CancellationToken cancellationToken = default)
    {
        var request = new MyOperationJsonRequest
        {
            Body = new MyRequestBody();
        };

        var response = await _api.MyOperationAsync(request, cancellationToken);

        // Process response here...
    }
}
```

The following basic example uses Moq to create a mock API to pass to a service under test.

```cs
[Fact]
public async Task MyTest()
{
    // Headers are optional, leave this off for responses that don't use special headers
    var headers = new HttpResponseHeaders();
    headers.Add("X-My-Header", "value");

    // Leave this off for responses that don't have a body
    var body = new MyResponseBody();

    var api = new Mock<IMyApi>();
    api
        .Setup(m => m.MyOperationAsync(It.IsAny<MyOperationJsonRequest>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new MyOperationOkResponse(body, headers));

    var myService = new MyService(api.Object);

    await myService.DoSomethingAsync();

    // More assertions here
}
```
