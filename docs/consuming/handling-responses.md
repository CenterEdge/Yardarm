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

## JSON Lines and streaming

Media types that use `itemSchema`, such as `application/jsonl` and `application/x-ndjson`, are supported
with the System.Text.Json extension. Each line is one item. The body is an `IAsyncEnumerable<T>` of the
item type, which reads items as they arrive rather than waiting for the whole response.

```cs
using var response = await api.StreamThingsAsync(new StreamThingsRequest());

await foreach (Thing thing in response.AsOk().GetBodyAsync())
{
    // Handle each thing as it is received
}
```

Calling `GetBodyAsync` returns immediately without waiting for any data, and the response content is not read
until the first item is requested. The body can only be enumerated once. Keep the response undisposed until
enumeration is finished. Pass a `CancellationToken` to `GetBodyAsync` or to `WithCancellation` to stop reading.
If either token is canceled, enumeration stops.

Requests for operations with a streamed response default `EnableResponseStreaming` to `true`, so the response
headers are returned as soon as they are received. Setting it to `false` still produces the same
`IAsyncEnumerable<T>`, but the full response is buffered before the first item is returned.

### Streaming JSON arrays

A response with a plain JSON array schema is a `List<T>` by default. To stream it as an
`IAsyncEnumerable<T>`, add the `x-yardarm-streaming: true` extension to the media type, beside the `schema`:

```yaml
paths:
  /things:
    get:
      operationId: listThings
      responses:
        '200':
          description: OK
          content:
            application/json:
              x-yardarm-streaming: true
              schema:
                type: array
                items:
                  $ref: '#/components/schemas/Thing'
```

The array is read incrementally, so the response should be an array at the root of the body. The extension
applies to the media type it is on, including media types of responses in `components/responses`, and it is ignored for
media types that are not arrays.

### Other details

- When a response offers both JSON and JSON Lines, Yardarm uses JSON, which is a `List<T>` unless
  `x-yardarm-streaming` is set on the media type.
- JSON Lines request bodies use a separate `{Operation}JsonLinesRequest` class, with a `List<T>` `Body` property.
- For OpenAPI 3.0 and 3.1, use the `x-oai-itemSchema` extension in place of `itemSchema`.
- A custom `ITypeSerializer` can support streaming by handling `IAsyncEnumerable<TElement>` as the `TSequence` type in
  `DeserializeSequenceAsync`, and by setting `SupportsStreaming` on its `SerializerDescriptor`.

The Newtonsoft.Json extension does not support JSON Lines or streaming. JSON Lines media types are not selected, so
responses that only offer JSON Lines have no typed body, and JSON Lines requests use an
`{Operation}HttpContentRequest` class with an `HttpContent` `Body` property. Array responses remain a
`List<T>`, and `x-yardarm-streaming` is ignored.

## Disposing

It is recommended  to use a `using` clause or some other means of calling `Dispose` on each response.
Calling `Dispose` on the response disposes of the `HttpResponseMessage`, which may perform important cleanup.
Failing to dispose may also have an impact on garbage collection performance.
