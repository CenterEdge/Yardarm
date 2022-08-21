using Microsoft.CodeAnalysis.CSharp;

namespace Yardarm.Enrichment.Compilation
{
    public interface ICompilationEnricher : IAsyncEnricher<CSharpCompilation>
    {
    }
}
