using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

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

            var context = new GenerationContext(document);

            var serviceProvider = settings.BuildServiceProvider(context);

            var syntaxTrees = serviceProvider.GetRequiredService<IEnumerable<ISyntaxTreeGenerator>>()
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

            return compilation.Emit(settings.DllOutput,
                pdbStream: settings.PdbOutput,
                options: new EmitOptions()
                    .WithDebugInformationFormat(DebugInformationFormat.PortablePdb));
        }
    }
}
