# Handling API Responses

The OpenAPI 3 specification allows a lot of flexibility in the responses returned from a given operation,
varying by HTTP status code and Content-Type. The generated Yardarm SDK is designed to allow as much
flexibility as possible in handling these responses, but in a strongly-typed manner.

## Simple response handling

In the case where you expect a specific status code and are okay with exceptions in other cases,
use `.AsXXX()` methods on the response to cast the response. These methods throw a `StatusCodeMismatchException`
exception if the response isn't of the correct status code.

```cs
MyOperationJsonRequest request = new MyOperationJsonRequest();

// Returns a generic response
using var response = await api.MyOperationAsync(request);

// Throws StatusCodeMismatchException if the response is not a MyOperationOkResponse
MyOperationOkResponse okResponse = response.AsOk();

// GetBodyAsync is available for any response known to return content, strongly typed to the schema
var body = await okResponse.GetBodyAsync();
```

## Advanced response handling

For more advanced scenarios, such as reading the body from error responses, use type checking to
determine the type of response.

```cs
MyOperationJsonRequest request = new MyOperationJsonRequest();

// Returns a generic response
using var response = await api.MyOperationAsync(request);

switch (response)
{
    case MyOperationOkResponse okResponse:
        var okBody = await okResponse.GetBodyAsync();
        // do things
        break;

    case MyOperationNotFoundResponse notFoundResponse:
        var notFoundBody = await notFoundResponse.GetBodyAsync();
        // do things
        break;

    case MyOperationUnknownResponse unknownResponse:
        var body = await unknownResponse.GetBodyAsync<MyExpectedBody>();
        // do things
        break;
}
```

## Two response codes which return the same schema

Switch expressions are a convienent way to extract the body from two different response
codes which return the same schema.

```cs
MyOperationJsonRequest request = new MyOperationJsonRequest();

// Returns a generic response
using var response = await api.MyOperationAsync(request);

var body = response switch {
    MyOperationOkResponse okResponse => await okResponse.GetBodyAsync(),
    MyOperationCreatedResponse createdResponse => await createdResponse.GetBodyAsync(),
    _ => throw new Exception("...")
};
```

## Other response properties

```cs
MyOperationJsonRequest request = new MyOperationJsonRequest();

// Returns a generic response
using var response = await api.MyOperationAsync(request);

// Access the status code
if (response.StatusCode == HttpStatusCode.Created)
{
    // Note: in many cases, testing for the type MyOperationOkResponse is preferable.
    // However, if you don't need the body or the headers, this may be simpler.
}

// Determine if the status code was a success or failure code.
if (!response.IsSuccessStatusCode)
{
    throw new Exception("...");
}

// Access the raw HttpResponseMessage.
if (response.Message.Content.Headers.ContentType.MediaType == "application/json")
{
}
```

## Working with unknown status codes

Unfortunately, it is usually the case that the OpenAPI specification and reality are not the same.
Many times we may encounter response codes other than those officially declared in the specification.
For example, 503 and 504 errors from gateways and proxies can often occur but are rarely included in
the specification. However, poor spec writing may also be a culprit.

For every operation, there is an additional `XXXUnknownResponse` type used to represent any status codes
not defined in the specification. It includes a `GetBodyAsync<T>` method which may be used to deserialize
any type, typically based on manually inspecting the status code.

```cs
MyOperationJsonRequest request = new MyOperationJsonRequest();

// Returns a generic response
using var response = await api.MyOperationAsync(request);

if (response is MyOperationUnknownResponse unknownResponse)
{
    if (unknownResponse.StatusCode == HttpStatusCode.Forbidden)
    {
        var body = await unknownResponse.GetBodyAsync<ExpectedBody>();
        // Do something with the body here.
    }
    else
    {
        throw new Exception("...");
    }
}
```

## Mixed schemas for the same status code

Yardarm does not currently support varying the response body schema based on the Content-Type
for the same status code. Yardarm will select the schema it feels is most appropriate and use it
for the response type. Exceptions may occur if the server returns one of the other schemas.

Different schemas based on the status code are fully supported.

## Disposing

It is recommended  to use a `using` clause or some other means of calling `Dispose` on each response.
Calling `Dispose` on the response disposes of the `HttpResponseMessage`, which may perform important cleanup.
Failing to dispose may also have an impact on garbage collection performance.
