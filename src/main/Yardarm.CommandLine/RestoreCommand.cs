using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Yardarm.Packaging;

namespace Yardarm.CommandLine
{
    public class RestoreCommand : CommonCommand
    {
        private readonly RestoreOptions _options;

        public RestoreCommand(RestoreOptions options) : base(options)
        {
            _options = options;
        }

        public async Task<int> ExecuteAsync(CancellationToken cancellationToken)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            var settings = new YardarmGenerationSettings(_options.AssemblyName)
            {
                RootNamespace = _options.RootNamespace ?? _options.AssemblyName,
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

                var generator = new YardarmProcessor(settings);
                await generator.RestoreAsync(cancellationToken);

                stopwatch.Stop();
                Log.Information("Restore complete in {0:f3}s", stopwatch.Elapsed.TotalSeconds);

                return 0;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                Log.Logger.Error(ex, "Error restoring SDK");
                return 1;
            }
        }
    }
}
