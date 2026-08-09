using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Names.Internal
{
    // ReSharper disable InconsistentNaming
    internal class AuthenticationNamespace : IAuthenticationNamespace
    {
        public NameSyntax Name { get; }

        public NameSyntax Authenticators { get; }

        public NameSyntax IAuthenticator { get; }

        public NameSyntax MultiAuthenticator { get; }

        public NameSyntax SecuritySchemeSetAttribute { get; }

        public AuthenticationNamespace(IRootNamespace rootNamespace)
        {
            ArgumentNullException.ThrowIfNull(rootNamespace);

            Name = QualifiedName(rootNamespace.Name, IdentifierName("Authentication"));

            Authenticators = QualifiedName(
                Name,
                IdentifierName("Authenticators"));

            IAuthenticator = QualifiedName(
                Name,
                IdentifierName("IAuthenticator"));

            MultiAuthenticator = QualifiedName(
                Name,
                IdentifierName("MultiAuthenticator"));

            SecuritySchemeSetAttribute = QualifiedName(
                Name,
                IdentifierName("SecuritySchemeSetAttribute"));
        }
    }
}
