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
    public string? KeyFile { get; set; }
    public string? KeyContainerName { get; set; }
    public string? DelaySign { get; set; }
    public string? PublicSign { get; set; }

    public ITaskItem[]? SpecFile { get; set; }
    public ITaskItem[]? References { get; set; }

    protected override bool ValidateParameters()
    {
        if (!base.ValidateParameters())
        {
            return false;
        }

        if (SpecFile is null || SpecFile.Length != 1)
        {
            Log.LogError("Must supply a single SpecFile.");
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

        builder.AppendFormat(" -i {0}", SpecFile![0].ItemSpec);

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
        if (!string.IsNullOrEmpty(KeyFile))
        {
            builder.AppendFormat(" --keyfile {0}", KeyFile);
        }
        if (!string.IsNullOrEmpty(KeyContainerName))
        {
            builder.AppendFormat(" --keycontainername {0}", KeyContainerName);
        }
        if (DelaySign == "true")
        {
            builder.Append(" --delay-sign");
        }
        if (PublicSign == "true")
        {
            builder.Append(" --public-sign");
        }

        if (References is {Length: > 0})
        {
            builder.Append(" --references");

            foreach (var reference in References)
            {
                var referencePath = reference.GetMetadata("ReferenceAssembly");
                if (string.IsNullOrEmpty(referencePath))
                {
                    referencePath = reference.ItemSpec;
                }

                builder.AppendFormat(" \"{0}\"", referencePath);
            }
        }
    }
}
