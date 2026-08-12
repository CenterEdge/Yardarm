using System;

namespace RootNamespace.Responses;

/// <summary>
/// The response was not of the expected status code.
/// </summary>
public class StatusCodeMismatchException(IOperationResponse response, Type expectedType) : Exception
{
    /// <summary>
    /// The response.
    /// </summary>
    public IOperationResponse Response { get; } = response;

    /// <summary>
    /// The type of response which was expected.
    /// </summary>
    public Type ExpectedType { get; } = expectedType;
}
