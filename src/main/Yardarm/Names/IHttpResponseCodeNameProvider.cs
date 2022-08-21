using System.Net;

namespace Yardarm.Names
{
    public interface IHttpResponseCodeNameProvider
    {
        string GetName(HttpStatusCode responseCode);
    }
}
