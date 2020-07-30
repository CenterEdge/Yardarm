using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm
{
    public class YardarmGenerator
    {
        public EmitResult Emit(OpenApiDocument document, YardarmGenerationSettings settings)
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

            var compilation = CSharpCompilation.Create(settings.AssemblyName)
                .WithOptions(settings.CompilationOptions)
                .AddReferences(serviceProvider.GetRequiredService<IEnumerable<IReferenceGenerator>>()
                    .SelectMany(p => p.Generate())
                    .Distinct())
                .AddSyntaxTrees(syntaxTrees);

            return compilation.Emit(settings.DllOutput,
                pdbStream: settings.PdbOutput,
                options: new EmitOptions()
                    .WithDebugInformationFormat(DebugInformationFormat.PortablePdb));
        }
    }
}
