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
    /// <summary>
    /// Generator creates an assembly from an OpenApiDocument.
    /// </summary>
    public class YardarmGenerator : YardarmProcessor
    {
        private readonly OpenApiDocument _document;

        public YardarmGenerator(OpenApiDocument document, YardarmGenerationSettings settings)
            : base(settings, settings.BuildServiceProvider(document))
        {
            ArgumentNullException.ThrowIfNull(document);

            _document = document;
        }

        public async Task<YardarmGenerationResult> EmitAsync(CancellationToken cancellationToken = default)
        {
            var toDispose = new List<IDisposable>();
            try
            {
                var context = ServiceProvider.GetRequiredService<GenerationContext>();

                await PerformRestoreAsync(context, Settings.NoRestore, cancellationToken);

                var compilationResults = new List<YardarmCompilationResult>();

                if (Settings.TargetFrameworkMonikers.Length == 1)
                {
                    var targetFramework = NuGetFramework.Parse(Settings.TargetFrameworkMonikers[0]);

                    // Perform the compilation
                    var (emitResult, additionalDiagnostics) = await BuildForTargetFrameworkAsync(
                        context, targetFramework,
                        Settings.DllOutput, Settings.PdbOutput, Settings.XmlDocumentationOutput, Settings.ReferenceAssemblyOutput,
                        cancellationToken).ConfigureAwait(false);

                    compilationResults.Add(new(targetFramework, emitResult,
                        Settings.DllOutput, Settings.PdbOutput, Settings.XmlDocumentationOutput, Settings.ReferenceAssemblyOutput,
                        additionalDiagnostics));
                }
                else
                {
                    if (Settings.NuGetOutput is null)
                    {
                        throw new InvalidOperationException(
                            "Targeting multiple frameworks is only supported with NuGet output.");
                    }

                    foreach (var targetFramework in Settings.TargetFrameworkMonikers.Select(NuGetFramework.Parse))
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
                    if (Settings.NuGetOutput != null)
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
            var compilation = CSharpCompilation.Create(Settings.AssemblyName)
                .WithOptions(Settings.CompilationOptions);

            // Run all enrichers against the compilation
            var enrichers = context.GenerationServices.GetRequiredService<IEnumerable<ICompilationEnricher>>();
            compilation = await compilation.EnrichAsync(enrichers, cancellationToken);

            ImmutableArray<Diagnostic> additionalDiagnostics;
            using (var sourceGeneratorLoadContext = new SourceGeneratorLoadContext(context.NuGetRestoreInfo!.Providers))
            {
                var sourceGenerators = sourceGeneratorLoadContext
                    .GetSourceGenerators(context.GenerationServices.GetRequiredService<PackageSpec>(),
                        context.NuGetRestoreInfo!.LockFile, targetFramework)
                    .ToList();

                // Execute the source generators
                compilation = ExecuteSourceGenerators(compilation,
                    sourceGenerators,
                    out additionalDiagnostics,
                    cancellationToken);
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
            if (!Settings.EmbedAllSources)
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

                if (Settings.NuGetSymbolsOutput != null)
                {
                    if (!result.PdbOutput.CanRead || !result.PdbOutput.CanSeek)
                    {
                        throw new InvalidOperationException(
                            $"{nameof(YardarmGenerationSettings.PdbOutput)} must be seekable and readable to pack a NuGet symbols package.");
                    }

                    result.PdbOutput.Seek(0, SeekOrigin.Begin);
                }
            }


            var packer = ServiceProvider.GetRequiredService<NuGetPacker>();

            packer.Pack(results, Settings.NuGetOutput!);

            if (Settings.NuGetSymbolsOutput != null)
            {
                if (!Settings.PdbOutput.CanRead || !Settings.PdbOutput.CanSeek)
                {
                    throw new InvalidOperationException(
                        $"{nameof(YardarmGenerationSettings.PdbOutput)} must be seekable and readable to pack a NuGet symbols package.");
                }



                packer.PackSymbols(results, Settings.NuGetSymbolsOutput);
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
