using System;
using System.Text;
using Microsoft.Build.Framework;

namespace Yardarm.Build.Tasks
{
    public abstract class YardarmCommonTask : YardarmTask
    {
        protected abstract string Verb { get; }

        public string? AssemblyName { get; set; }
        public string? TargetFramework { get; set; }
        public string? BaseIntermediateOutputPath { get; set; }

        public ITaskItem[]? SpecFile { get; set; }
        public ITaskItem[]? Extensions { get; set; }

        protected override bool ValidateParameters()
        {
            if (SpecFile is null || SpecFile.Length != 1)
            {
                Log.LogError("Must supply a single SpecFile.");
                return false;
            }

            if (string.IsNullOrWhiteSpace(TargetFramework))
            {
                Log.LogError("TargetFramework is required.");
                return false;
            }

            return true;
        }

        protected override string GenerateCommandLineCommands()
        {
            var builder = new StringBuilder(Verb);
            builder.AppendFormat(" -n {0}", string.IsNullOrEmpty(AssemblyName) ? "YardarmSdk" : AssemblyName);
            builder.AppendFormat(" -f {0}", TargetFramework);

            builder.AppendFormat(" -i {0}", SpecFile![0].ItemSpec);

            if (!string.IsNullOrEmpty(BaseIntermediateOutputPath))
            {
                builder.AppendFormat(" --intermediate-dir {0}", BaseIntermediateOutputPath);
            }

            if (Extensions is {Length: > 0})
            {
                builder.Append(" -x");
                foreach (var extension in Extensions)
                {
                    builder.Append(' ');
                    builder.Append(extension.ItemSpec);
                }
            }

            return builder.ToString();
        }

        protected virtual void AppendAdditionalArguments(StringBuilder builder)
        {
        }
    }
}
