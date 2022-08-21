using System;

namespace RootNamespace.Responses
{
    /// <summary>
    /// The response was not of the expected status code.
    /// </summary>
    public class StatusCodeMismatchException : Exception
    {
        /// <summary>
        /// The response.
        /// </summary>
        public IOperationResponse Response { get; }

        /// <summary>
        /// The type of response which was expected.
        /// </summary>
        public Type ExpectedType { get; }

        public StatusCodeMismatchException(IOperationResponse response, Type expectedType)
        {
            Response = response;
            ExpectedType = expectedType;
        }
    }
}
