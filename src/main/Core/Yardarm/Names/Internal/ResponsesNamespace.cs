using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Names.Internal
{
    // ReSharper disable InconsistentNaming
    internal class ResponsesNamespace : IResponsesNamespace
    {
        public NameSyntax Name { get; }
        public NameSyntax IOperationResponse { get; }
        public NameSyntax OperationResponse { get; }
        public NameSyntax StatusCodeMismatchException { get; }
        public NameSyntax UnknownResponse { get; }

        public ResponsesNamespace(IRootNamespace rootNamespace)
        {
            ArgumentNullException.ThrowIfNull(rootNamespace);

            Name = QualifiedName(rootNamespace.Name, IdentifierName("Responses"));

            IOperationResponse = QualifiedName(
                Name,
                IdentifierName("IOperationResponse"));

            OperationResponse = QualifiedName(
                Name,
                IdentifierName("OperationResponse"));

            StatusCodeMismatchException = QualifiedName(
                Name,
                IdentifierName("StatusCodeMismatchException"));

            UnknownResponse = QualifiedName(
                Name,
                IdentifierName("UnknownResponse"));
        }
    }
}
