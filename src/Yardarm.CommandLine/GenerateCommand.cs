using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NuGet.Packaging.Core;
using Serilog;
using Serilog.Events;
using Yardarm.Helpers;
using Yardarm.Spec;

namespace Yardarm.CommandLine
{
    public class GenerateCommand : CommonCommand
    {
        private readonly GenerateOptions _options;

        public GenerateCommand(GenerateOptions options) : base(options)
        {
            _options = options;
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var document = await ReadDocumentAsync();

            string basePath = _options.InputFile;
            if (!Path.IsPathFullyQualified(basePath))
            {
                basePath = Path.Combine(AppContext.BaseDirectory, basePath);
            }

            var settings = new YardarmGenerationSettings(_options.AssemblyName)
            {
                BasePath = basePath,
                EmbedAllSources = _options.EmbedAllSources,
                IntermediateOutputPath = _options.IntermediateOutputPath,
                NoRestore = _options.NoRestore,
            };

            ApplyVersion(settings);

            ApplyExtensions(settings);

            ApplyStrongNaming(settings);

            ApplyNuGetSettings(settings);

            List<Stream> streams = ApplyFileStreams(settings);
            try
            {
                settings
                    .AddLogging(builder =>
                    {
                        builder
                            .SetMinimumLevel(LogLevel.Information)
                            .AddSerilog();
                    });

                var generator = new YardarmGenerator(document, settings);

                YardarmGenerationResult generationResult = await generator.EmitAsync(cancellationToken);

                foreach (Diagnostic diagnostic in generationResult.GetAllDiagnostics()
                             .Where(p => p.Severity >= DiagnosticSeverity.Info))
                {
                    Log.Logger.Write(
                        diagnostic.Severity switch
                        {
                            DiagnosticSeverity.Error => LogEventLevel.Error,
                            DiagnosticSeverity.Warning => LogEventLevel.Warning,
                            _ => LogEventLevel.Information
                        },
                        diagnostic.GetMessageWithSource(
                            generationResult.Context.GenerationServices.GetRequiredService<IOpenApiElementRegistry>())
                    );
                }

                stopwatch.Stop();
                if (generationResult.Success)
                {
                    Log.Information("Generation complete in {0:f3}s", stopwatch.Elapsed.TotalSeconds);
                }
                else
                {
                    Log.Error("Generation failed in {0:f3}s", stopwatch.Elapsed.TotalSeconds);
                }

                return generationResult.Success ? 0 : 1;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Logger.Error(ex, "Error generating SDK");
                return 1;
            }
            finally
            {
                foreach (var stream in streams)
                {
                    await stream.DisposeAsync();
                }
            }
        }

        private void ApplyVersion(YardarmGenerationSettings settings)
        {
            int dashIndex = _options.Version.IndexOf('-');

            string versionStr = dashIndex >= 0
                ? _options.Version.Substring(0, dashIndex)
                : _options.Version;

            settings.VersionSuffix = dashIndex >= 0
                ? _options.Version.Substring(dashIndex)
                : "";

            if (!Version.TryParse(versionStr, out Version version))
            {
                Environment.ExitCode = 1;
                throw new InvalidOperationException($"Invalid version {_options.Version}");
            }

            settings.Version = version;
        }

        private List<Stream> ApplyFileStreams(YardarmGenerationSettings settings)
        {
            var streams = new List<Stream>();
            try
            {
                if (!string.IsNullOrEmpty(_options.OutputFile))
                {
                    bool isDirectory = Path.EndsInDirectorySeparator(_options.OutputFile);
                    if (isDirectory)
                    {
                        string directory = _options.OutputFile;
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        _options.OutputFile = Path.Combine(directory, $"{_options.AssemblyName}.dll");

                        if (string.IsNullOrEmpty(_options.OutputXmlFile))
                        {
                            _options.OutputXmlFile = Path.Combine(directory, $"{_options.AssemblyName}.xml");
                        }

                        if (string.IsNullOrEmpty(_options.OutputDebugSymbols))
                        {
                            _options.OutputDebugSymbols = Path.Combine(directory, $"{_options.AssemblyName}.pdb");
                        }

                        if (!_options.NoReferenceAssembly && string.IsNullOrEmpty(_options.OutputReferenceAssembly))
                        {
                            string refDirectory = Path.Combine(directory, "ref");
                            if (!Directory.Exists(refDirectory))
                            {
                                Directory.CreateDirectory(refDirectory);
                            }

                            _options.OutputReferenceAssembly = Path.Combine(refDirectory,
                                $"{Path.GetFileNameWithoutExtension(_options.OutputFile)}.dll");
                        }
                    }
                    else
                    {
                        string directory = Path.GetDirectoryName(_options.OutputPackageFile) ??
                                           Directory.GetCurrentDirectory();

                        if (string.IsNullOrEmpty(_options.OutputXmlFile))
                        {
                            _options.OutputXmlFile = Path.Combine(directory,
                                $"{Path.GetFileNameWithoutExtension(_options.OutputFile)}.xml");
                        }

                        if (string.IsNullOrEmpty(_options.OutputDebugSymbols))
                        {
                            _options.OutputDebugSymbols = Path.Combine(directory,
                                $"{Path.GetFileNameWithoutExtension(_options.OutputFile)}.pdb");
                        }
                    }

                    var dllStream = File.Create(_options.OutputFile!);
                    streams.Add(dllStream);
                    settings.DllOutput = dllStream;

                    if (!_options.NoXmlFile && !string.IsNullOrEmpty(_options.OutputXmlFile))
                    {
                        var xmlStream = File.Create(_options.OutputXmlFile);
                        streams.Add(xmlStream);
                        settings.XmlDocumentationOutput = xmlStream;
                    }

                    if (!_options.NoDebugSymbols && !string.IsNullOrEmpty(_options.OutputDebugSymbols))
                    {
                        var pdbStream = File.Create(_options.OutputDebugSymbols);
                        streams.Add(pdbStream);
                        settings.PdbOutput = pdbStream;
                    }

                    if (!_options.NoReferenceAssembly && !string.IsNullOrEmpty(_options.OutputReferenceAssembly))
                    {
                        var referenceAssemblyStream = File.Create(_options.OutputReferenceAssembly);
                        streams.Add(referenceAssemblyStream);
                        settings.ReferenceAssemblyOutput = referenceAssemblyStream;
                    }
                }
                else if (!string.IsNullOrEmpty(_options.OutputPackageFile))
                {
                    bool isDirectory = Path.EndsInDirectorySeparator(_options.OutputPackageFile);
                    if (isDirectory)
                    {
                        string directory = _options.OutputPackageFile;
                        if (!Directory.Exists(directory))
                        {
                            Directory.CreateDirectory(directory);
                        }

                        _options.OutputPackageFile =
                            Path.Combine(directory, $"{_options.AssemblyName}.{_options.Version}.nupkg");

                        if (string.IsNullOrEmpty(_options.OutputSymbolsPackageFile))
                        {
                            _options.OutputSymbolsPackageFile = Path.Combine(directory,
                                $"{_options.AssemblyName}.{_options.Version}.snupkg");
                        }
                    }
                    else if (string.IsNullOrEmpty(_options.OutputSymbolsPackageFile))
                    {
                        _options.OutputSymbolsPackageFile = Path.Combine(
                            Path.GetDirectoryName(_options.OutputPackageFile) ?? Directory.GetCurrentDirectory(),
                            $"{Path.GetFileNameWithoutExtension(_options.OutputSymbolsPackageFile)}.snupkg");
                    }

                    var nupkgStream = File.Create(_options.OutputPackageFile);
                    streams.Add(nupkgStream);
                    settings.NuGetOutput = nupkgStream;

                    if (!_options.NoSymbolsPackageFile && !string.IsNullOrEmpty(_options.OutputSymbolsPackageFile))
                    {
                        var snupkgStream = File.Create(_options.OutputSymbolsPackageFile);
                        streams.Add(snupkgStream);
                        settings.NuGetSymbolsOutput = snupkgStream;
                    }
                }

                return streams;
            }
            catch
            {
                // Don't leave dangling streams on exception
                foreach (var stream in streams)
                {
                    stream.Dispose();
                }

                throw;
            }
        }

        private void ApplyStrongNaming(YardarmGenerationSettings settings)
        {
            if (string.IsNullOrEmpty(_options.KeyFile))
            {
                return;
            }

            settings.CompilationOptions = settings.CompilationOptions
                .WithStrongNameProvider(new DesktopStrongNameProvider(ImmutableArray.Create(Directory.GetCurrentDirectory())))
                .WithCryptoKeyFile(_options.KeyFile);
        }

        protected override void ApplyNuGetSettings(YardarmGenerationSettings settings)
        {
            base.ApplyNuGetSettings(settings);

            if (!string.IsNullOrEmpty(_options.RepositoryType) && !string.IsNullOrEmpty(_options.RepositoryUrl))
            {
                settings.Repository =
                    new RepositoryMetadata(_options.RepositoryType, _options.RepositoryUrl, _options.RepositoryBranch,
                        _options.RepositoryCommit);
            }
        }
    }
}
