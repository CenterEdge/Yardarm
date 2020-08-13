using Microsoft.CodeAnalysis.CSharp;

namespace Yardarm.Enrichment
{
    public interface ICompilationEnricher : IEnricher<CSharpCompilation>
    {
    }
}
