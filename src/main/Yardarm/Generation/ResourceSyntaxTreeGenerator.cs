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
            ArgumentNullException.ThrowIfNull(generationContext);
            ArgumentNullException.ThrowIfNull(rootNamespace);

            GenerationContext = generationContext;
            RootNamespace = rootNamespace;
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

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(rawText, Encoding.UTF8),
                CSharpParseOptions.Default
                    .WithLanguageVersion(LanguageVersion.CSharp12)
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
    }
}
