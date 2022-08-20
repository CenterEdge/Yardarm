using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Yardarm.CommandLine.Interop;

namespace Yardarm.CommandLine
{
    public class CollectPackageReferencesCommand : CommonCommand
    {
        private readonly CollectPackageReferencesOptions _options;

        public CollectPackageReferencesCommand(CollectPackageReferencesOptions options) : base(options)
        {
            _options = options;
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var document = await ReadDocumentAsync();

            var settings = new YardarmGenerationSettings(_options.AssemblyName)
            {
                IntermediateOutputPath = _options.IntermediateOutputPath,
            };

            try
            {
                ApplyExtensions(settings);

                ApplyNuGetSettings(settings);

                settings
                    .AddLogging(builder =>
                    {
                        builder
                            .SetMinimumLevel(LogLevel.Information)
                            .AddSerilog();
                    });

                var generator = new YardarmGenerator(document, settings);
                var packageSpec = await generator.GetPackageSpecAsync(cancellationToken);

                foreach (var dependency in packageSpec.Dependencies)
                {
                    var item = new AddItemDto
                    {
                        ItemType = "PackageReference",
                        Identity = dependency.Name,
                        Metadata = new Dictionary<string, string>()
                        {
                            ["Version"] = dependency.LibraryRange.VersionRange.OriginalString
                        }
                    };

                    AddItem(item);
                }

                foreach (var framework in packageSpec.TargetFrameworks)
                {
                    foreach (var dependency in framework.Dependencies.Where(p => !p.AutoReferenced))
                    {
                        var item = new AddItemDto
                        {
                            ItemType = "PackageReference",
                            TargetFramework = framework.FrameworkName.GetShortFolderName(),
                            Identity = dependency.Name,
                            Metadata = new Dictionary<string, string>()
                            {
                                ["Version"] = dependency.LibraryRange.VersionRange.OriginalString
                            }
                        };

                        AddItem(item);
                    }
                }

                stopwatch.Stop();
                Log.Information("Collect package references complete in {0:f3}s", stopwatch.Elapsed.TotalSeconds);

                return 0;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Logger.Error(ex, "Error collecting package references");
                return 1;
            }
        }

        private static void AddItem(AddItemDto item)
        {
            Console.WriteLine($"AddItem: {JsonSerializer.Serialize(item, DtoSerializerContext.Default.AddItemDto)}");
        }
    }
}
