using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using NuGet.Frameworks;
using Yardarm.Generation;
using Yardarm.Names;
using Yardarm.Packaging;
using Yardarm.Spec;

namespace Yardarm
{
    public class YardarmContext
    {
        public YardarmGenerationSettings Settings { get; }
        public IServiceProvider GenerationServices { get; }

        /// <summary>
        /// Details about the NuGet restore operation, once it is completed.
        /// </summary>
        public NuGetRestoreInfo? NuGetRestoreInfo { get; set; }

        public YardarmContext(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            GenerationServices = serviceProvider;
            Settings = serviceProvider.GetRequiredService<YardarmGenerationSettings>();
        }
    }
}
