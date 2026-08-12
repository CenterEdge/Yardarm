using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names.Internal
{
    internal class RootNamespace : IRootNamespace
    {
        public NameSyntax Name { get; }

        public RootNamespace(YardarmGenerationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            Name = SyntaxFactory.ParseName(settings.RootNamespace);
        }
    }
}
