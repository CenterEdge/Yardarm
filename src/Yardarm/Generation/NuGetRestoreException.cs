using System;
using NuGet.Commands;

namespace Yardarm.Generation
{
    public class NuGetRestoreException : Exception
    {
        public RestoreResult Result { get; set; }

        public NuGetRestoreException(RestoreResult result) : this(result, null)
        {
        }

        public NuGetRestoreException(RestoreResult result, Exception? innerException)
            : base("Failed to restore NuGet packages.", innerException)
        {
            Result = result;
        }
    }
}
