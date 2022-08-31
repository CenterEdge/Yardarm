using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using NuGet.ProjectModel;
using Yardarm.Packaging;
using Yardarm.Packaging.Internal;

namespace Yardarm
{
    /// <summary>
    /// Basic processor which performs actions without an OpenApiDocument.
    /// </summary>
    public class YardarmProcessor
    {
        protected YardarmGenerationSettings Settings { get; }
        protected IServiceProvider ServiceProvider { get; }

        public YardarmProcessor(YardarmGenerationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            Settings = settings;
            ServiceProvider = settings.BuildServiceProvider(null);
        }

        private protected YardarmProcessor(YardarmGenerationSettings settings, IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(serviceProvider);

            Settings = settings;
            ServiceProvider = serviceProvider;
        }

        public Task<PackageSpec> GetPackageSpecAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ServiceProvider.GetRequiredService<PackageSpec>());
        }

        public async Task<NuGetRestoreInfo> RestoreAsync(CancellationToken cancellationToken = default)
        {
            var context = ServiceProvider.GetRequiredService<YardarmContext>();

            await PerformRestoreAsync(context, false, cancellationToken);

            return context.NuGetRestoreInfo!;
        }

        protected async Task PerformRestoreAsync(YardarmContext context, bool readLockFileOnly, CancellationToken cancellationToken = default)
        {
            // Perform the NuGet restore
            var restoreProcessor = context.GenerationServices.GetRequiredService<NuGetRestoreProcessor>();
            context.NuGetRestoreInfo = await restoreProcessor.ExecuteAsync(readLockFileOnly, cancellationToken).ConfigureAwait(false);

            if (context.Settings.TargetFrameworkMonikers.Length == 0)
            {
                throw new InvalidOperationException("No target framework monikers provided.");
            }
        }
    }
}
