using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace RootNamespace.Internal;

internal static class EnumExtensions
{
#if !NET5_0_OR_GREATER

    extension(Enum)
    {
        public static bool IsDefined<TEnum>(TEnum value)
            where TEnum : struct, Enum =>
            Enum.IsDefined(typeof(TEnum), value);
    }

#endif

    extension(InvalidEnumArgumentException)
    {
        public static void ThrowIfNotDefined<TEnum>(TEnum value, [CallerArgumentExpression(nameof(value))] string? argumentName = null)
            where TEnum : struct, Enum
        {
            if (!Enum.IsDefined(value))
            {
                ThrowInvalidEnumArgumentException(argumentName, (int)(object) value, typeof(TEnum));
            }
        }
    }

    [DoesNotReturn]
    private static void ThrowInvalidEnumArgumentException(string? argumentName, int invalidValue, Type enumClass) =>
        throw new InvalidEnumArgumentException(argumentName, invalidValue, enumClass);
}
