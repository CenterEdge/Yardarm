using NuGet.ProjectModel;

namespace Yardarm.Generation
{
    public interface IPackageSpecGenerator
    {
        PackageSpec Generate();
    }
}
