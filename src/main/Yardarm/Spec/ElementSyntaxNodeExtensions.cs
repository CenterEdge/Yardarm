using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;

namespace Yardarm.Spec
{
    public static class ElementSyntaxNodeExtensions
    {
        private static readonly Dictionary<string, Type> _annotationTypes = typeof(OpenApiDocument)
            .Assembly.GetExportedTypes()
            .Where(p => p.IsClass && !p.IsAbstract && p.Namespace == "Microsoft.OpenApi.Models" &&
                        typeof(IOpenApiElement).IsAssignableFrom(p))
            .ToDictionary(
                p => p.Name,
                p => p);

        private static readonly MethodInfo _tryGetMethod =
            typeof(IOpenApiElementRegistry).GetMethod(nameof(IOpenApiElementRegistry.TryGet))!;

        extension<TSyntaxNode>(TSyntaxNode node)
            where TSyntaxNode : SyntaxNode
        {
            public TSyntaxNode AddElementAnnotation<TElement>(
                ILocatedOpenApiElement<TElement> element, IOpenApiElementRegistry elementRegistry)
                where TElement : IOpenApiElement =>
                node.WithAdditionalAnnotations(
                    new SyntaxAnnotation(typeof(TElement).Name, elementRegistry.Add(element)));
        }

        extension(SyntaxNode node)
        {
            public ILocatedOpenApiElement<TElement>? GetElementAnnotation<TElement>(IOpenApiElementRegistry elementRegistry)
                where TElement : IOpenApiElement
            {
                string? key = node.GetAnnotations(typeof(TElement).Name).FirstOrDefault()?.Data;
                if (key == null)
                {
                    return null;
                }

                return elementRegistry.TryGet<TElement>(key, out var element)
                    ? element
                    : null;
            }

            public IEnumerable<ILocatedOpenApiElement> GetElementAnnotations(IOpenApiElementRegistry elementRegistry)
            {
                foreach (SyntaxAnnotation annotation in node.GetAnnotations(_annotationTypes.Keys)
                    .Where(p => p.Data != null))
                {
                    Type elementType = _annotationTypes[annotation.Kind!];

                    MethodInfo typedMethod = _tryGetMethod.MakeGenericMethod(elementType);

                    var parameters = new object?[] { annotation.Data!, null };

                    if ((bool)typedMethod.Invoke(elementRegistry, parameters)!)
                    {
                        yield return (ILocatedOpenApiElement)parameters[1]!;
                    }
                }
            }
        }
    }
}
