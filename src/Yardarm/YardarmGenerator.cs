using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Compilation;
using Yardarm.Packaging;

namespace Yardarm
{
    public class YardarmGenerator
    {
        public async Task<YardarmGenerationResult> EmitAsync(OpenApiDocument document, YardarmGenerationSettings settings, CancellationToken cancellationToken = default)
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

            var compilation = CSharpCompilation.Create(settings.AssemblyName)
                .WithOptions(settings.CompilationOptions);

            var enrichers = serviceProvider.GetRequiredService<IEnumerable<ICompilationEnricher>>();
            compilation = await compilation.EnrichAsync(enrichers, cancellationToken);

            var compilationResult = compilation.Emit(settings.DllOutput,
                pdbStream: settings.PdbOutput,
                xmlDocumentationStream: settings.XmlDocumentationOutput,
                options: new EmitOptions()
                    .WithDebugInformationFormat(DebugInformationFormat.PortablePdb)
                    .WithHighEntropyVirtualAddressSpace(true));

            if (compilationResult.Success)
            {
                if (settings.NuGetOutput != null)
                {
                    PackNuGet(serviceProvider, settings);
                }
            }

            return new YardarmGenerationResult(serviceProvider.GetRequiredService<GenerationContext>(), compilationResult);
        }

        private void PackNuGet(IServiceProvider serviceProvider, YardarmGenerationSettings settings)
        {
            if (!settings.DllOutput.CanRead || !settings.DllOutput.CanSeek)
            {
                throw new InvalidOperationException(
                    $"{nameof(YardarmGenerationSettings.DllOutput)} must be seekable and readable to pack a NuGet package.");
            }
            if (!settings.XmlDocumentationOutput.CanRead || !settings.XmlDocumentationOutput.CanSeek)
            {
                throw new InvalidOperationException(
                    $"{nameof(YardarmGenerationSettings.XmlDocumentationOutput)} must be seekable and readable to pack a NuGet package.");
            }

            settings.DllOutput.Seek(0, SeekOrigin.Begin);
            settings.XmlDocumentationOutput.Seek(0, SeekOrigin.Begin);

            var packer = serviceProvider.GetRequiredService<NuGetPacker>();

            packer.Pack(settings.DllOutput, settings.XmlDocumentationOutput, settings.NuGetOutput!);

            if (settings.NuGetSymbolsOutput != null)
            {
                if (!settings.PdbOutput.CanRead || !settings.PdbOutput.CanSeek)
                {
                    throw new InvalidOperationException(
                        $"{nameof(YardarmGenerationSettings.PdbOutput)} must be seekable and readable to pack a NuGet symbols package.");
                }

                settings.PdbOutput.Seek(0, SeekOrigin.Begin);

                packer.PackSymbols(settings.PdbOutput, settings.NuGetSymbolsOutput);
            }
        }
    }
}
