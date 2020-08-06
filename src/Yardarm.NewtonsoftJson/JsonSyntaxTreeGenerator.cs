using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Yardarm.Generation;
using Yardarm.Names;

namespace Yardarm.NewtonsoftJson
{
    public class JsonSyntaxTreeGenerator : ISyntaxTreeGenerator
    {
        private readonly INamespaceProvider _namespaceProvider;

        public JsonSyntaxTreeGenerator(INamespaceProvider namespaceProvider)
        {
            _namespaceProvider = namespaceProvider ?? throw new ArgumentNullException(nameof(namespaceProvider));
        }

        public void Preprocess()
        {
        }

        public IEnumerable<SyntaxTree> Generate()
        {
            yield return ParseResource("DiscriminatorConverter.txt");
        }

        private SyntaxTree ParseResource(string resourceName)
        {
            using var stream = typeof(JsonSyntaxTreeGenerator).Assembly
                .GetManifestResourceStream("Yardarm.NewtonsoftJson.Resources." + resourceName);
            using var reader = new StreamReader(stream, Encoding.UTF8);

            var rawText = reader.ReadToEnd();
            rawText = rawText.Replace("$rootnamespace$", _namespaceProvider.GetRootNamespace().ToString());

            return CSharpSyntaxTree.ParseText(SourceText.From(rawText), CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8));
        }
    }
}
