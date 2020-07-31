using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Yardarm.Generation.Internal
{
    internal class NetStandardReferenceGenerator : IReferenceGenerator
    {
        private readonly string _nuGetPackagesPath = GetNugetPackagesPath();

        public IEnumerable<MetadataReference> Generate() =>
            CollectDlls(Path.Combine(_nuGetPackagesPath, @"netstandard.library\2.0.3\build\netstandard2.0\ref"))
                .Concat(CollectDlls(Path.Combine(_nuGetPackagesPath, @"system.componentmodel.annotations\4.7.0\ref\netstandard2.0")))
                .Select(p => MetadataReference.CreateFromFile(p));

        private static IEnumerable<string> CollectDlls(string directory) =>
            Directory.EnumerateFiles(directory, "*.dll");

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
