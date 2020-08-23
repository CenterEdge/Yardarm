using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Names.Internal
{
    // ReSharper disable InconsistentNaming
    internal class AuthenticationNamespace : IAuthenticationNamespace
    {
        public NameSyntax Name { get; }

        public NameSyntax IAuthenticator { get; }

        public AuthenticationNamespace(IRootNamespace rootNamespace)
        {
            if (rootNamespace == null)
            {
                throw new ArgumentNullException(nameof(rootNamespace));
            }

            Name = QualifiedName(rootNamespace.Name, IdentifierName("Authentication"));

            IAuthenticator = QualifiedName(
                Name,
                IdentifierName("IAuthenticator"));
        }
    }
}
