using System.Collections.Generic;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Versioning;
using Yardarm.Packaging;

namespace Yardarm.NodaTime;

public class NodaTimeDependencyGenerator : IDependencyGenerator
{
    public IEnumerable<LibraryDependency> GetDependencies(NuGetFramework targetFramework) =>
    [
        new LibraryDependency
        {
            LibraryRange = new LibraryRange
            {
                Name = "NodaTime.Serialization.SystemTextJson",
                TypeConstraint = LibraryDependencyTarget.Package,
                VersionRange = VersionRange.Parse("1.2.0")
            }
        }
    ];
}
