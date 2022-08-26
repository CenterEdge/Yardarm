using System;
using System.Net.Http;
using Microsoft.Extensions.Options;
using Yardarm.Client.Internal;

namespace RootNamespace.Internal
{
    internal class ApiFactory
    {
        private readonly IOptionsMonitor<ApiFactoryOptions> _optionsMonitor;

        public ApiFactory(IOptionsMonitor<ApiFactoryOptions> optionsMonitor)
        {
            ThrowHelper.ThrowIfNull(optionsMonitor, nameof(optionsMonitor));

            _optionsMonitor = optionsMonitor;
        }

        public void ApplyHttpClientActions(HttpClient httpClient)
        {
            ApiFactoryOptions options = _optionsMonitor.CurrentValue;

            foreach (Action<HttpClient> action in options.HttpClientActions)
            {
                action(httpClient);
            }
        }
    }
}
