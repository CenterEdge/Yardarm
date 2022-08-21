using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Enrichment
{
    public interface ICreateDefaultRegistryEnricher : IEnricher<ExpressionSyntax>
    {
    }
}
