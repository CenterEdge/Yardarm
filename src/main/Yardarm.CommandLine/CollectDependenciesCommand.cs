using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.LibraryModel;
using Serilog;
using Yardarm.CommandLine.Interop;

namespace Yardarm.CommandLine
{
    public class CollectDependenciesCommand : CommonCommand
    {
        private readonly CollectDependenciesOptions _options;

        public CollectDependenciesCommand(CollectDependenciesOptions options) : base(options)
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
                    AddItem(GeneratePackageReference(dependency));
                }

                foreach (var framework in packageSpec.TargetFrameworks)
                {
                    foreach (var dependency in framework.Dependencies)
                    {
                        var item = GeneratePackageReference(dependency);
                        item.TargetFramework = framework.FrameworkName.GetShortFolderName();

                        AddItem(item);
                    }

                    foreach (var download in framework.DownloadDependencies)
                    {
                        if (download.Name == "Microsoft.NETCore.App.Ref")
                        {
                            // Don't add PackageDownload for .NET Core, this is added automatically by the ProcessFrameworkReferences
                            // target in MSBuild and we don't want a duplicate. This also has the advantage that MSBuild will automatically
                            // pull the latest known version.
                            continue;
                        }

                        var item = new AddItemDto
                        {
                            ItemType = "PackageDownload",
                            TargetFramework = framework.FrameworkName.GetShortFolderName(),
                            Identity = download.Name,
                            Metadata = new Dictionary<string, string>
                            {
                                ["Version"] = download.VersionRange.ToNormalizedString()
                            }
                        };

                        AddItem(item);
                    }

                    foreach (var reference in framework.FrameworkReferences)
                    {
                        var item = new AddItemDto
                        {
                            ItemType = "FrameworkReference",
                            TargetFramework = framework.FrameworkName.GetShortFolderName(),
                            Identity = reference.Name,
                            Metadata = new Dictionary<string, string>
                            {
                                ["PrivateAssets"] = reference.PrivateAssets == FrameworkDependencyFlags.All ? "All" : "None"
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

        private static AddItemDto GeneratePackageReference(LibraryDependency dependency)
        {
            var item = new AddItemDto
            {
                ItemType = "PackageReference",
                Identity = dependency.Name,
                Metadata = new Dictionary<string, string>
                {
                    ["Version"] = dependency.LibraryRange.VersionRange.ToNormalizedString()
                }
            };

            if (dependency.IncludeType != LibraryIncludeFlags.All)
            {
                item.Metadata["IncludeAssets"] = FormatIncludeFlags(dependency.IncludeType);
            }
            if (dependency.SuppressParent != LibraryIncludeFlags.None)
            {
                item.Metadata["PrivateAssets"] = FormatIncludeFlags(dependency.SuppressParent);
            }

            return item;
        }

        private static string FormatIncludeFlags(LibraryIncludeFlags flags)
        {
            if (flags == LibraryIncludeFlags.None)
            {
                return "None";
            }

            if (flags == LibraryIncludeFlags.All)
            {
                return "All";
            }

            bool first = true;
            var builder = new StringBuilder();
            for (var i = LibraryIncludeFlags.Runtime;
                 i <= LibraryIncludeFlags.BuildTransitive;
                 i = (LibraryIncludeFlags)((int)i * 2))
            {
                if (flags.HasFlag(i))
                {
                    if (first)
                    {
                        first = false;
                    }
                    else
                    {
                        builder.Append(';');
                    }

                    builder.Append(i.ToString());
                }
            }

            return builder.ToString();
        }
    }
}
