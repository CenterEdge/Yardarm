using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    internal class PathSegmentSerializer
    {
        public static string Serialize<
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            T>(string name, T value, PathSegmentStyle style = PathSegmentStyle.Simple, string? format = null) =>
            style switch
            {
                PathSegmentStyle.Simple => SerializeSimple(value, format),
                PathSegmentStyle.Label => SerializeLabel(value, format),
                PathSegmentStyle.Matrix => SerializeMatrix(name, value, format),
                _ => throw new InvalidEnumArgumentException(nameof(style), (int)style, typeof(PathSegmentStyle))
            };

        public static string SerializeList<
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            T>(string name, IEnumerable<T> values, PathSegmentStyle style = PathSegmentStyle.Simple, bool explode = false, string? format = null) =>
            style switch
            {
                PathSegmentStyle.Simple => LiteralSerializer.Instance.JoinList(",", values,  format),
                PathSegmentStyle.Label => "." + LiteralSerializer.Instance.JoinList(explode ? "." : ",", values, format),
                PathSegmentStyle.Matrix =>
                    $";{name}={LiteralSerializer.Instance.JoinList(explode ? $";{name}=" : ",", values, format)}",
                _ => throw new InvalidEnumArgumentException(nameof(style), (int)style, typeof(PathSegmentStyle))
            };

        private static string SerializeSimple<
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            T>(T value, string? format = null)
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

            return LiteralSerializer.Instance.Serialize(value, format);
        }

        private static string SerializeLabel<
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            T>(T value, string? format)
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

            return "." + LiteralSerializer.Instance.Serialize(value, format);
        }

        private static string SerializeMatrix<
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            T>(string name, T value, string? format)
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

            return $";{name}={LiteralSerializer.Instance.Serialize(value, format)}";
        }
    }
}
