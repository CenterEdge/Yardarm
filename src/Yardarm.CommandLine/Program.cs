﻿using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Readers;
using Yardarm.NewtonsoftJson;

namespace Yardarm.CommandLine
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var reader = new OpenApiStreamReader();

            await using var stream = File.OpenRead("centeredge-cardsystemapi.yaml");

            var document = reader.Read(stream, out _);

            var generator = new YardarmGenerator();

            await using var dllStream = File.Create("test2.dll");
            await using var pdbStream = File.Create("test2.pdb");
            // await using var nugetStream = File.Create("test.nupkg");

            var settings = new YardarmGenerationSettings("Test")
            {
                DllOutput = dllStream,
                PdbOutput = pdbStream,
                // NuGetOutput = nugetStream,
                VersionSuffix = "-beta001"
            }
                .AddExtension(services => services.AddLogging(builder =>
                {
                    builder
                        .SetMinimumLevel(LogLevel.Information)
                        .AddConsole();
                }))
                .AddNewtonsoftJson();

            var compilationResult = await generator.EmitAsync(document, settings);

            foreach (var diagnostic in compilationResult.Diagnostics.Where(p => p.Severity == DiagnosticSeverity.Error))
            {
                Console.WriteLine(diagnostic);
            }

            Console.WriteLine("Success: {0}", compilationResult.Success);
        }
    }
}
