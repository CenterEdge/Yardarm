using System;
using System.Text.RegularExpressions;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Yardarm.Build.Tasks
{
    /// <remarks>
    /// Derived tasks are marked with <see cref="MSBuildMultiThreadableTaskAttribute"/>. They must not touch process-wide
    /// state or resolve relative paths themselves; relative paths are passed through to the command line unchanged.
    /// <see cref="ToolTask"/> starts the tool from its injected TaskEnvironment (MSBuild 18.7 and later, SDK 10.0.400),
    /// which sets the working directory and environment variables to the project's, so those paths still resolve
    /// against the project directory when MSBuild runs in multithreaded mode.
    /// </remarks>
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
