using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace RootNamespace;

internal static class ThrowHelper
{
    [DoesNotReturn]
    public static void ThrowFormatException(string? message, Exception? innerException = null) =>
        throw new FormatException(message, innerException);

    [DoesNotReturn]
    public static void ThrowInvalidOperationException(string? message) =>
        throw new InvalidOperationException(message);

#if !NET6_0_OR_GREATER

    extension(ArgumentNullException)
    {
        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static void ThrowIfNull([NotNull] object? argument,
            [System.Runtime.CompilerServices.CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                ThrowArgumentNullException(paramName);
            }
        }
    }

    // Use a separate method to throw so that ThrowIfNull may be inlined
    [DoesNotReturn]
    private static void ThrowArgumentNullException(string? paramName)
    {
        throw new ArgumentNullException(paramName);
    }

#endif

    [DoesNotReturn]
    public static void ThrowArgumentException(string? message, string? paramName)
    {
        throw new ArgumentException(message, paramName);
    }

    [DoesNotReturn]
    public static void ThrowKeyNotFoundException(string? message = null)
    {
        throw new KeyNotFoundException(message);
    }
}
