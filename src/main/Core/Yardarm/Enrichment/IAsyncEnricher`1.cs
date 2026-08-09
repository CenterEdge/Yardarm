using System.Threading;
using System.Threading.Tasks;

namespace Yardarm.Enrichment
{
    public interface IAsyncEnricher<TTarget> : IEnricher
    {
        ValueTask<TTarget> EnrichAsync(TTarget target, CancellationToken cancellationToken = default);
    }
}
