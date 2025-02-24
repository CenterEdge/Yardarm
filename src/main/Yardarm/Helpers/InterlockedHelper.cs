using System;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Yardarm.Helpers;

internal static class InterlockedHelper
{
    /// <summary>
    /// Initialize a reference type if it is null, and return the initialized value.
    /// Thread-safe and will only ever return a single value to all threads.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetOrInitialize<T>(ref T? target) where T : class, new() =>
        target
            ?? Interlocked.CompareExchange(ref target, new T(), null)
            ?? target;

    /// <summary>
    /// Initialize a reference type if it is null, and return the initialized value.
    /// Thread-safe and will only ever return a single value to all threads.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static T GetOrInitialize<T>(ref T? target, Func<T> initializer) where T : class
    {
        ArgumentNullException.ThrowIfNull(initializer);

        return target
            ?? Interlocked.CompareExchange(ref target, initializer(), null)
            ?? target;
    }
}
