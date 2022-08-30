using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Http;

namespace RootNamespace.Internal
{
    internal class ApiFactoryOptions
    {
        private static readonly TimeSpan s_defaultHandlerLifetime = TimeSpan.FromMinutes(2);

        public List<Action<HttpClient>> HttpClientActions { get; } = new();

        public List<Action<HttpMessageHandlerBuilder>> HttpMessageHandlerBuilderActions { get; } = new();

        public TimeSpan HandlerLifetime { get; set; } = s_defaultHandlerLifetime;

        public Func<string, bool> ShouldRedactHeaderValue { get; set; } = static _ => false;
    }
}
