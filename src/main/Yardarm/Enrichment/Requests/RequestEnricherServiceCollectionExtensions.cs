using Microsoft.Extensions.DependencyInjection;

namespace Yardarm.Enrichment.Requests;

public static class RequestEnricherServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDefaultRequestEnrichers() =>
            services
                .AddOpenApiSyntaxNodeEnricher<RequestClassMethodDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequestInterfaceMethodDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequestMultipartEncodingEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequestParameterDocumentationEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequiredBodyRequestEnricher>()
                .AddOpenApiSyntaxNodeEnricher<RequiredParameterEnricher>();
    }
}
