#if NET6_0_OR_GREATER

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// Some code ported from DefaultInterpolatedStringHandler

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using RootNamespace.Serialization.Literals;

namespace RootNamespace.Serialization;

[InterpolatedStringHandler]
internal ref struct PathSegmentInterpolatedStringHandler
{
    private const int GuessedLengthPerHole = 11;
    private const int MinimumArrayPoolLength = 256;

    private char[]? _arrayToReturnToPool;
    /// <summary>The span to write into.</summary>
    private Span<char> _chars;
    /// <summary>Position at which to write the next character.</summary>
    private int _pos;

    public PathSegmentInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _chars = _arrayToReturnToPool = ArrayPool<char>.Shared.Rent(GetDefaultLength(literalLength, formattedCount));
        _pos = 0;
    }

    public PathSegmentInterpolatedStringHandler(int literalLength, int formattedCount, Span<char> initialBuffer)
    {
        _chars = initialBuffer;
        _arrayToReturnToPool = null;
        _pos = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // becomes a constant when inputs are constant
    private static int GetDefaultLength(int literalLength, int formattedCount) =>
        Math.Max(MinimumArrayPoolLength, literalLength + (formattedCount * GuessedLengthPerHole));

    private readonly ReadOnlySpan<char> Text => _chars.Slice(0, _pos);

    public override readonly string ToString() => new(Text);

    public string ToStringAndClear()
    {
        string result = new(Text);

        char[]? toReturn = _arrayToReturnToPool;
        this = default; // defensive clear
        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }

        return result;
    }

    public void AppendLiteral(string value)
    {
        if (value.TryCopyTo(_chars.Slice(_pos)))
        {
            _pos += value.Length;
        }
        else
        {
            GrowThenCopyString(value);
        }
    }

    #region AppendFormatted

    // We apply a "struct" constraint to ensure that IEnumerable<T> cases aren't routed through the AppendFormattted<T>(T value) overloads.
    // C# will prefer them for List<T>, T[], etc if the constraint is not present. However, this means we also need separate overloads for T?
    // to handle nullable cases which won't match the non-nullable T overload. Reference types will either use the string or object overloads
    // unless they are IEnumerable<T>, in which case they will use the IEnumerable<T> or List<T> overloads.
    //
    // alignment is a not used to represent padding and alignment, and instead encodes the PathSegmentStyle, explode boolean, and the length
    // of the name for PathSegmentStyle.Matrix
    //   0 = PathSegmentStyle.Simple
    //   1 = PathSegmentStyle.Label
    //   -1 = Unused, but reserved for future use
    //   >= 2 or <= -2 = PathSegmentStyle.Matrix, length of the name is ABS(alignment) - 2
    //   If negative, indicates that the list should be exploded
    //
    // format is the format in terms of the LiteralSerializer formats, not standard .NET formats.
    // For PathSegmentStyle.Matrix, the beginning of the format is the name and the remainder is the format for LiteralSerializer.

    public void AppendFormatted<T>(T value) where T : struct
    {
        int charsWritten;
        while (!LiteralSerializer.TrySerialize(value, format: default, _chars.Slice(_pos), out charsWritten))
        {
            Grow();
        }

        _pos += charsWritten;
    }

    public void AppendFormatted<T>(T value, string? format) where T : struct
    {
        int charsWritten;
        while (!LiteralSerializer.TrySerialize(value, format, _chars.Slice(_pos), out charsWritten))
        {
            Grow();
        }

        _pos += charsWritten;
    }

    public void AppendFormatted<T>(T value, int alignment, string? format = null) where T : struct
    {
        var trimmedFormat = AppendStyle(alignment, format);

        int charsWritten;
        while (!LiteralSerializer.TrySerialize(value, trimmedFormat, _chars.Slice(_pos), out charsWritten))
        {
            Grow();
        }

        _pos += charsWritten;
    }

    public void AppendFormatted<T>(T? value) where T : struct
    {
        if (value is not null)
        {
            int charsWritten;
            while (!LiteralSerializer.TrySerialize(value.GetValueOrDefault(), format: default, _chars.Slice(_pos), out charsWritten))
            {
                Grow();
            }

            _pos += charsWritten;
        }
    }

    public void AppendFormatted<T>(T? value, string? format) where T : struct
    {
        if (value is not null)
        {
            int charsWritten;
            while (!LiteralSerializer.TrySerialize(value.GetValueOrDefault(), format, _chars.Slice(_pos), out charsWritten))
            {
                Grow();
            }

            _pos += charsWritten;
        }
    }

    public void AppendFormatted<T>(T? value, int alignment, string? format = null) where T : struct
    {
        var trimmedFormat = AppendStyle(alignment, format);

        if (value is not null)
        {
            int charsWritten;
            while (!LiteralSerializer.TrySerialize(value.GetValueOrDefault(), trimmedFormat, _chars.Slice(_pos), out charsWritten))
            {
                Grow();
            }

            _pos += charsWritten;
        }
    }

    public void AppendFormatted<T>(List<T>? values, int alignment = 0, string? format = null)
    {
        // Store the current position so we can use it for exploded matrix separators
        var posCache = _pos;

        var trimmedFormat = AppendStyle(alignment, format);

        if (values is null || values.Count == 0)
        {
            // short circuit
            return;
        }

        // Compute the separator. We can't use a ROS<char> because _chars may be replaced if a Grow occurs and
        // the buffer is returned to the ArrayPool, so we make a copy in the exploded Matrix case.
        var separator = alignment switch
        {
            0 or >= 2 => ",",
            <= -2 => new string(_chars[posCache.._pos]), // Exploded matrix repeats ";name=" as the separator
            _ => "." // Label is always "." separator
        };

        for (int i=0; i<values.Count; i++)
        {
            if (i > 0)
            {
                AppendLiteral(separator);
            }

            T value = values[i];
            int charsWritten;
            while (!LiteralSerializer.TrySerialize(value, trimmedFormat, _chars.Slice(_pos), out charsWritten))
            {
                Grow();
            }

            _pos += charsWritten;
        }
    }

    // Fallback in cases where the enumerable is not a List<T>. This is less performant because the foreach loop
    // must enumerate using an enumerator rather than using List<T> optimizations.
    public void AppendFormatted<T>(IEnumerable<T>? values, int alignment = 0, string? format = null)
    {
        // Store the current position so we can use it for exploded matrix separators
        var posCache = _pos;

        var trimmedFormat = AppendStyle(alignment, format);

        if (values is null)
        {
            // short circuit
            return;
        }

        var separator = alignment switch
        {
            0 or >= 2 => ",",
            <= -2 => new string(_chars[posCache.._pos]), // Exploded matrix repeats ";name=" as the separator
            _ => "." // Label is always "." separator
        };


        IEnumerator<T> enumerator = values.GetEnumerator();
        try
        {
            if (!enumerator.MoveNext())
            {
                return;
            }

            while (true)
            {
                int charsWritten;
                while (!LiteralSerializer.TrySerialize(enumerator.Current, trimmedFormat, _chars.Slice(_pos), out charsWritten))
                {
                    Grow();
                }

                _pos += charsWritten;

                if (!enumerator.MoveNext())
                {
                    break;
                }

                AppendLiteral(separator);
            }
        }
        finally
        {
            enumerator?.Dispose();
        }
    }

    public void AppendFormatted(scoped ReadOnlySpan<char> value)
    {
        // Fast path for when the value fits in the current buffer
        if (value.TryCopyTo(_chars.Slice(_pos)))
        {
            _pos += value.Length;
        }
        else
        {
            GrowThenCopySpan(value);
        }
    }

    // Reduces IL size at the callsite when there is no alignment
    public void AppendFormatted(scoped ReadOnlySpan<char> value, string? format) => AppendFormatted(value);

    public void AppendFormatted(scoped ReadOnlySpan<char> value, int alignment, string? format = null)
    {
        AppendStyle(alignment, format);

        AppendFormatted(value);
    }

    public void AppendFormatted(string? value)
    {
        if (value is not null)
        {
            AppendLiteral(value);
        }
    }

    // Reduces IL size at the callsite when there is no alignment
    public void AppendFormatted(string? value, string? format) => AppendFormatted(value);

    public void AppendFormatted(string? value, int alignment, string? format = null)
    {
        AppendStyle(alignment, format);

        if (value is not null)
        {
            AppendLiteral(value);
        }
    }

    public void AppendFormatted(object value)
    {
        int charsWritten;
        while (!LiteralSerializer.TrySerialize(value, format: default, _chars.Slice(_pos), out charsWritten))
        {
            Grow();
        }

        _pos += charsWritten;
    }

    public void AppendFormatted(object value, string? format)
    {
        int charsWritten;
        while (!LiteralSerializer.TrySerialize(value, format, _chars.Slice(_pos), out charsWritten))
        {
            Grow();
        }

        _pos += charsWritten;
    }

    public void AppendFormatted(object value, int alignment, string? format = null)
    {
        var trimmedFormat = AppendStyle(alignment, format);

        int charsWritten;
        while (!LiteralSerializer.TrySerialize(value, trimmedFormat, _chars.Slice(_pos), out charsWritten))
        {
            Grow();
        }

        _pos += charsWritten;
    }

    private ReadOnlySpan<char> AppendStyle(int alignment, ReadOnlySpan<char> format)
    {
        switch (alignment)
        {
            case 0:
                break;

            case 1: // PathSegmentStyle.Label, ignores explode negation
                EnsureCapacityForAdditionalCharsAndSlice(1)[0] = '.';
                _pos++;
                break;

            default: // PathSegmentStyle.Matrix, inverse of alignment is the portion of the format that stores the name
                // For matrix, style is name length + 2, and name is stored at the beginning of the format. But we must
                // also add 2 characters, so style is effectively the total length.
                int style = Math.Abs(alignment);

                var nameSpan = EnsureCapacityForAdditionalCharsAndSlice(style);
                nameSpan[0] = ';';
                format.Slice(0, style - 2).CopyTo(nameSpan.Slice(1));
                nameSpan[^1] = '=';

                _pos += style;

                // Update the format to exclude the name
                return format.Slice(style - 2);
        }

        return format;
    }

    #endregion

    #region Grow

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Span<char> EnsureCapacityForAdditionalCharsAndSlice(int additionalChars)
    {
        if (_chars.Length - _pos < additionalChars)
        {
            Grow(additionalChars);
        }

        return _chars.Slice(_pos, additionalChars);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowThenCopyString(string value)
    {
        Grow(value.Length);
        value.CopyTo(_chars.Slice(_pos));
        _pos += value.Length;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void GrowThenCopySpan(scoped ReadOnlySpan<char> value)
    {
        Grow(value.Length);
        value.CopyTo(_chars.Slice(_pos));
        _pos += value.Length;
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // keep consumers as streamlined as possible
    private void Grow(int additionalChars)
    {
        // This method is called when the remaining space (_chars.Length - _pos) is
        // insufficient to store a specific number of additional characters.  Thus, we
        // need to grow to at least that new total. GrowCore will handle growing by more
        // than that if possible.
        Debug.Assert(additionalChars > _chars.Length - _pos);
        GrowCore((uint)_pos + (uint)additionalChars);
    }

    [MethodImpl(MethodImplOptions.NoInlining)] // keep consumers as streamlined as possible
    private void Grow()
    {
        // This method is called when the remaining space in _chars isn't sufficient to continue
        // the operation.  Thus, we need at least one character beyond _chars.Length.  GrowCore
        // will handle growing by more than that if possible.
        GrowCore((uint)_chars.Length + 1);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)] // but reuse this grow logic directly in both of the above grow routines
    private void GrowCore(uint requiredMinCapacity)
    {
        // We want the max of how much space we actually required and doubling our capacity (without going beyond the max allowed length). We
        // also want to avoid asking for small arrays, to reduce the number of times we need to grow, and since we're working with unsigned
        // ints that could technically overflow if someone tried to, for example, append a huge string to a huge string, we also clamp to int.MaxValue.
        // Even if the array creation fails in such a case, we may later fail in ToStringAndClear.

        uint newCapacity = Math.Max(requiredMinCapacity, Math.Min((uint)_chars.Length * 2, 0x3FFFFFDF));
        int arraySize = (int)Math.Clamp(newCapacity, MinimumArrayPoolLength, int.MaxValue);

        char[] newArray = ArrayPool<char>.Shared.Rent(arraySize);
        _chars.Slice(0, _pos).CopyTo(newArray);

        char[]? toReturn = _arrayToReturnToPool;
        _chars = _arrayToReturnToPool = newArray;

        if (toReturn is not null)
        {
            ArrayPool<char>.Shared.Return(toReturn);
        }
    }

    #endregion
}

#endif
