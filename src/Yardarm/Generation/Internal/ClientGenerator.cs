using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Yardarm.Names;

namespace Yardarm.Generation.Internal
{
    internal class ClientGenerator : ISyntaxTreeGenerator
    {
        private readonly INamespaceProvider _namespaceProvider;

        public ClientGenerator(INamespaceProvider namespaceProvider)
        {
            _namespaceProvider = namespaceProvider ?? throw new ArgumentNullException(nameof(namespaceProvider));
        }

        public IEnumerable<SyntaxTree> Generate() =>
            typeof(ClientGenerator).Assembly.GetManifestResourceNames()
                .Where(p => p.StartsWith("Yardarm.Client.") && p.EndsWith(".cs"))
                .Select(ParseResource);

        private SyntaxTree ParseResource(string resourceName)
        {
            using var stream = typeof(ClientGenerator).Assembly
                .GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream!, Encoding.UTF8);

            var rawText = reader.ReadToEnd();
            rawText = rawText.Replace("RootNamespace", _namespaceProvider.GetRootNamespace().ToString());

            return CSharpSyntaxTree.ParseText(SourceText.From(rawText),
                CSharpParseOptions.Default
                    .WithLanguageVersion(LanguageVersion.CSharp8)
                    .WithPreprocessorSymbols("NETSTANDARD2_0"));
        }
    }
}
