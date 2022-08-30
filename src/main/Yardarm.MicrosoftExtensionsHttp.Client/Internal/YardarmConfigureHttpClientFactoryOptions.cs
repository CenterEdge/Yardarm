using Microsoft.Extensions.Http;
using Microsoft.Extensions.Options;
using Yardarm.Client.Internal;

namespace RootNamespace.Internal
{
    internal class YardarmConfigureHttpClientFactoryOptions : IConfigureNamedOptions<HttpClientFactoryOptions>
    {
        private readonly IOptions<ApiFactoryOptions> _apiFactoryOptions;

        public string Name { get; }

        public YardarmConfigureHttpClientFactoryOptions(string name, IOptions<ApiFactoryOptions> apiFactoryOptions)
        {
            ThrowHelper.ThrowIfNull(name, nameof(name));
            ThrowHelper.ThrowIfNull(apiFactoryOptions, nameof(apiFactoryOptions));

            Name = name;
            _apiFactoryOptions = apiFactoryOptions;
        }

        public void Configure(HttpClientFactoryOptions options) => Configure(Options.DefaultName, options);

        public void Configure(string name, HttpClientFactoryOptions options)
        {
            ThrowHelper.ThrowIfNull(options, nameof(options));

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (name == null || name == Name)
            {
                ApiFactoryOptions apiFactoryOptions = _apiFactoryOptions.Value;

                options.HandlerLifetime = apiFactoryOptions.HandlerLifetime;
                options.ShouldRedactHeaderValue = apiFactoryOptions.ShouldRedactHeaderValue;

                foreach (var action in apiFactoryOptions.HttpClientActions)
                {
                    options.HttpClientActions.Add(action);
                }

                foreach (var action in apiFactoryOptions.HttpMessageHandlerBuilderActions)
                {
                    options.HttpMessageHandlerBuilderActions.Add(action);
                }
            }
        }
    }
}
