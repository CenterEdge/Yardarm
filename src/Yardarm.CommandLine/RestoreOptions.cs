using System;
using CommandLine;

namespace Yardarm.CommandLine
{
    [Verb("restore", HelpText = "Restore NuGet packages prior to generating an assembly")]
    public class RestoreOptions : CommonOptions
    {
    }
}
