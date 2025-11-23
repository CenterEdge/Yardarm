using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Compilation;

/// <summary>
/// If configured in, sets the default HTTP version for all requests.
/// </summary>
public class DefaultHttpVersionEnricher(GenerationContext generationContext) : IResourceFileEnricher
{
    public CompilationUnitSyntax Enrich(CompilationUnitSyntax target, ResourceFileEnrichmentContext context)
    {
        // If we received a default HTTP version, and we're targeting modern .NET where it makes sense,
        // apply the version. For .NET Standard 2.0 the highest version is 1.1 and there shouldn't be any
        // need to ever set it to 1.0, so we ignore the setting.
        if (!string.IsNullOrEmpty(generationContext.Options.DefaultHttpVersion) &&
            generationContext.CurrentTargetFramework.Version.Major >= 5)
        {
            VariableDeclaratorSyntax? declarator = target
                .DescendantNodes(p => p is not (PropertyDeclarationSyntax or MethodDeclarationSyntax)) // Don't decend into methods, etc, when searching
                .OfType<VariableDeclaratorSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == "s_defaultHttpVersion");

            if (declarator is not null)
            {
                VariableDeclaratorSyntax newDeclarator = declarator.WithInitializer(
                    EqualsValueClause(MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        WellKnownTypes.System.Net.HttpVersion.Name,
                        IdentifierName($"Version{generationContext.Options.DefaultHttpVersion.Replace(".", "")}"))));

                target = target.ReplaceNode(declarator, newDeclarator);
            }
        }

        if (!string.IsNullOrEmpty(generationContext.Options.DefaultHttpVersionPolicy) &&
            generationContext.CurrentTargetFramework.Version.Major >= 5)
        {
            VariableDeclaratorSyntax? declarator = target
                .DescendantNodes(p => p is not (PropertyDeclarationSyntax or MethodDeclarationSyntax)) // Don't decend into methods, etc, when searching
                .OfType<VariableDeclaratorSyntax>()
                .FirstOrDefault(p => p.Identifier.Text == "s_defaultHttpVersionPolicy");

            if (declarator is not null)
            {
                VariableDeclaratorSyntax newDeclarator = declarator.WithInitializer(
                    EqualsValueClause(MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        WellKnownTypes.System.Net.Http.HttpVersionPolicy.Name,
                        IdentifierName(generationContext.Options.DefaultHttpVersionPolicy))));

                target = target.ReplaceNode(declarator, newDeclarator);
            }
        }

        return target;
    }

    public bool ShouldEnrich(string resourceName) => resourceName == "Yardarm.Client.Requests.OperationRequest.cs";
}
