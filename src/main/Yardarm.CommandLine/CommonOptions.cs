using System;
using System.Collections.Generic;
using CommandLine;

namespace Yardarm.CommandLine
{
    public class CommonOptions
    {
        [Option('n', "name", Required = true, HelpText = "Generated assembly name")]
        public string AssemblyName { get; set; }

        [Option("root-namespace", HelpText = "Root namespace of the generated assembly")]
        public string RootNamespace { get; set; }

        [Option('f', "frameworks", HelpText = "List of target framework monikers. Must be a single item unless outputting a NuGet package.")]
        public IEnumerable<string> TargetFrameworks { get; set; }

        [Option('x', "extension", HelpText = "Extension assemblies to enable")]
        public IEnumerable<string> ExtensionFiles { get; set; }

        [Option("intermediate-dir", HelpText = "Intermediate output directory")]
        public string IntermediateOutputPath { get; set; }
    }
}
