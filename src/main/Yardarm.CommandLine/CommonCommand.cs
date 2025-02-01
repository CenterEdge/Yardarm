using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Yardarm.CommandLine
{
    public class CommonCommand
    {
        private readonly CommonOptions _options;

        public CommonCommand(CommonOptions options)
        {
            ArgumentNullException.ThrowIfNull(options);

            _options = options;
        }

        protected void ApplyExtensions(YardarmGenerationSettings settings)
        {
            foreach (string extensionFile in _options.ExtensionFiles)
            {
                try
                {
                    if (extensionFile.EndsWith(".dll"))
                    {
                        // Appears to be a file reference

                        string fullPath = !Path.IsPathFullyQualified(extensionFile)
                            ? Path.Combine(Directory.GetCurrentDirectory(), extensionFile)
                            : extensionFile;

                        Assembly assembly = Assembly.LoadFile(fullPath);

                        settings.AddExtension(assembly);
                    }
                    else
                    {
                        // Appears to be an assembly name

                        Assembly assembly = Assembly.Load(extensionFile);

                        settings.AddExtension(assembly);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error loading extension '{extensionFile}'.", ex);
                }
            }
        }

        protected virtual void ApplyNuGetSettings(YardarmGenerationSettings settings)
        {
            string[] targetFrameworks = _options.TargetFrameworks.ToArray();
            if (targetFrameworks.Length > 0)
            {
                settings.TargetFrameworkMonikers = targetFrameworks.ToImmutableArray();
            }
        }

        protected void ApplyProperties(YardarmGenerationSettings settings)
        {
            foreach (string property in _options.Properties)
            {
                string[] parts = property.Split('=', 2);
                if (parts.Length != 2)
                {
                    throw new ArgumentException($"Invalid property '{property}'. Properties must be in the format 'NAME=VALUE'.");
                }

                settings.Properties[parts[0]] = parts[1];
            }
        }
    }
}
