using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Enrichment
{
    public interface IAssemblyInfoEnricher
    {
        CompilationUnitSyntax Enrich(CompilationUnitSyntax syntax);
    }
}
