using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm
{
    public class YardarmGenerationSettings
    {
        private Stream? _dllOutput;
        private Stream? _pdbOutput;

        public string AssemblyName { get; set; } = "Yardarm.Sdk";
        public string RootNamespace { get; set; } = "Yardarm.Sdk";
        public Version Version { get; set; } = new Version(1, 0, 0);
        public string? VersionSuffix { get; set; }
        public string Author { get; set; } = "anonymous";

        public Stream DllOutput
        {
            get => _dllOutput ??= new MemoryStream();
            set => _dllOutput = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Stream PdbOutput
        {
            get => _pdbOutput ??= new MemoryStream();
            set => _pdbOutput = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Stream? NuGetOutput { get; set; }

        public List<Func<IServiceCollection, IServiceCollection>> Extensions { get; } =
            new List<Func<IServiceCollection, IServiceCollection>>();

        public CSharpCompilationOptions CompilationOptions { get; set; } =
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithDeterministic(true)
                .WithOptimizationLevel(OptimizationLevel.Release)
                .WithNullableContextOptions(NullableContextOptions.Enable)
                .WithOverflowChecks(false)
                .WithPlatform(Platform.AnyCpu)
                .WithConcurrentBuild(true)
                .WithAssemblyIdentityComparer(DesktopAssemblyIdentityComparer.Default);

        public YardarmGenerationSettings()
        {
        }

        public YardarmGenerationSettings(string assemblyName)
        {
            AssemblyName = assemblyName;
            RootNamespace = assemblyName;
        }

        public IServiceProvider BuildServiceProvider(OpenApiDocument document)
        {
            IServiceCollection services = new ServiceCollection();

            services = Extensions.Aggregate(services, (p, extension) => extension.Invoke(p));

            services.AddYardarm(this, document);

            return services.BuildServiceProvider();
        }

        public YardarmGenerationSettings AddExtension(Func<IServiceCollection, IServiceCollection> extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            Extensions.Add(extension);

            return this;
        }
    }
}
