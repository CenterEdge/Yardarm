using System;
using NuGet.Commands;

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
        public RestoreResult Result { get; set; }

        public NuGetRestoreInfo(RestoreCommandProviders providers, RestoreResult result)
        {
            Providers = providers;
            Result = result;
        }
    }
}
