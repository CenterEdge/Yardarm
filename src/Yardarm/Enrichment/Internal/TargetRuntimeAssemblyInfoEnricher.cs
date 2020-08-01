using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Internal
{
    internal class TargetRuntimeAssemblyInfoEnricher : IAssemblyInfoEnricher
    {
        public CompilationUnitSyntax Enrich(CompilationUnitSyntax syntax) => syntax
            .AddAttributeLists(
                SyntaxFactory.AttributeList().AddAttributes(
                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Runtime.Versioning.TargetFramework"))
                        .AddArgumentListArguments(
                            SyntaxFactory.AttributeArgument(SyntaxHelpers.StringLiteral(".NETStandard,Version=v2.0")),
                            SyntaxFactory.AttributeArgument(SyntaxHelpers.StringLiteral(""))
                                .WithNameEquals(SyntaxFactory.NameEquals("FrameworkDisplayName"))))
                    .WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword))));
    }
}
