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
                            VersionRange = VersionRange.Parse("4.5.4")
                        }
                    };
                }
                else if (targetFramework.Version == NuGetFrameworkConstants.NetStandard21)
                {
                    // Only include NETStandard.Library.Ref for restore, not as a listed dependency on the generated package
                    yield return new LibraryDependency
                    {
                        LibraryRange = new LibraryRange
                        {
                            Name = "NETStandard.Library.Ref",
                            TypeConstraint = LibraryDependencyTarget.Package,
                            VersionRange = VersionRange.Parse("2.1.0")
                        },
                        IncludeType = LibraryIncludeFlags.None,
                        SuppressParent = LibraryIncludeFlags.All,
                        AutoReferenced = true,
                    };
                }

                yield return new LibraryDependency
                {
                    LibraryRange = new LibraryRange
                    {
                        Name = "System.ComponentModel.Annotations",
                        TypeConstraint = LibraryDependencyTarget.Package,
                        VersionRange = VersionRange.Parse("4.7.0")
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
            else if (targetFramework.Framework == NuGetFrameworkConstants.NetCoreApp)
            {
                yield return new LibraryDependency
                {
                    LibraryRange = new LibraryRange
                    {
                        Name = "Microsoft.NETCore.App.Ref",
                        TypeConstraint = LibraryDependencyTarget.Package,
                        VersionRange = new VersionRange(minVersion: new NuGetVersion(
                            targetFramework.Version.Major, targetFramework.Version.Minor, targetFramework.Version.Revision)),
                    },
                    IncludeType = LibraryIncludeFlags.None,
                    SuppressParent = LibraryIncludeFlags.All,
                    AutoReferenced = true,
                };
            }
        }
    }
}
