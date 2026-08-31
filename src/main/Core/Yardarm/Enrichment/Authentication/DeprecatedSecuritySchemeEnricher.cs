using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Authentication
{
    /// <summary>
    /// Marks deprecated security schemes with <see cref="System.ObsoleteAttribute"/>.
    /// </summary>
    public class DeprecatedSecuritySchemeEnricher
        : IOpenApiSyntaxNodeEnricher<ClassDeclarationSyntax, IOpenApiSecurityScheme>
    {
        public ClassDeclarationSyntax Enrich(ClassDeclarationSyntax target,
            OpenApiEnrichmentContext<IOpenApiSecurityScheme> context) =>
            context.Element.Deprecated
                ? MarkObsolete(target, context.LocatedElement.Key)
                : target;

        private static ClassDeclarationSyntax MarkObsolete(ClassDeclarationSyntax target, string schemeName) =>
            target.AddAttributeLists(AttributeList(SingletonSeparatedList(
                Attribute(WellKnownTypes.System.ObsoleteAttribute.Name,
                    AttributeArgumentList(SingletonSeparatedList(AttributeArgument(
                        SyntaxHelpers.StringLiteral($"Security scheme {schemeName} has been marked deprecated.")))))))
                .WithTrailingTrivia(ElasticCarriageReturnLineFeed));
    }
}
