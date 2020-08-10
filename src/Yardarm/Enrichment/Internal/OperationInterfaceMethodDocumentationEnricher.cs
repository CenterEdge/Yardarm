using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Helpers;

namespace Yardarm.Enrichment.Internal
{
    internal class OperationInterfaceMethodDocumentationEnricher : IOperationInterfaceMethodEnricher
    {
        public int Priority => 100; // Run after most other enrichers

        public MethodDeclarationSyntax Enrich(MethodDeclarationSyntax target,
            LocatedOpenApiElement<OpenApiOperation> context) =>
            string.IsNullOrWhiteSpace(context.Element.Summary)
                ? target
                : AddDocumentation(target, context.Element);

        private MethodDeclarationSyntax AddDocumentation(MethodDeclarationSyntax target,
            OpenApiOperation context) =>
            target.WithLeadingTrivia(
                target.GetLeadingTrivia().Insert(0,
                    DocumentationSyntaxHelpers.BuildXmlCommentTrivia(GetSections(context).ToArray<XmlNodeSyntax>())));

        private IEnumerable<XmlElementSyntax> GetSections(OpenApiOperation context)
        {
            yield return DocumentationSyntaxHelpers.BuildSummaryElement(context.Summary);

            if (!string.IsNullOrWhiteSpace(context.Description))
            {
                yield return DocumentationSyntaxHelpers.BuildRemarksElement(context.Description);
            }
        }

    }
}
