using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RootNamespace.Authentication.Internal
{
    internal class SecuritySchemeSetRegistry<T>
    {
        private static readonly Dictionary<Type, PropertyInfo> _schemes =
            typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && typeof(IAuthenticator).IsAssignableFrom(p.PropertyType))
                .ToDictionary(
                    property => property.PropertyType,
                    property => property);

        // ReSharper disable once StaticMemberInGenericType
        private static readonly ConcurrentDictionary<Type, PropertyInfo[][]> _cache =
            new ConcurrentDictionary<Type, PropertyInfo[][]>();

        private readonly T _authenticators;

        public SecuritySchemeSetRegistry(T authenticators)
        {
            _authenticators = authenticators ?? throw new ArgumentNullException(nameof(authenticators));
        }

        public IAuthenticator? SelectAuthenticator(Type operationType)
        {
            if (operationType == null)
            {
                throw new ArgumentNullException(nameof(operationType));
            }

            return SelectAuthenticator(_cache.GetOrAdd(operationType, GetSecuritySchemeSets));
        }

        private static PropertyInfo[][] GetSecuritySchemeSets(Type operationType) =>
            operationType.GetCustomAttributes<SecuritySchemeSetAttribute>()
                .Select(set =>
                {
                    PropertyInfo?[] properties = new PropertyInfo?[set.SecuritySchemes.Length];

                    for (int i = 0; i < set.SecuritySchemes.Length; i++)
                    {
                        if (!_schemes.TryGetValue(set.SecuritySchemes[i], out PropertyInfo property))
                        {
                            return null;
                        }

                        properties[i] = property;
                    }

                    return properties;
                })
                .Where(p => p != null)
                .ToArray()!;

        private IAuthenticator? SelectAuthenticator(PropertyInfo[][] sets)
        {
            if (sets == null)
            {
                throw new ArgumentNullException(nameof(sets));
            }

            foreach (PropertyInfo[] set in sets)
            {
                IAuthenticator?[] selectedAuthenticators = new IAuthenticator?[set.Length];

                bool foundMatches = true;
                for (int i=0; i < set.Length; i++)
                {
                    selectedAuthenticators[i] = (IAuthenticator?)set[i].GetValue(_authenticators);
                    if (selectedAuthenticators[i] == null)
                    {
                        foundMatches = false;
                        break;
                    }
                }

                if (foundMatches)
                {
                    return selectedAuthenticators.Length == 1
                        ? selectedAuthenticators[0]
                        : new MultiAuthenticator(selectedAuthenticators!);
                }
            }

            return null;
        }
    }
}
