# Consuming Yardarm SDKs via Dependency Injection

## Registering on Microsoft.Extensions.DependencyInjection

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
