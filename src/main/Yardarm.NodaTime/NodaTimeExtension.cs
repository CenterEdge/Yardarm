using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.NodaTime.Internal;
using Yardarm.Packaging;

namespace Yardarm.NodaTime;

public sealed class NodaTimeExtension : YardarmExtension
{
    public override string Name => "NodaTime";

    public override bool IsOutputTrimmable(GenerationContext context) => true;

    public override IServiceCollection ConfigureServices(IServiceCollection services)
    {
        services
            .AddTypeGeneratorFactory<OpenApiSchema, NodaTimeSchemaGeneratorFactory>()
            .AddRegistrationEnricher<JsonSerializerSettingsEnricher>("JsonSerializerSettings")
            .AddKeyedTransient<IEnricher<AttributeSyntax>, JsonSourceGenerationOptionsEnricher>("JsonSourceGenerationOptions")
            .AddSingleton<ISyntaxTreeGenerator, ClientGenerator>()
            .AddSingleton<IDependencyGenerator, NodaTimeDependencyGenerator>()
            .AddDefaultLiteralConverterEnricher<DefaultLiteralConverterEnricher>();

        return services;
    }
}
