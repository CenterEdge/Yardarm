using System;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Spec;

namespace Yardarm.Generation.Internal;

internal class TypeGeneratorRegistry(IServiceProvider serviceProvider) : ITypeGeneratorRegistry
{
    private static MethodInfo? s_getTypedMethod;

    public ITypeGenerator Get(ILocatedOpenApiElement element, string? generatorCategory = null)
    {
        ArgumentNullException.ThrowIfNull(element);

        var getTypedMethod = s_getTypedMethod ??=
            ((Func<ILocatedOpenApiElement<OpenApiSchema>, string?, ITypeGenerator>)Get).GetMethodInfo()
            .GetGenericMethodDefinition();

        return (ITypeGenerator)getTypedMethod.MakeGenericMethod(element.ElementType)
            .Invoke(this, [element, generatorCategory])!;
    }

    public ITypeGenerator Get<T>(ILocatedOpenApiElement<T> element, string? generatorCategory = null)
        where T : IOpenApiElement
    {
        ITypeGeneratorRegistry<T> registry = serviceProvider.GetRequiredKeyedService<ITypeGeneratorRegistry<T>>(generatorCategory);

        return registry.Get(element);
    }
}
