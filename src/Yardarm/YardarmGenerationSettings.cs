using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;

namespace Yardarm
{
    public class YardarmGenerationSettings
    {
        private Stream? _dllOutput;
        private Stream? _pdbOutput;
        private Stream? _xmlDocumentationOutput;

        private readonly List<YardarmExtension> _extensions = new List<YardarmExtension>();
        private readonly List<Action<ILoggingBuilder>> _loggingBuilders = new List<Action<ILoggingBuilder>>();

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

        public Stream XmlDocumentationOutput
        {
            get => _xmlDocumentationOutput ??= new MemoryStream();
            set => _xmlDocumentationOutput = value ?? throw new ArgumentNullException(nameof(value));
        }

        public Stream? NuGetOutput { get; set; }

        public Stream? NuGetSymbolsOutput { get; set; }

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
            IServiceCollection services = new ServiceCollection()
                .AddLogging(builder =>
                {
                    foreach (var configuredBuilder in _loggingBuilders)
                    {
                        configuredBuilder(builder);
                    }
                });

            services = _extensions.Aggregate(services, (p, extension) => extension.ConfigureServices(p));

            services.AddYardarm(this, document);

            return services.BuildServiceProvider();
        }

        public YardarmGenerationSettings AddExtension(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (!typeof(YardarmExtension).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type {type.FullName} must inherit from YardarmExtension.");
            }

            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                throw new ArgumentException($"Type {type.FullName} must have a default constructor.");
            }

            _extensions.Add((YardarmExtension) constructor.Invoke(null));

            return this;
        }

        public YardarmGenerationSettings AddExtension<T>()
            where T : YardarmExtension =>
            AddExtension(typeof(T));

        public YardarmGenerationSettings AddExtension(Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes()
                .Where(p => p.IsClass && !p.IsAbstract && typeof(YardarmExtension).IsAssignableFrom(p)))
            {
                AddExtension(type);
            }

            return this;
        }

        public YardarmGenerationSettings AddLogging(Action<ILoggingBuilder> buildAction)
        {
            if (buildAction == null)
            {
                throw new ArgumentNullException(nameof(buildAction));
            }

            _loggingBuilders.Add(buildAction);

            return this;
        }
    }
}
