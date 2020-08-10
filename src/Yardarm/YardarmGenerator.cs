using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Packaging;

namespace Yardarm
{
    public class YardarmGenerator
    {
        public async Task<EmitResult> EmitAsync(OpenApiDocument document, YardarmGenerationSettings settings, CancellationToken cancellationToken = default)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            var serviceProvider = settings.BuildServiceProvider(document);

            ISyntaxTreeGenerator[] syntaxTreeGenerators =
                serviceProvider.GetRequiredService<IEnumerable<ISyntaxTreeGenerator>>().ToArray();

            Parallel.ForEach(syntaxTreeGenerators, p => p.Preprocess());

            SyntaxTree[] syntaxTrees = syntaxTreeGenerators
                .AsParallel()
                .AsUnordered()
                .WithCancellation(cancellationToken)
                .SelectMany(p => p.Generate())
                .ToArray();

            List<MetadataReference> references = await serviceProvider.GetRequiredService<IEnumerable<IReferenceGenerator>>()
                .ToAsyncEnumerable()
                .SelectMany(p => p.Generate(cancellationToken))
                .ToListAsync(cancellationToken);

            var compilation = CSharpCompilation.Create(settings.AssemblyName)
                .WithOptions(settings.CompilationOptions)
                .AddReferences(references.Distinct())
                .AddSyntaxTrees(syntaxTrees);

            var compilationResult = compilation.Emit(settings.DllOutput,
                pdbStream: settings.PdbOutput,
                xmlDocumentationStream: settings.XmlDocumentationOutput,
                options: new EmitOptions()
                    .WithDebugInformationFormat(DebugInformationFormat.PortablePdb)
                    .WithHighEntropyVirtualAddressSpace(true));

            if (!compilationResult.Success)
            {
                return compilationResult;
            }

            if (settings.NuGetOutput != null)
            {
                PackNuGet(serviceProvider, settings);
            }

            return compilationResult;
        }

        private void PackNuGet(IServiceProvider serviceProvider, YardarmGenerationSettings settings)
        {
            if (!settings.DllOutput.CanRead || !settings.DllOutput.CanSeek)
            {
                throw new InvalidOperationException(
                    $"{nameof(YardarmGenerationSettings.DllOutput)} must be seekable and readable to pack a NuGet package.");
            }
            if (!settings.PdbOutput.CanRead || !settings.PdbOutput.CanSeek)
            {
                throw new InvalidOperationException(
                    $"{nameof(YardarmGenerationSettings.PdbOutput)} must be seekable and readable to pack a NuGet package.");
            }
            if (!settings.XmlDocumentationOutput.CanRead || !settings.XmlDocumentationOutput.CanSeek)
            {
                throw new InvalidOperationException(
                    $"{nameof(YardarmGenerationSettings.XmlDocumentationOutput)} must be seekable and readable to pack a NuGet package.");
            }

            settings.DllOutput.Seek(0, SeekOrigin.Begin);
            settings.PdbOutput.Seek(0, SeekOrigin.Begin);
            settings.XmlDocumentationOutput.Seek(0, SeekOrigin.Begin);

            var packer = serviceProvider.GetRequiredService<NuGetPacker>();

            packer.Pack(settings.DllOutput, settings.PdbOutput, settings.XmlDocumentationOutput, settings.NuGetOutput!);
        }
    }
}
