using System;
using System.Collections.Generic;
using System.Net.Http;

namespace RootNamespace.Internal
{
    internal class ApiFactoryOptions
    {
        public List<Action<HttpClient>> HttpClientActions { get; } = new();
    }
}
