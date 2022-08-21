using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    internal class PathSegmentSerializer
    {

        public static PathSegmentSerializer Instance { get; } = new PathSegmentSerializer();

        public string Serialize<
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            T>(string name, T value, PathSegmentStyle style = PathSegmentStyle.Simple, bool explode = false) =>
            style switch
            {
                PathSegmentStyle.Simple => SerializeSimple(value, explode),
                PathSegmentStyle.Label => SerializeLabel(value, explode),
                PathSegmentStyle.Matrix => SerializeMatrix(name, value, explode),
                _ => throw new InvalidEnumArgumentException(nameof(style), (int)style, typeof(PathSegmentStyle))
            };

        private static string SerializeSimple<
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            T>(T value, bool explode)
        {
            if (value == null)
            {
                return "";
            }

            if (value is string str)
            {
                // Short-circuit for strings
                return str;
            }

            if (SerializationHelpers.IsEnumerable(typeof(T), out Type? itemType))
            {
                return LiteralSerializer.Instance.JoinList(",", value, itemType);
            }

            return LiteralSerializer.Instance.Serialize(value);
        }

        private static string SerializeLabel<
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            T>(T value, bool explode)
        {
            if (value == null)
            {
                return ".";
            }

            if (value is string str)
            {
                // Short-circuit for strings
                return "." + str;
            }

            if (SerializationHelpers.IsEnumerable(typeof(T), out Type? itemType))
            {
                return "." + LiteralSerializer.Instance.JoinList(explode ? "." : ",", value, itemType);
            }

            return "." + LiteralSerializer.Instance.Serialize(value);
        }

        private static string SerializeMatrix<
#if NET6_0_OR_GREATER
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)]
#endif
            T>(string name, T value, bool explode)
        {
            if (value == null)
            {
                return $";{name}=";
            }

            if (value is string str)
            {
                // Short-circuit for strings
                return $";{name}={str}";
            }

            if (SerializationHelpers.IsEnumerable(typeof(T), out Type? itemType))
            {
                string prefix = $";{name}=";

                return prefix + LiteralSerializer.Instance.JoinList(explode ? prefix : ",", value, itemType);
            }

            return $";{name}={LiteralSerializer.Instance.Serialize(value)}";
        }
    }
}
