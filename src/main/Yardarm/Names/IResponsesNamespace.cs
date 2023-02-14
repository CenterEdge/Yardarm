using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Names
{
    // ReSharper disable InconsistentNaming
    public interface IResponsesNamespace : IKnownNamespace
    {
        NameSyntax IOperationResponse { get; }
        NameSyntax OperationResponse { get; }
        NameSyntax StatusCodeMismatchException { get; }
        NameSyntax UnknownResponse { get; }

        NameSyntax IOperationResponseTBody(TypeSyntax bodyType) =>
            QualifiedName(
                Name,
                GenericName(Identifier("IOperationResponse"),
                    TypeArgumentList(SingletonSeparatedList(bodyType))));
    }
}
