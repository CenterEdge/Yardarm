using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names
{
    // ReSharper disable InconsistentNaming
    public interface IResponsesNamespace : IKnownNamespace
    {
        NameSyntax IOperationResponse { get; }
        NameSyntax OperationResponse { get; }
        NameSyntax StatusCodeMismatchException { get; }
        NameSyntax UnknownResponse { get; }
    }
}
