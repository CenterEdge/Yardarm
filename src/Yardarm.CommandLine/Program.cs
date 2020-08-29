using System;
using System.Threading.Tasks;
using CommandLine;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Yardarm.CommandLine
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console(
                    theme: AnsiConsoleTheme.Code,
                    outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
                    standardErrorFromLevel: LogEventLevel.Error)
                .CreateLogger();

            await Parser.Default.ParseArguments<GenerateOptions, object>(args)
                .WithParsedAsync<GenerateOptions>(options => new GenerateCommand(options).ExecuteAsync());

            Log.CloseAndFlush();

            return Environment.ExitCode;
        }
    }
}
