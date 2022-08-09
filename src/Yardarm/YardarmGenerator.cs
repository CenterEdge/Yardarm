using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
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

            var toDispose = new List<IDisposable>();
            try
            {
                var serviceProvider = settings.BuildServiceProvider(document);
                var context = serviceProvider.GetRequiredService<GenerationContext>();

                // Perform the NuGet restore
                var restoreProcessor = serviceProvider.GetRequiredService<NuGetRestoreProcessor>();
                context.NuGetRestoreInfo = await restoreProcessor.ExecuteAsync(cancellationToken).ConfigureAwait(false);

                if (settings.TargetFrameworkMonikers.Length == 0)
                {
                    throw new InvalidOperationException("No target framework monikers provided.");
                }

                var compilationResults = new List<YardarmCompilationResult>();

                if (settings.TargetFrameworkMonikers.Length == 1)
                {
                    var targetFramework = NuGetFramework.Parse(settings.TargetFrameworkMonikers[0]);

                    // Perform the compilation
                    var (emitResult, additionalDiagnostics) = await BuildForTargetFrameworkAsync(
                        context, settings, targetFramework,
                        settings.DllOutput, settings.PdbOutput, settings.XmlDocumentationOutput,
                        cancellationToken).ConfigureAwait(false);

                    compilationResults.Add(new(targetFramework, emitResult,
                        settings.DllOutput, settings.PdbOutput, settings.XmlDocumentationOutput,
                        additionalDiagnostics));
                }
                else
                {
                    if (settings.NuGetOutput is null)
                    {
                        throw new InvalidOperationException(
                            "Targeting multiple frameworks is only supported with NuGet output.");
                    }

                    foreach (var targetFramework in settings.TargetFrameworkMonikers.Select(NuGetFramework.Parse))
                    {
                        var dllOutput = new MemoryStream();
                        toDispose.Add(dllOutput);
                        var pdbOutput = new MemoryStream();
                        toDispose.Add(pdbOutput);
                        var xmlDocumentationOutput = new MemoryStream();
                        toDispose.Add(xmlDocumentationOutput);

                        // Perform the compilation
                        var (emitResult, additionalDiagnostics) = await BuildForTargetFrameworkAsync(
                            context, settings, targetFramework,
                            dllOutput, pdbOutput, xmlDocumentationOutput,
                            cancellationToken).ConfigureAwait(false);

                        compilationResults.Add(new(targetFramework, emitResult,
                            dllOutput, pdbOutput, xmlDocumentationOutput,
                            additionalDiagnostics));
                    }
                }

                if (compilationResults.All(p => p.EmitResult.Success))
                {
                    if (settings.NuGetOutput != null)
                    {
                        PackNuGet(serviceProvider, settings, compilationResults);
                    }
                }

                return new YardarmGenerationResult(serviceProvider.GetRequiredService<GenerationContext>(),
                    compilationResults);
            }
            finally
            {
                foreach (var disposable in toDispose)
                {
                    disposable.Dispose();
                }
            }
        }

        private async Task<(EmitResult, ImmutableArray<Diagnostic>)> BuildForTargetFrameworkAsync(
            GenerationContext context, YardarmGenerationSettings settings, NuGetFramework targetFramework,
            Stream dllOutput, Stream pdbOutput, Stream xmlDocumentationOutput,
            CancellationToken cancellationToken = default)
        {
            context.CurrentTargetFramework = targetFramework;

            // Create the empty compilation
            var compilation = CSharpCompilation.Create(settings.AssemblyName)
                .WithOptions(settings.CompilationOptions);

            // Run all enrichers against the compilation
            var enrichers = context.GenerationServices.GetRequiredService<IEnumerable<ICompilationEnricher>>();
            compilation = await compilation.EnrichAsync(enrichers, cancellationToken);

            ImmutableArray<Diagnostic> additionalDiagnostics;
            var assemblyLoadContext = new YardarmAssemblyLoadContext();
            try
            {
                var sourceGenerators = context.GenerationServices.GetRequiredService<NuGetRestoreProcessor>()
                    .GetSourceGenerators(context.NuGetRestoreInfo!.Providers, context.NuGetRestoreInfo!.Result,
                        targetFramework, assemblyLoadContext);

                // Execute the source generators
                compilation = ExecuteSourceGenerators(compilation,
                    sourceGenerators,
                    out additionalDiagnostics,
                    cancellationToken);
            }
            finally
            {
                assemblyLoadContext.Unload();
            }

            return (compilation.Emit(dllOutput,
                    pdbStream: pdbOutput,
                    xmlDocumentationStream: xmlDocumentationOutput,
                    options: new EmitOptions()
                        .WithDebugInformationFormat(DebugInformationFormat.PortablePdb)
                        .WithHighEntropyVirtualAddressSpace(true)),
                additionalDiagnostics);
        }

        private void PackNuGet(IServiceProvider serviceProvider, YardarmGenerationSettings settings, List<YardarmCompilationResult> results)
        {
            foreach (var result in results)
            {
                if (!result.DllOutput.CanRead || !result.DllOutput.CanSeek)
                {
                    throw new InvalidOperationException(
                        $"{nameof(YardarmGenerationSettings.DllOutput)} must be seekable and readable to pack a NuGet package.");
                }

                if (!result.XmlDocumentationOutput.CanRead || !result.XmlDocumentationOutput.CanSeek)
                {
                    throw new InvalidOperationException(
                        $"{nameof(YardarmGenerationSettings.XmlDocumentationOutput)} must be seekable and readable to pack a NuGet package.");
                }

                result.DllOutput.Seek(0, SeekOrigin.Begin);
                result.XmlDocumentationOutput.Seek(0, SeekOrigin.Begin);

                if (settings.NuGetSymbolsOutput != null)
                {
                    if (!result.PdbOutput.CanRead || !result.PdbOutput.CanSeek)
                    {
                        throw new InvalidOperationException(
                            $"{nameof(YardarmGenerationSettings.PdbOutput)} must be seekable and readable to pack a NuGet symbols package.");
                    }

                    result.PdbOutput.Seek(0, SeekOrigin.Begin);
                }
            }


            var packer = serviceProvider.GetRequiredService<NuGetPacker>();

            packer.Pack(results, settings.NuGetOutput!);

            if (settings.NuGetSymbolsOutput != null)
            {
                if (!settings.PdbOutput.CanRead || !settings.PdbOutput.CanSeek)
                {
                    throw new InvalidOperationException(
                        $"{nameof(YardarmGenerationSettings.PdbOutput)} must be seekable and readable to pack a NuGet symbols package.");
                }



                packer.PackSymbols(results, settings.NuGetSymbolsOutput);
            }
        }

        private CSharpCompilation ExecuteSourceGenerators(CSharpCompilation compilation, IReadOnlyList<ISourceGenerator>? generators,
            out ImmutableArray<Diagnostic> diagnostics, CancellationToken cancellationToken = default)
        {
            if (generators is null || generators.Count == 0)
            {
                diagnostics = ImmutableArray<Diagnostic>.Empty;
                return compilation;
            }

            var driver = CSharpGeneratorDriver.Create(generators);

            driver.RunGeneratorsAndUpdateCompilation(compilation, out var newCompilation, out diagnostics, cancellationToken);

            return (CSharpCompilation) newCompilation;
        }
    }
}
