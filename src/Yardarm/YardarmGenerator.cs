using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Generation;
using Yardarm.Packaging;
using Yardarm.Spec;

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

            var context = serviceProvider.GetRequiredService<GenerationContext>();

            await AddReferences(context, cancellationToken);

            AddSyntaxTrees(context, cancellationToken);

            var compilationResult = context.Compilation.Emit(settings.DllOutput,
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

        private async Task AddReferences(GenerationContext context, CancellationToken cancellationToken)
        {
            List<MetadataReference> references = await context.GenerationServices
                .GetRequiredService<IEnumerable<IReferenceGenerator>>()
                .ToAsyncEnumerable()
                .SelectMany(p => p.Generate(cancellationToken))
                .ToListAsync(cancellationToken);

            context.Compilation = context.Compilation
                .AddReferences(references);
        }

        private void AddSyntaxTrees(GenerationContext context, CancellationToken cancellationToken)
        {
            ISyntaxTreeGenerator[] syntaxTreeGenerators =
                context.GenerationServices.GetRequiredService<IEnumerable<ISyntaxTreeGenerator>>().ToArray();

            context.Compilation = context.Compilation
                .AddSyntaxTrees(syntaxTreeGenerators
                    .AsParallel()
                    .AsUnordered()
                    .WithCancellation(cancellationToken)
                    .SelectMany(p => p.Generate())
                    .ToArray());

            var enrichers = context.GenerationServices.GetRequiredService<IEnumerable<IOpenApiSyntaxNodeEnricher>>();
            foreach (var enricher in enrichers.OrderBy(p => p.Priority))
            {
                context.Compilation = context.Compilation.Enrich(enricher, context);
            }
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
