using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using NuGet.Commands;

namespace Yardarm.Packaging
{
    public class NuGetRestoreInfo
    {
        /// <summary>
        /// Providers used to execute the restore command.
        /// </summary>
        public RestoreCommandProviders? Providers { get; set; }

        /// <summary>
        /// Result of the restore command.
        /// </summary>
        public RestoreResult? Result { get; set; }

        /// <summary>
        /// List of loaded source generators to be run after other code generation is complete.
        /// </summary>
        public IReadOnlyList<ISourceGenerator>? SourceGenerators { get; set; }
    }
}
