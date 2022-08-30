using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Extensions.Http;
using RootNamespace.Authentication;

namespace RootNamespace.Internal
{
    internal class ApiFactoryOptions
    {
        private static readonly TimeSpan s_defaultHandlerLifetime = TimeSpan.FromMinutes(2);

        public List<Action<HttpClient>> HttpClientActions { get; } = new();

        public List<Action<HttpMessageHandlerBuilder>> HttpMessageHandlerBuilderActions { get; } = new();

        public List<Action<Authenticators>> AuthenticatorActions { get; } = new();

        public TimeSpan HandlerLifetime { get; set; } = s_defaultHandlerLifetime;

        public Func<string, bool> ShouldRedactHeaderValue { get; set; } = static _ => false;
    }
}
