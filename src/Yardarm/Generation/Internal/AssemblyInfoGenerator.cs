using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Compilation;

namespace Yardarm.Generation.Internal
{
    internal class AssemblyInfoGenerator : ISyntaxTreeGenerator
    {
        private readonly GenerationContext _context;
        private readonly IEnumerable<IAssemblyInfoEnricher> _enrichers;

        public AssemblyInfoGenerator(GenerationContext context,
            IEnumerable<IAssemblyInfoEnricher> enrichers)
        {
            _context = context;
            _enrichers = enrichers ?? throw new ArgumentNullException(nameof(enrichers));
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            yield return CSharpSyntaxTree.Create(
                SyntaxFactory.CompilationUnit()
                    .Enrich(_enrichers)
                    .NormalizeWhitespace(),
                path: Path.Combine(_context.Settings.BasePath, "Properties", "AssemblyInfo.cs"),
                encoding: Encoding.UTF8);
        }
    }
}
