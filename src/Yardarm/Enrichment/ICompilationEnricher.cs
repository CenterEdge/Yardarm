using Microsoft.CodeAnalysis.CSharp;

namespace Yardarm.Enrichment
{
    public interface ICompilationEnricher : IAsyncEnricher<CSharpCompilation>
    {
    }
}
