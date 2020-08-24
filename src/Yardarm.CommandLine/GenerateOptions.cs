using System;
using System.Collections.Generic;
using System.Text;
using CommandLine;

namespace Yardarm.CommandLine
{
    [Verb("generate", HelpText = "Generate an assembly")]
    public class GenerateOptions
    {
        [Option('i', "input", Required = true, HelpText = "OpenAPI 3 files to process")]
        public string InputFile { get; set; }

        [Option('n', "name", Required = true, HelpText = "Generated assembly name")]
        public string AssemblyName { get; set; }

        [Option('v', "version", Default = "1.0.0", HelpText = "Generated assembly version")]
        public string Version { get; set; }

        [Option("keyfile", HelpText = "Key file to create a strongly-named assembly")]
        public string KeyFile { get; set; }

        #region DLL

        [Option('o', "output", HelpText = "Output .dll file", SetName = "dll")]
        public string OutputFile { get; set; }

        [Option("xml", HelpText = "Output .xml documentation file", SetName = "dll")]
        public string OutputXmlFile { get; set; }

        [Option("pdb", HelpText = "Output .pdb debug symbols files", SetName = "dll")]
        public string OutputDebugSymbols { get; set; }

        #endregion

        #region NuGet

        [Option("nupkg", HelpText = "Output .nupkg package file", SetName = "nuget")]
        public string OutputPackageFile { get; set; }

        [Option("snupkg", HelpText = "Output .snupkg symbols package file", SetName = "nuget")]
        public string OutputSymbolsPackageFile { get; set; }

        #endregion

        [Option('x', "extension", HelpText = "Extension assemblies to enable")]
        public IEnumerable<string> ExtensionFiles { get; set; }
    }
}
