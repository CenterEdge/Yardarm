using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Yardarm.Generation;

namespace Yardarm
{
    public class YardarmGenerationSettings
    {
        private Stream? _dllOutput;

        public string AssemblyName { get; set; } = "Yardarm.Sdk";
        public string RootNamespace { get; set; } = "Yardarm.Sdk";

        public Stream DllOutput
        {
            get => _dllOutput ??= new MemoryStream();
            set => _dllOutput = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Stream? PdbOutput { get; set; }

        public List<Action<IServiceCollection>> Extensions { get; } = new List<Action<IServiceCollection>>();

        public CSharpCompilationOptions CompilationOptions { get; set; } = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Release);

        public YardarmGenerationSettings()
        {
        }

        public YardarmGenerationSettings(string assemblyName)
        {
            AssemblyName = assemblyName;
            RootNamespace = assemblyName;
        }

        public IServiceProvider BuildServiceProvider(GenerationContext generationContext)
        {
            var services = new ServiceCollection();

            foreach (var extension in Extensions)
            {
                extension.Invoke(services);
            }

            services.AddYardarm(this, generationContext);

            return services.BuildServiceProvider();
        }
    }
}
