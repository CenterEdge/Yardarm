using System;
using Microsoft.Extensions.Options;
using NuGet.Frameworks;
using Yardarm.Internal;
using Yardarm.Packaging;

namespace Yardarm.Generation.Schema;

internal class StringSchemaOptionsConfigurator(
    YardarmGenerationSettings settings)
    : IConfigureOptions<GenerationOptions>
{
    public void Configure(GenerationOptions options)
    {
        // Use legacy handling for DateOnly and TimeOnly if explicitly requested
        if (settings.Properties.TryGetValue("LegacyDateTimeHandling", out string? legacyDateTimeHandling) &&
            string.Equals(legacyDateTimeHandling, "true", StringComparison.OrdinalIgnoreCase))
        {
            options.LegacyDateTimeHandling = true;
            return;
        }

        // Also, use legacy handling if any target framework is less than .NET 6. This is because .NET Standard or
        // earlier .NET Core versions do not support DateOnly and TimeOnly. Because public APIs should be forward
        // compatible from older targets, we need to use legacy handling for all targets if any target is unsupported
        // to ensure consistent API surface.
        foreach (string moniker in settings.TargetFrameworkMonikers)
        {
            var framework = NuGetFramework.Parse(moniker);
            if (framework.Framework != NuGetFrameworkConstants.NetCoreApp || framework.Version.Major < 6)
            {
                options.LegacyDateTimeHandling = true;
                return;
            }
        }
    }
}
