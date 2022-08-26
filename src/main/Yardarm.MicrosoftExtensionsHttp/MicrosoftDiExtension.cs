using Microsoft.Extensions.DependencyInjection;
using Yardarm.Enrichment.Compilation;
using Yardarm.Generation;
using Yardarm.MicrosoftExtensionsHttp.Internal;
using Yardarm.Packaging;

namespace Yardarm.MicrosoftExtensionsHttp
{
    public class MicrosoftDiExtension : YardarmExtension
    {
        public override IServiceCollection ConfigureServices(IServiceCollection services)
        {
            services
                .AddSingleton<IDependencyGenerator, DependencyInjectionDependencyGenerator>()
                .AddSingleton<ISyntaxTreeGenerator, ClientGenerator>()
                .AddResourceFileEnricher<ServiceCollectionExtensionsEnricher>()
                .AddResourceFileEnricher<ApiBuilderExtensionsEnricher>();

            return services;
        }
    }
}
