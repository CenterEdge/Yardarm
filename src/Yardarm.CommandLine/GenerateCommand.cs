using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using NuGet.Configuration;
using NuGet.Packaging.Core;
using Serilog;
using Serilog.Events;
using Yardarm.Helpers;
using Yardarm.Spec;

namespace Yardarm.CommandLine
{
    public class GenerateCommand
    {
        private readonly GenerateOptions _options;

        public GenerateCommand(GenerateOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public async Task<int> ExecuteAsync()
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var document = await ReadDocumentAsync();

            var generator = new YardarmGenerator();

            var settings = new YardarmGenerationSettings(_options.AssemblyName);

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

                YardarmGenerationResult generationResult = await generator.EmitAsync(document, settings);

                foreach (Diagnostic diagnostic in generationResult.CompilationResult.Diagnostics
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
            catch (Exception ex)
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

        private async Task<OpenApiDocument> ReadDocumentAsync()
        {
            var reader = new OpenApiStreamReader();

            await using var stream = File.OpenRead(_options.InputFile);

            return reader.Read(stream, out _);
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

        private void ApplyExtensions(YardarmGenerationSettings settings)
        {
            foreach (string extensionFile in _options.ExtensionFiles)
            {
                try
                {
                    if (extensionFile.EndsWith(".dll"))
                    {
                        // Appears to be a file reference

                        string fullPath = !Path.IsPathFullyQualified(extensionFile)
                            ? Path.Combine(Directory.GetCurrentDirectory(), extensionFile)
                            : extensionFile;

                        Assembly assembly = Assembly.LoadFile(fullPath);

                        settings.AddExtension(assembly);
                    }
                    else
                    {
                        // Appears to be an assembly name

                        Assembly assembly = Assembly.Load(extensionFile);

                        settings.AddExtension(assembly);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error loading extension '{extensionFile}'.", ex);
                }
            }
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
                    }
                    else
                    {
                        var directory = Path.GetDirectoryName(_options.OutputPackageFile) ??
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

                    var dllStream = File.Create(_options.OutputFile);
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

        private void ApplyNuGetSettings(YardarmGenerationSettings settings)
        {
            if (!string.IsNullOrEmpty(_options.RepositoryType) && !string.IsNullOrEmpty(_options.RepositoryUrl))
            {
                settings.Repository =
                    new RepositoryMetadata(_options.RepositoryType, _options.RepositoryUrl, _options.RepositoryBranch,
                        _options.RepositoryCommit);
            }
        }
    }
}
