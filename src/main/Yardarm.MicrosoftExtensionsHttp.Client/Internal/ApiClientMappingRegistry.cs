using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.DependencyInjection;

namespace RootNamespace.Internal
{
    // Internal tracking for HTTP Client configuration. This is used to support AddAllOtherApis by
    // tracking what APIs have been registered previously. The HTTP client factory only prevents registering
    // the same interface twice if the implementation is different. We need to be stricter because we don't want
    // to add the same default configurators more than once for the same API. It's bad for performance, and depending
    // on registration order may unintentionally override API specific configuration that was applied elsewhere.
    internal static class ApiClientMappingRegistry
    {
        private static readonly ConditionalWeakTable<IServiceCollection, HashSet<Type>> s_conditionalWeakTable = new();

        // Attempts to reserve the interface type, returns true if it was successful or false if it was already reserved.
        public static bool TryReserve(IServiceCollection serviceCollection, Type type)
        {
            var set = s_conditionalWeakTable.GetOrCreateValue(serviceCollection);

            return set.Add(type);
        }
    }
}
