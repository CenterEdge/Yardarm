using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Compilation
{
    /// <summary>
    /// Adds an <see cref="AssemblyMetadataAttribute"/> with the key <c>YardarmVersion</c> that records
    /// the version of Yardarm used to generate the SDK.
    /// </summary>
    public class YardarmVersionAssemblyInfoEnricher : IAssemblyInfoEnricher
    {
        internal const string MetadataKey = "YardarmVersion";

        internal static string YardarmVersion { get; } = GetYardarmVersion();

        public CompilationUnitSyntax Enrich(CompilationUnitSyntax target) =>
            target.AddAttributeLists(
                AttributeList(
                        AttributeTargetSpecifier(Token(SyntaxKind.AssemblyKeyword)),
                        SingletonSeparatedList(Attribute(
                            ParseName("System.Reflection.AssemblyMetadata"),
                            AttributeArgumentList(SeparatedList(new[]
                            {
                                AttributeArgument(SyntaxHelpers.StringLiteral(MetadataKey)),
                                AttributeArgument(SyntaxHelpers.StringLiteral(YardarmVersion))
                            })))))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed));

        private static string GetYardarmVersion()
        {
            Assembly assembly = typeof(YardarmVersionAssemblyInfoEnricher).Assembly;

            return assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion
                ?? assembly.GetName().Version?.ToString()
                ?? "0.0.0";
        }
    }
}
