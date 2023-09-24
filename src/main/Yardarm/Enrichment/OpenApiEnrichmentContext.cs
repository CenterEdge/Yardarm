﻿using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Enrichment
{
    public class OpenApiEnrichmentContext<TElement>
        where TElement : IOpenApiElement
    {
        public CSharpCompilation Compilation { get; }

        public SyntaxTree SyntaxTree { get; }

        /// <summary>
        /// The original node before any enrichments in the current set. This must be used to access the
        /// semantic model, as any mutations to the target won't belong on the syntax tree until all
        /// enrichments to that syntax tree are completed.
        /// </summary>
        public SyntaxNode OriginalNode { get; }

        public ILocatedOpenApiElement<TElement> LocatedElement { get; }

        public TElement Element => LocatedElement.Element;


        public OpenApiEnrichmentContext(CSharpCompilation compilation, SyntaxTree syntaxTree,
            ILocatedOpenApiElement<TElement> locatedElement, SyntaxNode originalNode)
        {
            ArgumentNullException.ThrowIfNull(compilation);
            ArgumentNullException.ThrowIfNull(syntaxTree);
            ArgumentNullException.ThrowIfNull(locatedElement);
            ArgumentNullException.ThrowIfNull(originalNode);

            Compilation = compilation;
            SyntaxTree = syntaxTree;
            LocatedElement = locatedElement;
            OriginalNode = originalNode;
        }
    }
}
