using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Yardarm.Generation.Internal;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Packaging;

namespace Yardarm.Generation
{
    public abstract class ResourceSyntaxTreeGenerator : ISyntaxTreeGenerator
    {
        public GenerationContext GenerationContext { get; }
        public IRootNamespace RootNamespace { get; }

        protected abstract string ResourcePrefix { get; }

        protected ResourceSyntaxTreeGenerator(GenerationContext generationContext, IRootNamespace rootNamespace)
        {
            GenerationContext = generationContext ?? throw new ArgumentNullException(nameof(generationContext));
            RootNamespace = rootNamespace ?? throw new ArgumentNullException(nameof(rootNamespace));
        }

        public virtual IEnumerable<SyntaxTree> Generate()
        {
            string excludeSuffix = GenerationContext.CurrentTargetFramework.Framework ==
                                   NuGetFrameworkConstants.NetStandardFramework
                ? ".netcoreapp.cs"
                : ".netstandard.cs";

            return GetType().Assembly.GetManifestResourceNames()
                .Where(p => p.StartsWith(ResourcePrefix) && p.EndsWith(".cs") && !p.EndsWith(excludeSuffix))
                .Select(ParseResource);
        }

        private SyntaxTree ParseResource(string resourceName)
        {
            using var stream = GetType().Assembly
                .GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!, Encoding.UTF8);

            string rawText = reader.ReadToEnd();
            rawText = rawText.Replace("RootNamespace", RootNamespace.Name.ToString());

            string[] preprocessorSymbols = GenerationContext.CurrentTargetFramework switch
            {
                {Framework: NuGetFrameworkConstants.NetStandardFramework} =>
                    GetNetStandardPreprocessorSymbols(GenerationContext.CurrentTargetFramework.Version),
                {Framework: NuGetFrameworkConstants.NetCoreApp, Version.Major: < 5 } =>
                    GetNetCoreAppPreprocessorSymbols(GenerationContext.CurrentTargetFramework.Version),
                {Framework: NuGetFrameworkConstants.NetCoreApp, Version.Major: >= 5 } =>
                    GetNetPreprocessorSymbols(GenerationContext.CurrentTargetFramework.Version),
                _ => Array.Empty<string>()
            };

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(rawText, Encoding.UTF8),
                CSharpParseOptions.Default
                    .WithLanguageVersion(LanguageVersion.CSharp10)
                    .WithPreprocessorSymbols(preprocessorSymbols),
                path: PathHelpers.Combine(
                    GenerationContext.Settings.BasePath,
                    "Resources",
                    PathHelpers.NormalizePath(resourceName)));

            // Annotate the compilation root so we know which resource file it came from
            syntaxTree = syntaxTree.WithRootAndOptions(
                syntaxTree.GetCompilationUnitRoot()
                    .AddResourceNameAnnotation(resourceName),
                syntaxTree.Options);

            return syntaxTree;
        }

        private static readonly ConcurrentDictionary<Version, string[]> s_netStandardPreprocessorDirectives = new();
        private static string[] GetNetStandardPreprocessorSymbols(Version version) =>
            s_netStandardPreprocessorDirectives.GetOrAdd(version, static v =>
            {
                var result = new List<string> {"NETSTANDARD", $"NETSTANDARD{v.Major}_{v.Minor}"};

                foreach (var version in new Version[] {new(2, 0), new(2, 1)}.Where(p => p <= v))
                {
                    result.Add($"NETSTANDARD{version.Major}_{version.Minor}_OR_GREATER");
                }

                return result.ToArray();
            });

        private static readonly ConcurrentDictionary<Version, string[]> s_netCoreAppPreprocessorDirectives = new();
        private static string[] GetNetCoreAppPreprocessorSymbols(Version version) =>
            s_netCoreAppPreprocessorDirectives.GetOrAdd(version, static v =>
            {
                var result = new List<string> {"NETCOREAPP", $"NETCOREAPP{v.Major}_{v.Minor}"};

                foreach (var version in new Version[] {new(2, 0), new(2, 1), new(3, 0), new Version(3, 1)}.Where(p => p <= v))
                {
                    result.Add($"NETCOREAPP{version.Major}_{version.Minor}_OR_GREATER");
                }

                return result.ToArray();
            });

        private static readonly ConcurrentDictionary<Version, string[]> s_netPreprocessorDirectives = new();
        private static string[] GetNetPreprocessorSymbols(Version version) =>
            s_netPreprocessorDirectives.GetOrAdd(version, static v =>
            {
                var result = new List<string> {$"NET{v.Major}_{v.Minor}"};

                foreach (var version in new Version[] {new(5, 0), new(6, 0)}.Where(p => p <= v))
                {
                    result.Add($"NET{version.Major}_{version.Minor}_OR_GREATER");
                }

                // Also include all .NET Core 3.1 symbols except NETCOREAPP3_1
                result.AddRange(GetNetCoreAppPreprocessorSymbols(new Version(3, 1)).Except(new[] {"NETCOREAPP3_1"}));

                return result.ToArray();
            });
    }
}
