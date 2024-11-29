using System.Collections.Generic;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Versioning;

namespace Yardarm.Packaging.Internal
{
    internal class StandardDependencyGenerator : IDependencyGenerator
    {
        public IEnumerable<LibraryDependency> GetDependencies(NuGetFramework targetFramework)
        {
            if (targetFramework.Framework == NuGetFrameworkConstants.NetStandardFramework)
            {
                if (targetFramework.Version == NuGetFrameworkConstants.NetStandard20)
                {
                    // Only include NETStandard.Library for restore, not as a listed dependency on the generated package
                    yield return new LibraryDependency
                    {
                        LibraryRange = new LibraryRange
                        {
                            Name = "NETStandard.Library",
                            TypeConstraint = LibraryDependencyTarget.Package,
                            VersionRange = VersionRange.Parse("2.0.3")
                        },
                        SuppressParent = LibraryIncludeFlags.All,
                        AutoReferenced = true,
                    };

                    // Only include System.Threading.Tasks.Extensions for netstandard2.0
                    yield return new LibraryDependency
                    {
                        LibraryRange = new LibraryRange
                        {
                            Name = "System.Threading.Tasks.Extensions",
                            TypeConstraint = LibraryDependencyTarget.Package,
                            VersionRange = VersionRange.Parse("4.6.0")
                        }
                    };
                }

                yield return new LibraryDependency
                {
                    LibraryRange = new LibraryRange
                    {
                        Name = "System.ComponentModel.Annotations",
                        TypeConstraint = LibraryDependencyTarget.Package,
                        VersionRange = VersionRange.Parse("5.0.0")
                    }
                };

                yield return new LibraryDependency
                {
                    LibraryRange = new LibraryRange
                    {
                        Name = "System.Collections.Immutable",
                        TypeConstraint = LibraryDependencyTarget.Package,
                        VersionRange = VersionRange.Parse("8.0.0")
                    }
                };

                yield return new LibraryDependency
                {
                    LibraryRange = new LibraryRange
                    {
                        Name = "Microsoft.CSharp",
                        TypeConstraint = LibraryDependencyTarget.Package,
                        VersionRange = VersionRange.Parse("4.7.0")
                    }
                };
            }
            else if (targetFramework.Version.Major < 8)
            {
                // .NET < 8 also requires System.Collections.Immutable for FrozenDictionary
                yield return new LibraryDependency
                {
                    LibraryRange = new LibraryRange
                    {
                        Name = "System.Collections.Immutable",
                        TypeConstraint = LibraryDependencyTarget.Package,
                        VersionRange = VersionRange.Parse("8.0.0")
                    }
                };
            }
        }
    }
}
