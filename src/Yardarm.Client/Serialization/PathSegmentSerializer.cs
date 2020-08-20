using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    internal class PathSegmentSerializer
    {
        private static readonly MethodInfo _joinListMethod = typeof(PathSegmentSerializer)
            .GetMethods(BindingFlags.NonPublic | BindingFlags.Static)
            .Single(p => p.Name == "JoinList");

        public static PathSegmentSerializer Instance { get; } = new PathSegmentSerializer();

        public string Serialize<T>(string name, T value, PathSegmentStyle style = PathSegmentStyle.Simple, bool explode = false) =>
            style switch
            {
                PathSegmentStyle.Simple => SerializeSimple(value, explode),
                PathSegmentStyle.Label => SerializeLabel(value, explode),
                PathSegmentStyle.Matrix => SerializeMatrix(name, value, explode),
                _ => throw new InvalidEnumArgumentException(nameof(style), (int)style, typeof(PathSegmentStyle))
            };

        private static string SerializeSimple<T>(T value, bool explode)
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

            if (IsEnumerable(typeof(T), out Type? itemType))
            {
                MethodInfo joinList = _joinListMethod.MakeGenericMethod(itemType);

                return (string)joinList.Invoke(null, new object[] {",", value});
            }

            return SerializeLiteral(value);
        }

        private static string SerializeLabel<T>(T value, bool explode)
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

            if (IsEnumerable(typeof(T), out Type? itemType))
            {
                MethodInfo joinList = _joinListMethod.MakeGenericMethod(itemType);

                return "." + (string)joinList.Invoke(null,
                    new object[] {explode ? "." : ",", value});
            }

            return "." + SerializeLiteral(value);
        }

        private static string SerializeMatrix<T>(string name, T value, bool explode)
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

            if (IsEnumerable(typeof(T), out Type? itemType))
            {
                MethodInfo joinList = _joinListMethod.MakeGenericMethod(itemType);

                string prefix = $";{name}=";

                return prefix + (string)joinList.Invoke(null,
                    new object[] {explode ? prefix : ",", value});
            }

            return $";{name}={SerializeLiteral(value)}";
        }

        private static string SerializeLiteral<T>(T value) =>
            value != null
                ? value switch {
                    bool boolean => boolean ? "true" : "false",
                    _ => TypeDescriptor.GetConverter(typeof(T)).ConvertToString(value) ?? ""
                }
                : "";

        // ReSharper disable once UnusedMember.Local
        private static string JoinList<T>(string separator, IEnumerable<T> list) =>
            string.Join(separator, list
                .Select(SerializeLiteral));

        private static bool IsEnumerable(Type type,
            [NotNullWhen(true)] out Type? itemType)
        {
            itemType = null;

            var interfaceType = type.GetInterfaces()
                .FirstOrDefault(p => p.IsGenericType && p.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            if (interfaceType == null)
            {
                return false;
            }

            itemType = interfaceType.GetGenericArguments()[0];
            return true;
        }
    }
}
