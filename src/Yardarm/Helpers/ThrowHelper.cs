using System;
using System.Diagnostics.CodeAnalysis;

namespace Yardarm.Helpers
{
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowArgumentNullException(string paramName)
        {
            throw new ArgumentNullException(paramName);
        }
    }
}
