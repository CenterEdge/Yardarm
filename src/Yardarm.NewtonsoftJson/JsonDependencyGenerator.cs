using System.Collections.Generic;
using NuGet.LibraryModel;
using NuGet.Versioning;
using Yardarm.Packaging;

namespace Yardarm.NewtonsoftJson
{
    public class JsonDependencyGenerator : IDependencyGenerator
    {
        public IEnumerable<LibraryDependency> GetDependencies()
        {
            yield return new LibraryDependency
            {
                LibraryRange = new LibraryRange
                {
                    Name = "Newtonsoft.Json",
                    TypeConstraint = LibraryDependencyTarget.Package,
                    VersionRange = VersionRange.Parse("13.0.1")
                }
            };
        }
    }
}
