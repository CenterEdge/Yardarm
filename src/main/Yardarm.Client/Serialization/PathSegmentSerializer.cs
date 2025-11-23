using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using RootNamespace.Serialization.Literals;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization;

internal static class PathSegmentSerializer
{
    public static string Serialize<T>(string name, T value, PathSegmentStyle style = PathSegmentStyle.Simple, string? format = null) =>
        style switch
        {
            PathSegmentStyle.Simple => SerializeSimple(value, format),
            PathSegmentStyle.Label => SerializeLabel(value, format),
            PathSegmentStyle.Matrix => SerializeMatrix(name, value, format),
            _ => throw new InvalidEnumArgumentException(nameof(style), (int)style, typeof(PathSegmentStyle))
        };

    public static string SerializeList<T>(string name, IEnumerable<T> values, PathSegmentStyle style = PathSegmentStyle.Simple, bool explode = false, string? format = null) =>
        style switch
        {
            PathSegmentStyle.Simple => LiteralSerializer.JoinList(",", values,  format),
            PathSegmentStyle.Label => "." + LiteralSerializer.JoinList(".", values, format),
            PathSegmentStyle.Matrix =>
                $";{name}={LiteralSerializer.JoinList(explode ? $";{name}=" : ",", values, format)}",
            _ => throw new InvalidEnumArgumentException(nameof(style), (int)style, typeof(PathSegmentStyle))
        };

#if NET6_0_OR_GREATER

    public static string Build(PathSegmentInterpolatedStringHandler handler) =>
        handler.ToStringAndClear();

#pragma warning disable IDE0060 // Remove unused parameter
    public static string Build(Span<char> initialBuffer, [InterpolatedStringHandlerArgument(nameof(initialBuffer))] PathSegmentInterpolatedStringHandler handler) =>
        handler.ToStringAndClear();
#pragma warning restore IDE0060 // Remove unused parameter

#endif

    private static string SerializeSimple<T>(T value, string? format = null)
    {
        if (value is null)
        {
            return "";
        }

        if (value is string str)
        {
            // Short-circuit for strings
            return str;
        }

        return LiteralSerializer.Serialize(value, format);
    }

    private static string SerializeLabel<T>(T value, string? format)
    {
        if (value is null)
        {
            return ".";
        }

        if (value is string str)
        {
            // Short-circuit for strings
            return "." + str;
        }

        return "." + LiteralSerializer.Serialize(value, format);
    }

    private static string SerializeMatrix<T>(string name, T value, string? format)
    {
        if (value is null)
        {
            return $";{name}=";
        }

        if (value is string str)
        {
            // Short-circuit for strings
            return $";{name}={str}";
        }

        return $";{name}={LiteralSerializer.Serialize(value, format)}";
    }
}
