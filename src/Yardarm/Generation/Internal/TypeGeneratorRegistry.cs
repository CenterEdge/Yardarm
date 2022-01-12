using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Interfaces;
using Yardarm.Spec;

namespace Yardarm.Generation.Internal
{
    internal class TypeGeneratorRegistry : ITypeGeneratorRegistry
    {
        private static readonly MethodInfo _getTypedMethod = typeof(TypeGeneratorRegistry)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .Single(p => p.IsGenericMethod && p.Name == nameof(Get));

        private readonly IServiceProvider _serviceProvider;

        public TypeGeneratorRegistry(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public ITypeGenerator Get(ILocatedOpenApiElement element, Type generatorCategory)
        {
            if (element == null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            return (ITypeGenerator)_getTypedMethod.MakeGenericMethod(element.ElementType, generatorCategory)
                .Invoke(this, new object[] {element})!;
        }

        public ITypeGenerator Get<T, TGeneratorCategory>(ILocatedOpenApiElement<T> element)
            where T : IOpenApiElement
        {
            return _serviceProvider.GetRequiredService<ITypeGeneratorRegistry<T, TGeneratorCategory>>().Get(element);
        }
    }
}
