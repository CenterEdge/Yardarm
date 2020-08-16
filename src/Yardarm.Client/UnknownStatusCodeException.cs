using System;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace RootNamespace
{
    /// <summary>
    /// A response returned an unknown status code.
    /// </summary>
    public class UnknownStatusCodeException : Exception
    {
        public HttpResponseMessage ResponseMessage { get; }

        public UnknownStatusCodeException(HttpResponseMessage responseMessage)
            : base($"Unknown response status {responseMessage.StatusCode}.")
        {
            ResponseMessage = responseMessage ?? throw new ArgumentNullException(nameof(responseMessage));
        }
    }
}
