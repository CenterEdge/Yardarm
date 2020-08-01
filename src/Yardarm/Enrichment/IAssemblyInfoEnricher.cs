using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Enrichment
{
    public interface IAssemblyInfoEnricher : IEnricher<CompilationUnitSyntax>
    {
    }
}
