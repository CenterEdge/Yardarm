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

        [Option('f', "frameworks", HelpText = "List of target framework monikers. Must be a single item unless outputting a NuGet package.")]
        public IEnumerable<string> TargetFrameworks { get; set; }

        [Option("keyfile", HelpText = "Key file to create a strongly-named assembly")]
        public string KeyFile { get; set; }

        #region DLL

        [Option('o', "output", HelpText = "Output directory or .dll file. Indicate a directory with a trailing slash.", SetName = "dll")]
        public string OutputFile { get; set; }

        [Option("xml", HelpText = "Output .xml documentation file", SetName = "dll")]
        public string OutputXmlFile { get; set; }

        [Option("no-xml", HelpText = "Suppress output of .xml documentation", SetName = "dll")]
        public bool NoXmlFile { get; set; }

        [Option("pdb", HelpText = "Output .pdb debug symbols files", SetName = "dll")]
        public string OutputDebugSymbols { get; set; }

        [Option("no-pdb", HelpText = "Suppress output of .pdb debug symbols", SetName = "dll")]
        public bool NoDebugSymbols { get; set; }

        #endregion

        #region NuGet

        [Option("nupkg", HelpText = "Output directory or .nupkg package file. Indicate a directory with a trailing slash.", SetName = "nuget")]
        public string OutputPackageFile { get; set; }

        [Option("snupkg", HelpText = "Output .snupkg symbols package file", SetName = "nuget")]
        public string OutputSymbolsPackageFile { get; set; }

        [Option("no-snupkg", HelpText = "Suppress output of .snupkg symbols package file", SetName = "nuget")]
        public bool NoSymbolsPackageFile { get; set; }

        [Option("repository-type", Default="git", HelpText = "Type of repository for NuGet packaging")]
        public string RepositoryType { get; set; }

        [Option("repository-url", HelpText = "Url of repository for NuGet packaging (ex. \"https://github.com/CenterEdge/Yardarm.git\")")]
        public string RepositoryUrl { get; set; }

        [Option("repository-branch", HelpText = "Branch of repository for NuGet packaging (ex. \"main\")")]
        public string RepositoryBranch { get; set; }

        [Option("repository-commit", HelpText = "Commit identifier for NuGet packaging")]
        public string RepositoryCommit { get; set; }

        #endregion

        [Option('x', "extension", HelpText = "Extension assemblies to enable")]
        public IEnumerable<string> ExtensionFiles { get; set; }
    }
}
