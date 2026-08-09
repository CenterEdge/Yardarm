using System.Threading;
using System.Threading.Tasks;

namespace Yardarm.Enrichment
{
    public interface IAsyncEnricher<TTarget, in TContext> : IEnricher
    {
        ValueTask<TTarget> EnrichAsync(TTarget target, TContext context, CancellationToken cancellationToken = default);
    }
}
