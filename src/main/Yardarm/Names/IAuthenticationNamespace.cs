using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names
{
    // ReSharper disable InconsistentNaming
    public interface IAuthenticationNamespace : IKnownNamespace
    {
        NameSyntax Authenticators { get; }
        NameSyntax IAuthenticator { get; }
        NameSyntax MultiAuthenticator { get; }
        NameSyntax SecuritySchemeSetAttribute { get; }
    }
}
