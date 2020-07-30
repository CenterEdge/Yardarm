using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        private readonly IServiceProvider _serviceProvider;

        internal YardarmGenerator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public EmitResult Emit(OpenApiDocument document, string dllFileName, string? pdbFileName = null)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            if (dllFileName == null)
            {
                throw new ArgumentNullException(nameof(dllFileName));
            }

            using var dllFile = File.Create(dllFileName);
            using var pdbFile = pdbFileName != null ? File.Create(pdbFileName) : null;

            return Emit(document, dllFile, pdbFile);
        }

        public EmitResult Emit(OpenApiDocument document, Stream dllStream, Stream? pdbStream = null)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }
            if (dllStream == null)
            {
                throw new ArgumentNullException(nameof(dllStream));
            }

            using var scope = _serviceProvider.CreateScope();
            var context = _serviceProvider.GetRequiredService<GenerationContext>();
            context.Document = document;

            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Debug)
                .WithGeneralDiagnosticOption(ReportDiagnostic.Error);

            var syntaxTrees = _serviceProvider.GetRequiredService<IEnumerable<ISyntaxTreeGenerator>>()
                .SelectMany(p => p.Generate())
                .ToArray();

            var compilation = CSharpCompilation.Create("TestOutput")
                .WithOptions(compilationOptions)
                .AddReferences(_serviceProvider.GetRequiredService<IEnumerable<IReferenceGenerator>>()
                    .SelectMany(p => p.Generate())
                    .Distinct())
                .AddSyntaxTrees(syntaxTrees);

            return compilation.Emit(dllStream,
                pdbStream: pdbStream,
                options: new EmitOptions()
                    .WithDebugInformationFormat(DebugInformationFormat.PortablePdb));
        }

        #region static

        public static YardarmGenerator Create() => Create(null);

        public static YardarmGenerator Create(Action<IServiceCollection>? registerServices)
        {
            var collection = new ServiceCollection();

            registerServices?.Invoke(collection);

            collection.AddYardarm();

            return new YardarmGenerator(collection.BuildServiceProvider());
        }

        #endregion
    }
}
