using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Enrichment.Internal
{
    internal class TargetRuntimeAssemblyInfoEnricher : IAssemblyInfoEnricher
    {
        public CompilationUnitSyntax Enrich(CompilationUnitSyntax syntax) => syntax
            .AddAttributeLists(
                SyntaxFactory.AttributeList().AddAttributes(
                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Runtime.Versioning.TargetFramework"))
                        .AddArgumentListArguments(
                            SyntaxFactory.AttributeArgument(
                                SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(".NETStandard,Version=v2.0"))))));
    }
}
