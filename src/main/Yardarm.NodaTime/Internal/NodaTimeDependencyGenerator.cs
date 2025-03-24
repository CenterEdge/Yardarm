using System.Collections.Generic;
using System.Linq;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Versioning;
using Yardarm.Packaging;

namespace Yardarm.NodaTime.Internal;

internal sealed class NodaTimeDependencyGenerator(
    YardarmGenerationSettings settings)
    : IDependencyGenerator
{
    public IEnumerable<LibraryDependency> GetDependencies(NuGetFramework targetFramework)
    {
        if (settings.Extensions.Any(p => p.Name == "SystemTextJsonExtension"))
        {
            yield return new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
                    Name = "NodaTime.Serialization.SystemTextJson",
                    TypeConstraint = LibraryDependencyTarget.Package,
                    VersionRange = VersionRange.Parse("1.3.0")
                }
            };
        }
        else if (settings.Extensions.Any(p => p.Name == "NewtonsoftJsonExtension"))
        {
            yield return new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
                    Name = "NodaTime.Serialization.JsonNet",
                    TypeConstraint = LibraryDependencyTarget.Package,
                    VersionRange = VersionRange.Parse("3.2.0")
                }
            };
        }
    }
}
