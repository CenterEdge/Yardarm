using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names
{
    // ReSharper disable InconsistentNaming
    public interface IApiNamespace : IKnownNamespace
    {
        NameSyntax IApi { get; }
    }
}
