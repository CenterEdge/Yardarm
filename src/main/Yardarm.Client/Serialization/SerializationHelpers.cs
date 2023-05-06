using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Serialization
{
    internal static class SerializationHelpers
    {
        public static bool IsEnumerable(
            [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] Type type,
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
