using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names;

// ReSharper disable InconsistentNaming
public interface IRequestsNamespace : IKnownNamespace
{
    NameSyntax IOperationRequest { get; }
    NameSyntax OperationRequest { get; }
    NameSyntax BuildRequestContext { get; }
}
