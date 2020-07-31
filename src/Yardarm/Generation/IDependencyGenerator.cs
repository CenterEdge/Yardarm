using System.Collections.Generic;
using NuGet.LibraryModel;

namespace Yardarm.Generation
{
    public interface IDependencyGenerator
    {
        IEnumerable<LibraryDependency> GetDependencies();
    }
}
