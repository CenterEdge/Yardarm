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
        public NameSyntax UnknownResponse { get; }

        public ResponsesNamespace(IRootNamespace rootNamespace)
        {
            if (rootNamespace == null)
            {
                throw new ArgumentNullException(nameof(rootNamespace));
            }

            Name = QualifiedName(rootNamespace.Name, IdentifierName("Responses"));

            IOperationResponse = QualifiedName(
                Name,
                IdentifierName("IOperationResponse"));

            OperationResponse = QualifiedName(
                Name,
                IdentifierName("OperationResponse"));

            UnknownResponse = QualifiedName(
                Name,
                IdentifierName("UnknownResponse"));
        }
    }
}
