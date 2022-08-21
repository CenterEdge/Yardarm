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

        public static TSyntaxNode AddElementAnnotation<TSyntaxNode, TElement>(this TSyntaxNode node,
            ILocatedOpenApiElement<TElement> element, IOpenApiElementRegistry elementRegistry)
            where TSyntaxNode : SyntaxNode
            where TElement : IOpenApiElement =>
            node.WithAdditionalAnnotations(
                new SyntaxAnnotation(typeof(TElement).Name, elementRegistry.Add(element)));

        public static ILocatedOpenApiElement<TElement>? GetElementAnnotation<TElement>(this SyntaxNode node,
            IOpenApiElementRegistry elementRegistry)
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

        public static IEnumerable<ILocatedOpenApiElement> GetElementAnnotations(this SyntaxNode node,
            IOpenApiElementRegistry elementRegistry)
        {
            foreach (SyntaxAnnotation annotation in node.GetAnnotations(_annotationTypes.Keys)
                .Where(p => p.Data != null))
            {
                Type elementType = _annotationTypes[annotation.Kind!];

                MethodInfo typedMethod = _tryGetMethod.MakeGenericMethod(elementType);

                var parameters = new object?[] {annotation.Data!, null};

                if ((bool)typedMethod.Invoke(elementRegistry, parameters)!)
                {
                    yield return (ILocatedOpenApiElement)parameters[1]!;
                }
            }
        }
    }
}
