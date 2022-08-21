using NuGet.ProjectModel;

namespace Yardarm.Packaging
{
    public interface IPackageSpecGenerator
    {
        PackageSpec Generate();
    }
}
