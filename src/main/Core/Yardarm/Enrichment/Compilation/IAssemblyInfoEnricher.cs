using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Enrichment.Compilation
{
    public interface IAssemblyInfoEnricher : IEnricher<CompilationUnitSyntax>
    {
    }
}
