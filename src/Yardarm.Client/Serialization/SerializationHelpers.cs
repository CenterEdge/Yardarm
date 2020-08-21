using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    internal static class SerializationHelpers
    {
        public static bool IsEnumerable(Type type,
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

        public static bool IsList(Type type,
            [NotNullWhen(true)] out Type? itemType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                itemType = type.GetGenericArguments()[0];
                return true;
            }

            itemType = null;
            return false;
        }
    }
}
