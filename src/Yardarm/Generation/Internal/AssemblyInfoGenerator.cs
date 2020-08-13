using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Yardarm.Enrichment;

namespace Yardarm.Generation.Internal
{
    internal class AssemblyInfoGenerator : ISyntaxTreeGenerator
    {
        private readonly IEnumerable<IAssemblyInfoEnricher> _enrichers;

        public AssemblyInfoGenerator(IEnumerable<IAssemblyInfoEnricher> enrichers)
        {
            _enrichers = enrichers ?? throw new ArgumentNullException(nameof(enrichers));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            yield return CSharpSyntaxTree.Create(
                SyntaxFactory.CompilationUnit()
                    .Enrich(_enrichers)
                    .NormalizeWhitespace());
        }
    }
}
