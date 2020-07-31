using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

        public List<Func<IServiceCollection, IServiceCollection>> Extensions { get; } =
            new List<Func<IServiceCollection, IServiceCollection>>();

        public CSharpCompilationOptions CompilationOptions { get; set; } =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithNullableContextOptions(NullableContextOptions.Enable);

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
            IServiceCollection services = new ServiceCollection();

            services = Extensions.Aggregate(services, (p, extension) => extension.Invoke(p));

            services.AddYardarm(this, generationContext);

            return services.BuildServiceProvider();
        }
    }
}
