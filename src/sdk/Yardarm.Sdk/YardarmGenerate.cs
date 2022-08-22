using System;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;

namespace Yardarm.Build.Tasks;

public class YardarmGenerate : YardarmCommonTask
{
    protected override string Verb => "generate";

    public string? Version { get; set; }
    public bool EmbedAllSources { get; set; } = true;

    [Required]
    public string? OutputAssembly { get; set; }

    public string? OutputRefAssembly { get; set; }
    public string? OutputDebugSymbols { get; set; }
    public string? OutputXmlDocumentation { get; set; }

    public ITaskItem[]? ResolvedFrameworkReferences { get; set; }

    protected override bool ValidateParameters()
    {
        if (!base.ValidateParameters())
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(OutputAssembly))
        {
            Log.LogError("Must supply an OutputAssembly.");
            return false;
        }

        return true;
    }

    protected override void AppendAdditionalArguments(StringBuilder builder)
    {
        builder.AppendFormat(" --no-restore"); // Restore is performed by MSBuild

        if (!string.IsNullOrEmpty(Version))
        {
            builder.AppendFormat(" -v {0}", Version);
        }

        if (EmbedAllSources)
        {
            builder.AppendFormat(" --embed");
        }

        builder.AppendFormat(" -o {0}", OutputAssembly);
        if (!string.IsNullOrEmpty(OutputRefAssembly))
        {
            builder.AppendFormat(" --ref {0}", OutputRefAssembly);
        }
        if (!string.IsNullOrEmpty(OutputDebugSymbols))
        {
            builder.AppendFormat(" --pdb {0}", OutputDebugSymbols);
        }
        if (!string.IsNullOrEmpty(OutputXmlDocumentation))
        {
            builder.AppendFormat(" --xml {0}", OutputXmlDocumentation);
        }

        if (ResolvedFrameworkReferences is {Length: > 0})
        {
            builder.Append(" --framework-references");

            foreach (var frameworkReference in ResolvedFrameworkReferences)
            {
                builder.AppendFormat(" \"{0}={1}\"", frameworkReference.ItemSpec,
                    frameworkReference.GetMetadata("TargetingPackPath"));
            }
        }
    }
}
