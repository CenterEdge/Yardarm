# Consuming Yardarm SDKs via Dependency Injection

## Using Microsoft.Extensions.Http

This example assumes that the `Yardarm.MicrosoftExtensionsHttp` extension was used when
the SDK was generated. For further details see
[Make HTTP requests using IHttpClientFactory in ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests).

In it's simple form this is sufficient:

```cs
public void ConfigureServices(IServiceCollection services)
{
    // Other services are registered here

    services.AddXXXApis() // AddXXXApis name will vary based on the SDK name, and returns an IApiBuilder
        .AddAllApis();
}
```

However, there is support for a high degree of customization, such as:

- Configuring default authentication
- Setting the BaseAddress, default timeouts, etc
- Redacting sensitive headers from logs
- Configuring the primary HttpMessageHandler
- Add DelegatingHandler instances to the handler stack
- Setting all of the above differently for specific APIs within the SDK

```cs
public void ConfigureServices(IServiceCollection services)
{
    // Other services are registered here

    services.AddXXXApis() // AddXXXApis name will vary based on the SDK name, and returns an IApiBuilder
        .ConfigureAuthenticators(authenticators => {
            // Apply any global authentication rules here.
            // There is also an overload which provides an IServiceProvider parameter.
        })
        .ConfigureHttpClient(client => {
            // Apply global configuration to the HttpClient, such as the BaseAddress or timeouts
            // There is also an overload which provides an IServiceProvider parameter.

            // For simply setting the BaseAddress, there is also a ConfigureBaseAddress method.
        })
        .ConfigurePrimaryHttpMessageHandler(() =>
        {
            // Configure or substitute the primary HttpMessageHandler.
            // There are also overloads which provides an IServiceProvider or accepts the type
            // of the handler as a generic type parameter.
            return new HttpClientHandler();
        })
        .AddHttpMessageHandler(() =>
        {
            // Add a DelegatingHandler to the stack which is applied to all APIs within the SDK.
            // There are also overloads which provides an IServiceProvider or accepts the type
            // of the handler as a generic type parameter.
            return new MyDelegatingHandler();
        })
        .RedactLoggedHeaders(new[] {"Authorization"}) // Redact one or more headers from logging
        .AddApi<IApiType, ApiType>(builder => {
            // This adds a specific API from the SDK (IApiType) along with its concrete implementation (ApiType).
            // The builder is an IHttpClientBuilder which is used to further customize behaviors for
            // this specific API.

            // Any builder.ConfigureHttpClient calls are executed after the global configurations.
            // Any builder.ConfigurePrimaryHttpMessageHandler call will completely replace the global configuration.
            // Any builder.AddHttpMessageHandler calls will add further message handlers, in addition to the global configuration.
            // Any builder.RedactLoggedHeaders call will completely replace the global configuration.
        })
        .AddAllOtherApis((type, builder) =>
        {
            // Adds all other APIs which have not been manually registered above.

            // The type parameter is the IApiType of the API being added.
            // The builder parameter allows further customization of the HttpClient in the same way as AddApi above.
        });

        // .AddAllApis((type, builder) => { }) may also be used to add all APIs from the SDK.
        // The AddAllApis method will throw an InvalidOperationException if any APIs are manually registered.
}
```

## Consuming the API in a Service

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
        // Call methods on _api here, all handling of HttpClient is automatic.
        // Default authenticators are received from DI, but may overridden per-request.
    }
}
```

## Registering Manually on Microsoft.Extensions.DependencyInjection

This example is for manual registration when not using `Yardarm.MicrosoftExtensionsHttp`.

```cs
public static IServiceCollection AddApiServices(this IServiceCollection services)
{
    services.TryAddSingleton(TypeSerializerRegistry.Instance);

    // For a simple, static set of authenticators.
    services.TryAddSingleton(new Authenticators());

    // Or, for authenticators configured from appsettings.json in ASP.NET Core.
    // Note: Only choose one or the other, not both.
    services.TryAddSingleton(serviceProvider =>
    {
        var options = serviceProvider.GetRequiredService<IOptions<MyOptions>>().Value;

        return new Authenticators
        {
            // Configure from options here.
        }
    });

    // This assumes you are using the Microsoft.Extensions.Http NuGet package.
    // Repeat this step for each API interface in the SDK you intend to consume.
    if (!services.Any(s => s.ServiceType == typeof(IMyApi)))
    {
        var builder = services.AddHttpClient<IMyApi, MyApi>((serviceProvider, httpClient) => {
            var options = serviceProvider.GetRequiredService<IOptions<MyOptions>>().Value;

            // This could also be a constant instead of using IOptions to acquire from configuration.
            // Use IOptionsSnapshot<T> if you desire options that dynamically update at runtime.
            httpClient.BaseAddress = options.MyBaseUri;
        });

        // Make additional calls to register middleware on the HttpClient, if desired.
        // This can also be used to add Polly policies, such as retries and circuit breakers,
        // via the Microsoft.Extensions.Http.Polly NuGet package.
        builder.AddHttpMessageHandler<MyHttpClientHandler>();
    }
}
```
