using System;
using System.Threading.Tasks;
using CommandLine;

namespace Yardarm.CommandLine
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            await Parser.Default.ParseArguments<GenerateOptions, object>(args)
                .WithParsedAsync<GenerateOptions>(options => new GenerateCommand(options).ExecuteAsync());

            return Environment.ExitCode;
        }
    }
}
