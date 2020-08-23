using RootNamespace.Authentication;

namespace RootNamespace.Requests
{
    public interface IOperationRequest
    {
        /// <summary>
        /// Optionally override the default authenticator for this request.
        /// </summary>
        public IAuthenticator? Authenticator { get; set; }
    }
}
