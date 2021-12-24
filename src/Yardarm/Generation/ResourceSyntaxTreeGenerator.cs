using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Yardarm.Generation.Internal;
using Yardarm.Names;

namespace Yardarm.Generation
{
    public abstract class ResourceSyntaxTreeGenerator : ISyntaxTreeGenerator
    {
        public IRootNamespace RootNamespace { get; }

        protected abstract string ResourcePrefix { get; }

        protected ResourceSyntaxTreeGenerator(IRootNamespace rootNamespace)
        {
            RootNamespace = rootNamespace ?? throw new ArgumentNullException(nameof(rootNamespace));
        }

        public virtual IEnumerable<SyntaxTree> Generate() =>
            GetType().Assembly.GetManifestResourceNames()
                .Where(p => p.StartsWith(ResourcePrefix) && p.EndsWith(".cs"))
                .Select(ParseResource);

        private SyntaxTree ParseResource(string resourceName)
        {
            using var stream = GetType().Assembly
                .GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!, Encoding.UTF8);

            string rawText = reader.ReadToEnd();
            rawText = rawText.Replace("RootNamespace", RootNamespace.Name.ToString());

            SyntaxTree syntaxTree = CSharpSyntaxTree.ParseText(SourceText.From(rawText),
                CSharpParseOptions.Default
                    .WithLanguageVersion(LanguageVersion.CSharp10)
                    .WithPreprocessorSymbols("NETSTANDARD2_0", "NETSTANDARD2_0_OR_GREATER"));

            // Annotate the compilation root so we know which resource file it came from
            syntaxTree = syntaxTree.WithRootAndOptions(
                syntaxTree.GetCompilationUnitRoot()
                    .AddResourceNameAnnotation(resourceName),
                syntaxTree.Options);

            return syntaxTree;
        }
    }
}
