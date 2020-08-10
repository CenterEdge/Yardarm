using System.Collections.Generic;
using NuGet.LibraryModel;

namespace Yardarm.Packaging
{
    public interface IDependencyGenerator
    {
        IEnumerable<LibraryDependency> GetDependencies();
    }
}
