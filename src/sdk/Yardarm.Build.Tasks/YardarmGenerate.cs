using System;
using System.Text;
using Microsoft.Build.Framework;

namespace Yardarm.Build.Tasks;

public class YardarmGenerate : YardarmCommonTask
{
    protected override string Verb => "generate";

    public string? Version { get; set; }
    public bool EmbedAllSources { get; set; } = true;
    public string? OutputAssembly { get; set; }
    public string? OutputRefAssembly { get; set; }
    public string? OutputDebugSymbols { get; set; }
    public string? OutputXmlDocumentation { get; set; }

    protected override void AppendAdditionalArguments(StringBuilder builder)
    {
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
    }
}
