using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Internal
{
    public class VersionAssemblyInfoEnricher : IAssemblyInfoEnricher
    {
        private readonly YardarmGenerationSettings _settings;

        public int Priority => 0;

        public VersionAssemblyInfoEnricher(YardarmGenerationSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        public CompilationUnitSyntax Enrich(CompilationUnitSyntax target) =>
            target.AddAttributeLists(
                SyntaxFactory.AttributeList().AddAttributes(
                    SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Reflection.AssemblyVersion"))
                        .AddArgumentListArguments(SyntaxFactory.AttributeArgument(
                            SyntaxHelpers.StringLiteral(_settings.Version.ToString()))))
                    .WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword))),
                SyntaxFactory.AttributeList().AddAttributes(
                        SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Reflection.AssemblyFileVersion"))
                            .AddArgumentListArguments(SyntaxFactory.AttributeArgument(
                                SyntaxHelpers.StringLiteral(_settings.Version.ToString()))))
                    .WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword))),
                SyntaxFactory.AttributeList().AddAttributes(
                        SyntaxFactory.Attribute(SyntaxFactory.ParseName("System.Reflection.AssemblyInformationalVersion"))
                            .AddArgumentListArguments(SyntaxFactory.AttributeArgument(
                                SyntaxHelpers.StringLiteral(_settings.Version.ToString() + (_settings.VersionSuffix ?? "")))))
                    .WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword))));
    }
}
