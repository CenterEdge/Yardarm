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
        public INamespaceProvider NamespaceProvider { get; }

        protected abstract string ResourcePrefix { get; }

        protected ResourceSyntaxTreeGenerator(INamespaceProvider namespaceProvider)
        {
            NamespaceProvider = namespaceProvider ?? throw new ArgumentNullException(nameof(namespaceProvider));
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
            rawText = rawText.Replace("RootNamespace", NamespaceProvider.GetRootNamespace().ToString());

            return CSharpSyntaxTree.ParseText(SourceText.From(rawText),
                CSharpParseOptions.Default
                    .WithLanguageVersion(LanguageVersion.CSharp8)
                    .WithPreprocessorSymbols("NETSTANDARD2_0"));
        }
    }
}
