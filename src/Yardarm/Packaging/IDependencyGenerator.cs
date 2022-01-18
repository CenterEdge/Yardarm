using System.Collections.Generic;
using NuGet.Frameworks;
using NuGet.LibraryModel;

namespace Yardarm.Packaging
{
    public interface IDependencyGenerator
    {
        IEnumerable<LibraryDependency> GetDependencies(NuGetFramework targetFramework);
    }
}
