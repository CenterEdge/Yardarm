using System;
using System.Diagnostics.CodeAnalysis;

namespace Yardarm.Helpers;

internal static class ThrowHelpers
{
    [DoesNotReturn]
    public static void ThrowInvalidOperationException(string? message)
        => throw new InvalidOperationException(message);
}
