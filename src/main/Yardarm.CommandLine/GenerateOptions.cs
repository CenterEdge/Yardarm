using System;
using System.Collections.Generic;
using CommandLine;

namespace Yardarm.CommandLine
{
    [Verb("generate", HelpText = "Generate an assembly")]
    public class GenerateOptions : CommonOptions
    {
        [Option('v', "version", Default = "1.0.0", HelpText = "Generated assembly version")]
        public string Version { get; set; }

        [Option("keyfile", HelpText = "Key file to create a strongly-named assembly")]
        public string KeyFile { get; set; }

        [Option("keycontainername", HelpText = "Key container to create a strongly-named assembly")]
        public string KeyContainerName { get; set; }

        [Option("public-sign", HelpText = "Sign the strongly-named assembly with the public key only")]
        public bool PublicSign { get; set; }

        [Option("embed", HelpText = "Embed source files with debug symbols")]
        public bool EmbedAllSources { get; set; }

        [Option("no-restore", HelpText = "Use existing restore lock file from the intermediate directory.")]
        public bool NoRestore { get; set; }

        [Option("references", HelpText = "Referenced assemblies to use, rather than calculated references. Typically only supplied by MSBuild. Should be a full path.")]
        public IEnumerable<string> References { get; set; }

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

        [Option("ref", HelpText = "Output reference assembly file", SetName = "dll")]
        public string OutputReferenceAssembly { get; set; }

        [Option("no-ref", HelpText = "Suppress output of the reference assembly file", SetName = "dll")]
        public bool NoReferenceAssembly { get; set; }

        [Option("delay-sign", HelpText = "Delay signing of a strongly-named assembly", SetName = "dll")]
        public bool DelaySign { get; set; }

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
    }
}
