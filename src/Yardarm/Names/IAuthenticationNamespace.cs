using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names
{
    // ReSharper disable InconsistentNaming
    public interface IAuthenticationNamespace : IKnownNamespace
    {
        NameSyntax IAuthenticator { get; }
        NameSyntax MultiAuthenticator { get; }
    }
}
