using System.Net;

namespace Yardarm.Names
{
    public class HttpResponseCodeNameProvider : IHttpResponseCodeNameProvider
    {
        public string GetName(HttpStatusCode responseCode) =>
            responseCode switch
            {
                // Optimize common forms
                HttpStatusCode.OK => "Ok",
                HttpStatusCode.Created => "Created",
                HttpStatusCode.NoContent => "NoContent",
                HttpStatusCode.BadRequest => "BadRequest",
                HttpStatusCode.NotFound => "NotFound",
                HttpStatusCode.Conflict => "Conflict",
                HttpStatusCode.InternalServerError => "Error",
                _ => responseCode.ToString()
            };
    }
}
