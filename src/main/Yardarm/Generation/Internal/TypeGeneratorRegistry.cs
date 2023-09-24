using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Schema;
using Yardarm.Spec;

namespace Yardarm.Generation.Internal
{
    internal class TypeGeneratorRegistry : ITypeGeneratorRegistry
    {
        private static MethodInfo? s_getTypedMethod;

        private readonly IServiceProvider _serviceProvider;

        public TypeGeneratorRegistry(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            _serviceProvider = serviceProvider;
        }

        public ITypeGenerator Get(ILocatedOpenApiElement element, Type generatorCategory)
        {
            ArgumentNullException.ThrowIfNull(element);

            var getTypedMethod = s_getTypedMethod ??=
                ((Func<ILocatedOpenApiElement<OpenApiSchema>, ITypeGenerator>)Get<OpenApiSchema, SchemaGenerator>).GetMethodInfo()
                .GetGenericMethodDefinition();

            return (ITypeGenerator)getTypedMethod.MakeGenericMethod(element.ElementType, generatorCategory)
                .Invoke(this, new object[] {element})!;
        }

        public ITypeGenerator Get<T, TGeneratorCategory>(ILocatedOpenApiElement<T> element)
            where T : IOpenApiElement
        {
            return _serviceProvider.GetRequiredService<ITypeGeneratorRegistry<T, TGeneratorCategory>>().Get(element);
        }
    }
}
