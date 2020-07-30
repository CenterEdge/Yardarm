using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Yardarm.Generation.Internal
{
    internal class NetStandardReferenceGenerator : IReferenceGenerator
    {
        public IEnumerable<MetadataReference> Generate() =>
            Directory.EnumerateFiles(
                    Path.Combine(GetNugetPackagesPath(), @"netstandard.library\2.0.3\build\netstandard2.0\ref"),
                    "*.dll")
                .Select(p => MetadataReference.CreateFromFile(p));

        private static string GetNugetPackagesPath()
        {
            var path = Environment.GetEnvironmentVariable("NUGET_PACKAGES");
            if (string.IsNullOrWhiteSpace(path))
            {
                path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget/packages");
            }

            return path;
        }
    }
}
