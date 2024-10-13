using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment;
using Yardarm.Packaging;

namespace Yardarm.NodaTime;

public class NodaTimeExtension : YardarmExtension
{
    public override bool IsOutputTrimmable(GenerationContext context) => true;

    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services
            .AddOpenApiSyntaxNodeEnricher<NodaTimePropertyEnricher>()
            .AddSingleton<IDependencyGenerator, NodaTimeDependencyGenerator>();

        return services;
    }
}
