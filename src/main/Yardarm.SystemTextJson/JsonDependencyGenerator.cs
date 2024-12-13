using System;
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
            // Add System.Text.Json even if we're targeting .NET 8 to ensure we get bug fixes, especially for the source generator

            yield return new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
                    Name = "System.Text.Json",
                    TypeConstraint = LibraryDependencyTarget.Package,
                    VersionRange = VersionRange.Parse("8.0.5")
                }
            };

            yield return new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
                    Name = "System.Net.Http.Json",
                    TypeConstraint = LibraryDependencyTarget.Package,
                    VersionRange = VersionRange.Parse("8.0.1")
                }
            };
        }
    }
}
