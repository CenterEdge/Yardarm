using System;
using NuGet.Commands;
using NuGet.ProjectModel;

namespace Yardarm.Packaging
{
    public class NuGetRestoreInfo
    {
        /// <summary>
        /// Providers used to execute the restore command.
        /// </summary>
        public RestoreCommandProviders Providers { get; set; }

        /// <summary>
        /// Result of the restore command.
        /// </summary>
        public LockFile LockFile { get; set; }

        public NuGetRestoreInfo(RestoreCommandProviders providers, LockFile lockFile)
        {
            Providers = providers;
            LockFile = lockFile;
        }
    }
}
