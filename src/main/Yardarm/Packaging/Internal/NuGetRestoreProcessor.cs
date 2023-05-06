﻿using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NuGet.Commands;
using NuGet.Configuration;
using NuGet.Packaging.Signing;
using NuGet.ProjectModel;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using Yardarm.Generation.Internal;

namespace Yardarm.Packaging.Internal
{
    internal class NuGetRestoreProcessor
    {
        private readonly PackageSpec _packageSpec;
        private readonly YardarmGenerationSettings _settings;
        private readonly ILogger<NuGetReferenceGenerator> _logger;

        public NuGetRestoreProcessor(PackageSpec packageSpec, YardarmGenerationSettings settings,
            ILogger<NuGetReferenceGenerator> logger)
        {
            ArgumentNullException.ThrowIfNull(packageSpec);
            ArgumentNullException.ThrowIfNull(settings);
            ArgumentNullException.ThrowIfNull(logger);

            _packageSpec = packageSpec;
            _settings = settings;
            _logger = logger;
        }

        public async Task<NuGetRestoreInfo> ExecuteAsync(bool readLockFileOnly, CancellationToken cancellationToken = default)
        {
            string? intermediatePath = _settings.IntermediateOutputPath;
            bool isTempPath = false;

            if (string.IsNullOrEmpty(intermediatePath))
            {
                intermediatePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                Directory.CreateDirectory(intermediatePath);
                isTempPath = true;
            }
            else if (!Path.IsPathFullyQualified(intermediatePath))
            {
                intermediatePath = Path.Combine(Directory.GetCurrentDirectory(), intermediatePath);
            }

            try
            {
                using var cacheContext = new SourceCacheContext();

                _packageSpec.RestoreMetadata = new ProjectRestoreMetadata
                {
                    OutputPath = intermediatePath,
                    CacheFilePath = Path.Combine(intermediatePath, "project.nuget.cache"),
                    ProjectName = _packageSpec.Name,
                    ProjectPath = $"{_packageSpec.Name}.csproj", // Ensures consistent naming of .nuget.g.props/targets in intermediate directory
                    ProjectStyle = ProjectStyle.PackageReference,
                    CrossTargeting = true,
                };
                _packageSpec.RestoreMetadata.ProjectUniqueName = _packageSpec.RestoreMetadata.ProjectPath;

                var settings = Settings.LoadDefaultSettings(intermediatePath);

                var logger = new NuGetLogger(_logger);

                var dependencyProviders = RestoreCommandProviders.Create(
                    SettingsUtility.GetGlobalPackagesFolder(settings),
                    Enumerable.Empty<string>(),
                    SettingsUtility.GetEnabledSources(settings).Select(source =>
                        Repository.Factory.GetCoreV3(source.Source)),
                    cacheContext,
                    new LocalPackageFileCache(),
                    logger);

                if (!readLockFileOnly)
                {
                    var clientPolicyContext = ClientPolicyContext.GetClientPolicy(settings, logger);
                    var packageSourceMapping = PackageSourceMapping.GetPackageSourceMapping(settings);

                    var dependencyGraphSpec = new DependencyGraphSpec();
                    dependencyGraphSpec.AddProject(_packageSpec);

                    var restoreRequest = new RestoreRequest(_packageSpec, dependencyProviders, cacheContext,
                        clientPolicyContext, packageSourceMapping, logger, new LockFileBuilderCache())
                    {
                        ProjectStyle = ProjectStyle.PackageReference,
                        RestoreOutputPath = intermediatePath,
                        AllowNoOp = !isTempPath,
                        DependencyGraphSpec = dependencyGraphSpec
                    };

                    var restoreCommand = new RestoreCommand(restoreRequest);

                    var result = await restoreCommand.ExecuteAsync(cancellationToken).ConfigureAwait(false);
                    if (!result.Success)
                    {
                        throw new NuGetRestoreException(result);
                    }

                    if (!isTempPath) // No need to commit if we're deleting anyway
                    {
                        await result.CommitAsync(logger, cancellationToken).ConfigureAwait(false);
                    }

                    return new NuGetRestoreInfo(dependencyProviders, result.LockFile);
                }
                else
                {
                    var lockFileName = Path.Combine(intermediatePath, LockFileFormat.AssetsFileName);

                    var lockFile = LockFileUtilities.GetLockFile(lockFileName, logger);

                    return new NuGetRestoreInfo(dependencyProviders, lockFile);
                }
            }
            finally
            {
                if (isTempPath)
                {
                    Directory.Delete(intermediatePath, true);
                }
            }
        }
    }
}
