using System.Collections.Generic;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Versioning;
using Yardarm.Packaging;

namespace Yardarm.SystemTextJson;

public class JsonDependencyGenerator : IDependencyGenerator
{
    public IEnumerable<LibraryDependency> GetDependencies(NuGetFramework targetFramework)
    {
        if (targetFramework.Version.Major < 9)
        {
            // Upgrade System.Text.Json to at least 9.0 if we're targeting downlevel frameworks
            yield return new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
                    Name = "System.Text.Json",
                    TypeConstraint = LibraryDependencyTarget.Package,
                    VersionRange = VersionRange.Parse("9.0.0")
                }
            };
        }

        if (targetFramework.Version.Major < 10)
        {
            // System.Net.Http.Json is in-box in .NET 10 and later
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
