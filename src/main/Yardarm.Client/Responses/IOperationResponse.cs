using System;
using System.Net;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Responses;

public interface IOperationResponse : IDisposable
{
    HttpResponseMessage Message { get; }

    HttpStatusCode StatusCode { get; }

    bool IsSuccessStatusCode { get; }
}
