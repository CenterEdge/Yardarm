using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Yardarm.Client.Internal
{
    internal static class ThrowHelper
    {
        [DoesNotReturn]
        public static void ThrowFormatException(string? message, Exception? innerException = null) =>
            throw new FormatException(message, innerException);

        [DoesNotReturn]
        public static void ThrowInvalidOperationException(string? message) =>
            throw new InvalidOperationException(message);

        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
#if NET6_0_OR_GREATER
        [StackTraceHidden]
#endif
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
#if NET6_0_OR_GREATER
            // Forward to the common implementation
            ArgumentNullException.ThrowIfNull(argument, paramName);
#else
            if (argument is null)
            {
                ThrowArgumentNullException(paramName);
            }
#endif
        }

        [DoesNotReturn]
        public static void ThrowArgumentException(string? message, string? paramName)
        {
            throw new ArgumentException(message, paramName);
        }

#if !NET6_0_OR_GREATER
        // Use a separate method to throw so that ThrowIfNull may be inlined
        [DoesNotReturn]
        private static void ThrowArgumentNullException(string? paramName)
        {
            throw new ArgumentNullException(paramName);
        }
#endif

        [DoesNotReturn]
        public static void ThrowKeyNotFoundException(string? message = null)
        {
            throw new KeyNotFoundException(message);
        }
    }
}
