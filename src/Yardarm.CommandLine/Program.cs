using System;
using System.Threading.Tasks;
using CommandLine;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;
using Yardarm.CommandLine;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console(
        theme: AnsiConsoleTheme.Code,
        outputTemplate: "[{Level:u3}] {Message:lj}{NewLine}{Exception}",
        standardErrorFromLevel: LogEventLevel.Error)
    .CreateLogger();

int exitCode = await Parser.Default
    .ParseArguments<GenerateOptions, RestoreOptions>(args)
    .MapResult(
        (GenerateOptions options) => new GenerateCommand(options).ExecuteAsync(),
        (RestoreOptions options) => new RestoreCommand(options).ExecuteAsync(),
        errs => Task.FromResult(1));

Log.CloseAndFlush();

return exitCode;
