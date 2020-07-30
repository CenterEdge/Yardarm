using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Readers;

namespace Yardarm.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            var reader = new OpenApiStreamReader();

            using var stream = File.OpenRead("centeredge-cardsystemapi.yaml");

            var document = reader.Read(stream, out _);

            var generator = YardarmGenerator.Create();

            using var dllStream = File.OpenWrite("test.dll");
            using var pdbStream = File.OpenWrite("test.pdb");
            var compilationResult = generator.Emit(document, dllStream, pdbStream);

            foreach (var diagnostic in compilationResult.Diagnostics.Where(p => p.Severity == DiagnosticSeverity.Error))
            {
                Console.WriteLine(diagnostic);
            }

            Console.WriteLine("Success: {0}", compilationResult.Success);
        }
    }
}
