using NuGet.ProjectModel;

namespace Yardarm.Enrichment
{
    public interface IPackageSpecEnricher
    {
        PackageSpec Enrich(PackageSpec packageSpec);
    }
}
