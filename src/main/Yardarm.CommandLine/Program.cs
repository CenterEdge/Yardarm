using System;
using System.Threading;
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

var cts = new CancellationTokenSource();

int exitCode;
bool completedGracefully = false;

AppDomain.CurrentDomain.ProcessExit += (_, _) =>
{
    // ReSharper disable once AccessToModifiedClosure
    if (!completedGracefully)
    {
        Cancel();
    }
};
Console.CancelKeyPress += (_, e) =>
{
    Cancel();
    // Don't terminate immediately, wait for cancellation to propagate
    e.Cancel = true;
};

try
{
    exitCode = await Parser.Default
        .ParseArguments<GenerateOptions, RestoreOptions, CollectDependenciesOptions>(args)
        .MapResult(
            (GenerateOptions options) => new GenerateCommand(options).ExecuteAsync(cts.Token),
            (RestoreOptions options) => new RestoreCommand(options).ExecuteAsync(cts.Token),
            (CollectDependenciesOptions options) => new CollectDependenciesCommand(options).ExecuteAsync(cts.Token),
            errs => Task.FromResult(1));

    completedGracefully = true;
}
catch (OperationCanceledException)
{
    exitCode = 2;
}

Log.CloseAndFlush();

return exitCode;

void Cancel()
{
    if (!cts.IsCancellationRequested)
    {
        Console.WriteLine("Cancelling...");
        cts.Cancel();
    }
}
