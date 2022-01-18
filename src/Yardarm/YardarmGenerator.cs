using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NuGet.Frameworks;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Compilation;
using Yardarm.Internal;
using Yardarm.Packaging;
using Yardarm.Packaging.Internal;

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
            try
            {
                var context = serviceProvider.GetRequiredService<GenerationContext>();
                context.CurrentTargetFramework = NuGetFramework.Parse("netstandard2.0");

                // Perform the NuGet restore
                var restoreProcessor = serviceProvider.GetRequiredService<NuGetRestoreProcessor>();
                context.NuGetRestoreInfo = await restoreProcessor.ExecuteAsync(cancellationToken).ConfigureAwait(false);

                // Create the empty compilation
                var compilation = CSharpCompilation.Create(settings.AssemblyName)
                    .WithOptions(settings.CompilationOptions);

                // Run all enrichers against the compilation
                var enrichers = serviceProvider.GetRequiredService<IEnumerable<ICompilationEnricher>>();
                compilation = await compilation.EnrichAsync(enrichers, cancellationToken);

                // Execute the source generators
                compilation = ExecuteSourceGenerators(compilation,
                    context.NuGetRestoreInfo.SourceGenerators,
                    out var additionalDiagnostics,
                    cancellationToken);

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

                return new YardarmGenerationResult(serviceProvider.GetRequiredService<GenerationContext>(),
                    compilationResult, additionalDiagnostics);
            }
            finally
            {
                serviceProvider.GetRequiredService<YardarmAssemblyLoadContext>().Unload();
            }
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

        private CSharpCompilation ExecuteSourceGenerators(CSharpCompilation compilation, IReadOnlyList<ISourceGenerator>? generators,
            out ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken = default)
        {
            if (generators is null || generators.Count == 0)
            {
                return compilation;
            }

            var driver = CSharpGeneratorDriver.Create(generators);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out diagnostics, cancellationToken);

            return (CSharpCompilation) newCompilation;
        }
    }
}
