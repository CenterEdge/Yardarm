using System.Collections.Generic;
using NuGet.Frameworks;
using NuGet.LibraryModel;
using NuGet.Versioning;
using Yardarm.Packaging;

namespace Yardarm.MicrosoftExtensionsHttp;

public class DependencyInjectionDependencyGenerator : IDependencyGenerator
{
    public IEnumerable<LibraryDependency> GetDependencies(NuGetFramework targetFramework)
    {
        yield return new LibraryDependency
        {
            LibraryRange = new LibraryRange
            {
                Name = "Microsoft.Extensions.Http",
                TypeConstraint = LibraryDependencyTarget.Package,
                VersionRange = VersionRange.Parse("8.0.1")
            }
        };
    }
}
