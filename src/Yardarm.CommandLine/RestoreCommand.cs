using System;
using System.Diagnostics;
using System.Linq;
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

        public async Task<int> ExecuteAsync()
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
                await generator.RestoreAsync();

                stopwatch.Stop();
                Log.Information("Restore complete in {0:f3}s", stopwatch.Elapsed.TotalSeconds);

                return 0;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Error restoring SDK");
                return 1;
            }
        }
    }
}
