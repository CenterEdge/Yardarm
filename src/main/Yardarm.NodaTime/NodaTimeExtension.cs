using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.NodaTime.Internal;
using Yardarm.Packaging;

namespace Yardarm.NodaTime;

public class NodaTimeExtension : YardarmExtension
{
    public override bool IsOutputTrimmable(GenerationContext context) => true;

    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services
            .AddOpenApiSyntaxNodeEnricher<NodaTimePropertyEnricher>()
            .AddRegistrationEnricher<JsonSerializerSettingsEnricher>("JsonSerializerSettings")
            .AddKeyedTransient<IEnricher<AttributeSyntax>, JsonSourceGenerationOptionsEnricher>("JsonSourceGenerationOptions")
            .AddSingleton<ISyntaxTreeGenerator, ClientGenerator>()
            .AddSingleton<IDependencyGenerator, NodaTimeDependencyGenerator>()
            .AddDefaultLiteralConverterEnricher<DefaultLiteralConverterEnricher>();

        return services;
    }
}
