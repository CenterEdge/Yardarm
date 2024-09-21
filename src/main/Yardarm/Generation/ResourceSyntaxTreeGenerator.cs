using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Yardarm.Helpers;
using Yardarm.Internal;
using Yardarm.Names;
using Yardarm.Packaging;

namespace Yardarm.Generation
{
    public abstract class ResourceSyntaxTreeGenerator : ISyntaxTreeGenerator
    {
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

        public virtual IEnumerable<SyntaxTree> Generate()
        {
            string excludeSuffix = GenerationContext.CurrentTargetFramework.Framework ==
                                   NuGetFrameworkConstants.NetStandardFramework
                ? ".netcoreapp.cs"
                : ".netstandard.cs";

            byte[] namespaceName = s_utf8NoBom.GetBytes(RootNamespace.Name.ToString());

            var result = new List<SyntaxTree>();
            foreach (string resourceName in GetType().Assembly.GetManifestResourceNames())
            {
                if (resourceName.StartsWith(ResourcePrefix) && resourceName.EndsWith(".cs") && !resourceName.EndsWith(excludeSuffix))
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
