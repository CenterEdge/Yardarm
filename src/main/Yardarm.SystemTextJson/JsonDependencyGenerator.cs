using System.Collections.Generic;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Versioning;
using Yardarm.Packaging;

namespace Yardarm.SystemTextJson
{
    public class JsonDependencyGenerator : IDependencyGenerator
    {
        public IEnumerable<LibraryDependency> GetDependencies(NuGetFramework targetFramework)
        {
            // Add System.Text.Json even if we're targeting .NET 9 to ensure we get bug fixes, especially for the source generator.
            // This doesn't apply at the moment using 9.0.0, but it's a good practice to follow so we don't forget if we upgrade.

            yield return new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
                    Name = "System.Text.Json",
                    TypeConstraint = LibraryDependencyTarget.Package,
                    VersionRange = VersionRange.Parse("9.0.0")
                }
            };

            yield return new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
                    Name = "System.Net.Http.Json",
                    TypeConstraint = LibraryDependencyTarget.Package,
                    VersionRange = VersionRange.Parse("9.0.0")
                }
            };
        }
    }
}
