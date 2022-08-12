using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Names;

namespace Yardarm.Enrichment.Schema
{
    /// <summary>
    /// Enriches schema class properties with the MultipartPropertyAttribute to support multipart encoding. This is currently
    /// applied to all schemas because there isn't an easy way to recognize schemas which are used for multipart encoding.
    /// </summary>
    public class MultipartPropertyEnricher : IOpenApiSyntaxNodeEnricher<PropertyDeclarationSyntax, OpenApiSchema>
    {
        private readonly ISerializationNamespace _serializationNamespace;

        public MultipartPropertyEnricher(ISerializationNamespace serializationNamespace)
        {
            _serializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
        }

        public PropertyDeclarationSyntax Enrich(PropertyDeclarationSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            if (target.Parent is ClassDeclarationSyntax classDeclaration &&
                classDeclaration.GetGeneratorAnnotation() == typeof(RequestMediaTypeGenerator))
            {
                // Don't enrich body properties on the request classes
                return target;
            }

            return target.AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                SyntaxFactory.Attribute(_serializationNamespace.MultipartPropertyAttribute)
                    .AddArgumentListArguments(
                        SyntaxFactory.AttributeArgument(SyntaxHelpers.StringLiteral(context.LocatedElement.Key))))
                .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));
        }
    }
}
