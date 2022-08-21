using System;
using CommandLine;

namespace Yardarm.CommandLine
{
    /// <summary>
    /// This command is used to generate a list of package references and downloads for a given target framework.
    /// It may be used by MSBuild as part of the CollectPackageReferences extension point.
    /// </summary>
    [Verb("collect-dependencies", HelpText = "Collect NuGet dependencies to include in MSBuild")]
    public class CollectDependenciesOptions : CommonOptions
    {
    }
}
