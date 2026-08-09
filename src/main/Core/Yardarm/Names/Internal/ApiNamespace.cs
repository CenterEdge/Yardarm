using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Names.Internal
{
    // ReSharper disable InconsistentNaming
    internal class ApiNamespace : IApiNamespace
    {
        public NameSyntax Name { get; }
        public NameSyntax IApi { get; }

        public ApiNamespace(IRootNamespace rootNamespace)
        {
            ArgumentNullException.ThrowIfNull(rootNamespace);

            Name = QualifiedName(rootNamespace.Name, IdentifierName("Api"));

            IApi = QualifiedName(
                Name,
                IdentifierName("IApi"));
        }
    }
}
