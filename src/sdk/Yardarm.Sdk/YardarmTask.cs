using System;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Yardarm.Build.Tasks
{
    public abstract partial class YardarmTask : ToolTask
    {
        [GeneratedRegex(@"^\[(\w+)\] (.+)")]
        private static partial Regex ErrorRegex { get; }

        protected override string GenerateFullPathToTool() => ToolName;

        protected override string ToolName => "dotnet";


        protected override void LogEventsFromTextOutput(string singleLine, MessageImportance messageImportance)
        {
            var match = ErrorRegex.Match(singleLine);

            if (match.Success)
            {
                var type = match.Groups[1].Value;
                var message = match.Groups[2].Value;

                switch (type)
                {
                    case "ERR":
                        Log.LogError(message);
                        break;

                    case "WRN":
                        Log.LogWarning(message);
                        break;

                    default:
                        Log.LogMessage(messageImportance, message);
                        break;
                }
            }
            else
            {
                Log.LogMessage(messageImportance, singleLine);
            }
        }
    }
}
