using System;
using CommandLine;

namespace Yardarm.CommandLine
{
    /// <summary>
    /// This command is used to generate a list of package references for a given target framework. It may be used
    /// by MSBuild as part of the CollectPackageReferences extension point.
    /// </summary>
    [Verb("collect-package-references", HelpText = "Collect NuGet package references to include in MSBuild")]
    public class CollectPackageReferencesOptions : CommonOptions
    {
    }
}
