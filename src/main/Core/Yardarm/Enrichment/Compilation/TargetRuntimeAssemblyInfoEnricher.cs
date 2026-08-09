using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Frameworks;
using Yardarm.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Compilation
{
    public class TargetRuntimeAssemblyInfoEnricher : IAssemblyInfoEnricher
    {
        private readonly GenerationContext _generationContext;

        public TargetRuntimeAssemblyInfoEnricher(GenerationContext generationContext)
        {
            ArgumentNullException.ThrowIfNull(generationContext);
            _generationContext = generationContext;
        }

        public CompilationUnitSyntax Enrich(CompilationUnitSyntax syntax) => syntax
            .AddAttributeLists(AttributeList(
                    AttributeTargetSpecifier(Token(SyntaxKind.AssemblyKeyword)),
                    SingletonSeparatedList(Attribute(
                        ParseName("System.Runtime.Versioning.TargetFramework"),
                        AttributeArgumentList(SeparatedList(new[]
                        {
                            AttributeArgument(SyntaxHelpers.StringLiteral(_generationContext.CurrentTargetFramework.DotNetFrameworkName)),
                            AttributeArgument(
                                    NameEquals("FrameworkDisplayName"),
                                    null,
                                    SyntaxHelpers.StringLiteral(GetDisplayName(_generationContext.CurrentTargetFramework)))
                        })))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));

        // This appears to align with example builds which don't include a display name for .NET Standard or .NET Core before .NET 5
        private static string GetDisplayName(NuGetFramework framework) => framework switch
        {
            {Framework: ".NETCoreApp", Version.Major: >= 5} =>
                $".NET {framework.Version.Major}.{framework.Version.Minor}",
            {Framework: ".NETFramework", Version.Revision: > 0} =>
                $".NET Framework {framework.Version.Major}.{framework.Version.Minor}.{framework.Version.Revision}",
            {Framework: ".NETFramework"} =>
                $".NET Framework {framework.Version.Major}.{framework.Version.Minor}",
            _ => ""
        };
    }
}
