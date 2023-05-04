using System;
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
        public static void ThrowIfNull([NotNull] object? argument,
#if NET6_0_OR_GREATER
            [CallerArgumentExpression(nameof(argument))]
#endif
            string? paramName = null)
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

#if !NET6_0_OR_GREATER
        // Use a separate method to throw so that ThrowIfNull may be inlined
        [DoesNotReturn]
        private static void ThrowArgumentNullException(string? paramName)
        {
            throw new ArgumentNullException(paramName);
        }
#endif
    }
}
