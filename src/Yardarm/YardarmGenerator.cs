using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NuGet.Frameworks;
using NuGet.ProjectModel;
using Yardarm.Enrichment;
using Yardarm.Enrichment.Compilation;
using Yardarm.Internal;
using Yardarm.Packaging;
using Yardarm.Packaging.Internal;

namespace Yardarm
{
    public class YardarmGenerator
    {
        private readonly OpenApiDocument _document;
        private readonly YardarmGenerationSettings _settings;
        private readonly IServiceProvider _serviceProvider;

        public YardarmGenerator(OpenApiDocument document, YardarmGenerationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(document);
            ArgumentNullException.ThrowIfNull(settings);
            
            _document = document;
            _settings = settings;
            _serviceProvider = settings.BuildServiceProvider(document);
        }

        public Task<PackageSpec> GetPackageSpecAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_serviceProvider.GetRequiredService<PackageSpec>());
        }

        public async Task<NuGetRestoreInfo> RestoreAsync(CancellationToken cancellationToken = default)
        {
            var context = _serviceProvider.GetRequiredService<GenerationContext>();

            await PerformRestoreAsync(context, false, cancellationToken);

            return context.NuGetRestoreInfo!;
        }

        private async Task PerformRestoreAsync(GenerationContext context, bool readLockFileOnly, CancellationToken cancellationToken = default)
        {
            // Perform the NuGet restore
            var restoreProcessor = context.GenerationServices.GetRequiredService<NuGetRestoreProcessor>();
            context.NuGetRestoreInfo = await restoreProcessor.ExecuteAsync(readLockFileOnly, cancellationToken).ConfigureAwait(false);

            if (context.Settings.TargetFrameworkMonikers.Length == 0)
            {
                throw new InvalidOperationException("No target framework monikers provided.");
            }
        }

        public async Task<YardarmGenerationResult> EmitAsync(CancellationToken cancellationToken = default)
        {
            var toDispose = new List<IDisposable>();
            try
            {
                var context = _serviceProvider.GetRequiredService<GenerationContext>();

                await PerformRestoreAsync(context, _settings.NoRestore, cancellationToken);

                var compilationResults = new List<YardarmCompilationResult>();

                if (_settings.TargetFrameworkMonikers.Length == 1)
                {
                    var targetFramework = NuGetFramework.Parse(_settings.TargetFrameworkMonikers[0]);

                    // Perform the compilation
                    var (emitResult, additionalDiagnostics) = await BuildForTargetFrameworkAsync(
                        context, targetFramework,
                        _settings.DllOutput, _settings.PdbOutput, _settings.XmlDocumentationOutput, _settings.ReferenceAssemblyOutput,
                        cancellationToken).ConfigureAwait(false);

                    compilationResults.Add(new(targetFramework, emitResult,
                        _settings.DllOutput, _settings.PdbOutput, _settings.XmlDocumentationOutput, _settings.ReferenceAssemblyOutput,
                        additionalDiagnostics));
                }
                else
                {
                    if (_settings.NuGetOutput is null)
                    {
                        throw new InvalidOperationException(
                            "Targeting multiple frameworks is only supported with NuGet output.");
                    }

                    foreach (var targetFramework in _settings.TargetFrameworkMonikers.Select(NuGetFramework.Parse))
                    {
                        var dllOutput = new MemoryStream();
                        toDispose.Add(dllOutput);
                        var pdbOutput = new MemoryStream();
                        toDispose.Add(pdbOutput);
                        var xmlDocumentationOutput = new MemoryStream();
                        toDispose.Add(xmlDocumentationOutput);
                        var referenceAssemblyOutput = new MemoryStream();
                        toDispose.Add(referenceAssemblyOutput);

                        // Perform the compilation
                        var (emitResult, additionalDiagnostics) = await BuildForTargetFrameworkAsync(
                            context, targetFramework,
                            dllOutput, pdbOutput, xmlDocumentationOutput, referenceAssemblyOutput,
                            cancellationToken).ConfigureAwait(false);

                        compilationResults.Add(new(targetFramework, emitResult,
                            dllOutput, pdbOutput, xmlDocumentationOutput, referenceAssemblyOutput,
                            additionalDiagnostics));
                    }
                }

                if (compilationResults.All(p => p.EmitResult.Success))
                {
                    if (_settings.NuGetOutput != null)
                    {
                        PackNuGet(compilationResults);
                    }
                }

                return new YardarmGenerationResult(context, compilationResults);
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
            GenerationContext context, NuGetFramework targetFramework,
            Stream dllOutput, Stream pdbOutput, Stream xmlDocumentationOutput, Stream? referenceAssemblyOutput,
            CancellationToken cancellationToken = default)
        {
            context.CurrentTargetFramework = targetFramework;

            // Create the empty compilation
            var compilation = CSharpCompilation.Create(_settings.AssemblyName)
                .WithOptions(_settings.CompilationOptions);

            // Run all enrichers against the compilation
            var enrichers = context.GenerationServices.GetRequiredService<IEnumerable<ICompilationEnricher>>();
            compilation = await compilation.EnrichAsync(enrichers, cancellationToken);

            ImmutableArray<Diagnostic> additionalDiagnostics;
            var assemblyLoadContext = new YardarmAssemblyLoadContext();
            try
            {
                var sourceGenerators = context.GenerationServices.GetRequiredService<NuGetRestoreProcessor>()
                    .GetSourceGenerators(context.NuGetRestoreInfo!.Providers, context.NuGetRestoreInfo!.LockFile,
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
                    metadataPEStream: referenceAssemblyOutput,
                    embeddedTexts: await GetEmbeddedTextsAsync(compilation, cancellationToken)
                        .ToListAsync(cancellationToken),
                    options: new EmitOptions()
                        .WithDebugInformationFormat(DebugInformationFormat.PortablePdb)
                        .WithHighEntropyVirtualAddressSpace(true)
                        .WithIncludePrivateMembers(false)),
                additionalDiagnostics);
        }

        private async IAsyncEnumerable<EmbeddedText> GetEmbeddedTextsAsync(
            CSharpCompilation compilation, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (!_settings.EmbedAllSources)
            {
                yield break;
            }

            foreach (var syntaxTree in compilation.SyntaxTrees
                         .Where(static p => p.FilePath != "")
                         .Cast<CSharpSyntaxTree>())
            {
                var content = (await syntaxTree.GetRootAsync(cancellationToken))
                    .GetText(Encoding.UTF8, SourceHashAlgorithm.Sha1);

                if (content.CanBeEmbedded)
                {
                    yield return EmbeddedText.FromSource(syntaxTree.FilePath, content);
                }
            }
        }

        private void PackNuGet(List<YardarmCompilationResult> results)
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
                result.ReferenceAssemblyOutput?.Seek(0, SeekOrigin.Begin);

                if (_settings.NuGetSymbolsOutput != null)
                {
                    if (!result.PdbOutput.CanRead || !result.PdbOutput.CanSeek)
                    {
                        throw new InvalidOperationException(
                            $"{nameof(YardarmGenerationSettings.PdbOutput)} must be seekable and readable to pack a NuGet symbols package.");
                    }

                    result.PdbOutput.Seek(0, SeekOrigin.Begin);
                }
            }


            var packer = _serviceProvider.GetRequiredService<NuGetPacker>();

            packer.Pack(results, _settings.NuGetOutput!);

            if (_settings.NuGetSymbolsOutput != null)
            {
                if (!_settings.PdbOutput.CanRead || !_settings.PdbOutput.CanSeek)
                {
                    throw new InvalidOperationException(
                        $"{nameof(YardarmGenerationSettings.PdbOutput)} must be seekable and readable to pack a NuGet symbols package.");
                }



                packer.PackSymbols(results, _settings.NuGetSymbolsOutput);
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
