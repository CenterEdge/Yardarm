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
            if (targetFramework.Framework != NuGetFrameworkConstants.NetCoreApp || targetFramework.Version < new Version(6, 0))
            {
                // Only add System.Text.Json if we're not already targeting .NET 6

                yield return new LibraryDependency
                {
                    LibraryRange = new LibraryRange
                    {
                        Name = "System.Text.Json",
                        TypeConstraint = LibraryDependencyTarget.Package,
                        VersionRange = VersionRange.Parse("6.0.0")
                    }
                };
            }

            yield return new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
                    Name = "System.Net.Http.Json",
                    TypeConstraint = LibraryDependencyTarget.Package,
                    VersionRange = VersionRange.Parse("6.0.0")
                }
            };
        }
    }
}
