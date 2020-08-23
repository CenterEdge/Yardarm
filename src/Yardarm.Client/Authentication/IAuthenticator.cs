using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RootNamespace.Authentication
{
    public interface IAuthenticator
    {
        ValueTask ApplyAsync(HttpRequestMessage message, CancellationToken cancellationToken = default);

        ValueTask ProcessResponseAsync(HttpResponseMessage message, CancellationToken cancellationToken = default);
    }
}
