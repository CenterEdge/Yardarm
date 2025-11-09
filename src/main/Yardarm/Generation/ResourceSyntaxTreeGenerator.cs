using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Yardarm.Helpers;
using Yardarm.Internal;
using Yardarm.Names;
using Yardarm.Packaging;

namespace Yardarm.Generation
{
    public abstract partial class ResourceSyntaxTreeGenerator : ISyntaxTreeGenerator
    {
        [GeneratedRegex(@"\.netstandard\.cs$")]
        private static partial Regex NetStandardSuffix();

        [GeneratedRegex(@"\.netcoreapp\.cs$")]
        private static partial Regex NetCoreAppSuffix();

        [GeneratedRegex(@"\.net\d+\.\d+\.cs$")]
        private static partial Regex AnyNetNumberSuffix();

        [GeneratedRegex(@"\.net6\.0\.cs$")]
        private static partial Regex Net60Suffix();

        [GeneratedRegex(@"\.net7\.0\.cs$")]
        private static partial Regex Net70Suffix();

        [GeneratedRegex(@"\.net8\.0\.cs$")]
        private static partial Regex Net80Suffix();

        [GeneratedRegex(@"\.net9\.0\.cs$")]
        private static partial Regex Net90Suffix();

        private static readonly UTF8Encoding s_utf8NoBom = new(encoderShouldEmitUTF8Identifier: false);
        private static ReadOnlySpan<byte> RootNamespaceBytes => "RootNamespace"u8;

        public GenerationContext GenerationContext { get; }
        public IRootNamespace RootNamespace { get; }

        protected abstract string ResourcePrefix { get; }

        protected ResourceSyntaxTreeGenerator(GenerationContext generationContext, IRootNamespace rootNamespace)
        {
            ArgumentNullException.ThrowIfNull(generationContext);
            ArgumentNullException.ThrowIfNull(rootNamespace);

            GenerationContext = generationContext;
            RootNamespace = rootNamespace;
        }

        public virtual IEnumerable<Regex> GetResourceNameExclusions() =>
            GenerationContext.CurrentTargetFramework.Framework ==
                                       NuGetFrameworkConstants.NetStandardFramework
                ? [NetCoreAppSuffix(), AnyNetNumberSuffix()]
                : [NetStandardSuffix(), .. GetNetVersionSuffixExclusions(GenerationContext.CurrentTargetFramework.Version)];

        public virtual IEnumerable<SyntaxTree> Generate()
        {
            Regex[] excludeSuffixes = GetResourceNameExclusions().ToArray();

            byte[] namespaceName = s_utf8NoBom.GetBytes(RootNamespace.Name.ToString());

            var result = new List<SyntaxTree>();
            foreach (string resourceName in GetType().Assembly.GetManifestResourceNames())
            {
                if (resourceName.StartsWith(ResourcePrefix) && resourceName.EndsWith(".cs") &&
                    !IsAnyMatch(excludeSuffixes, resourceName))
                {
                    result.Add(ParseResource(resourceName, namespaceName));
                }
            };

            return result;
        }

        private unsafe SyntaxTree ParseResource(string resourceName, ReadOnlySpan<byte> namespaceName)
        {
            using var stream = (UnmanagedMemoryStream)GetType().Assembly
                .GetManifestResourceStream(resourceName)!;
            if (stream.Length > int.MaxValue)
            {
                throw new InvalidOperationException("Resource is too large to parse");
            }

            int length = (int)stream.Length;
            ReadOnlySpan<byte> resourceContents = new(stream.PositionPointer, length);

            SourceText sourceText = resourceContents.TryCopyWithReplace(RootNamespaceBytes, namespaceName, out ArrayPoolBuffer<byte> buffer)
                ? SourceText.From(buffer.Buffer!, buffer.Length, s_utf8NoBom, canBeEmbedded: true)
                : SourceText.From(stream, s_utf8NoBom, canBeEmbedded: true);

            // Return the buffer, if any, to the array pool
            buffer.Dispose();

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(sourceText,
                CSharpParseOptions.Default
                    .WithLanguageVersion(LanguageVersion.CSharp14)
                    .WithPreprocessorSymbols(GenerationContext.PreprocessorSymbols),
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

        private static IEnumerable<Regex> GetNetVersionSuffixExclusions(Version version)
        {
            if (version.Major < 6)
            {
                yield return Net60Suffix();
            }
            if (version.Major < 7)
            {
                yield return Net70Suffix();
            }
            if (version.Major < 8)
            {
                yield return Net80Suffix();
            }
            if (version.Major < 9)
            {
                yield return Net90Suffix();
            }
        }

        private static bool IsAnyMatch(Regex[] patterns, string input)
        {
            foreach (Regex pattern in patterns)
            {
                if (pattern.IsMatch(input))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
