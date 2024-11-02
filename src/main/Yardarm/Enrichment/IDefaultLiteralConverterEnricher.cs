using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Enrichment;

/// <summary>
/// Enriches the body of the CreateDefaultRegistry method in the LiteralConverterRegistry class.
/// </summary>
public interface IDefaultLiteralConverterEnricher : IEnricher<ExpressionSyntax>
{
}
