using RootNamespace.Authentication;

namespace RootNamespace.Requests
{
    /// <summary>
    /// Base class for operation requests.
    /// </summary>
    public abstract class OperationRequest : IOperationRequest
    {
        /// <inheritdoc />
        public IAuthenticator? Authenticator { get; set; }

        /// <inheritdoc />
        public bool EnableResponseStreaming { get; set; }
    }
}
