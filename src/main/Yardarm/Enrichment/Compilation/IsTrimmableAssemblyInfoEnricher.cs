using System;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Helpers;
using Yardarm.Packaging;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Compilation
{
    public class IsTrimmableAssemblyInfoEnricher : IAssemblyInfoEnricher
    {
        private readonly GenerationContext _context;

        public IsTrimmableAssemblyInfoEnricher(GenerationContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            _context = context;
        }

        public CompilationUnitSyntax Enrich(CompilationUnitSyntax target)
        {
            if (_context.CurrentTargetFramework.Framework != NuGetFrameworkConstants.NetCoreApp
                || _context.CurrentTargetFramework.Version.Major < 6)
            {
                // Only .NET 6 and later targets include the required trimming attributes
                return target;
            }

            if (_context.Settings.Extensions.Any(p => !p.IsOutputTrimmable(_context)))
            {
                // If any extension produces output that is not compatible with trimming
                // we should not apply the attribute.
                return target;
            }

            return target.AddAttributeLists(
                AttributeList(
                        AttributeTargetSpecifier(Token(SyntaxKind.AssemblyKeyword)),
                        SingletonSeparatedList(Attribute(
                            ParseName("System.Reflection.AssemblyMetadata"),
                            AttributeArgumentList(SeparatedList(new[]
                            {
                                AttributeArgument(SyntaxHelpers.StringLiteral("IsTrimmable")),
                                AttributeArgument(SyntaxHelpers.StringLiteral("True"))
                            })))))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
        }
    }
}
