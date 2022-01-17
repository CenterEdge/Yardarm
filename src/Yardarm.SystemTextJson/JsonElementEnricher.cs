﻿using System;
using System.Linq;
using System.Text.Json;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.SystemTextJson.Helpers;

namespace Yardarm.SystemTextJson
{
    /// <summary>
    /// Converts schemas which use dynamic types to use <see cref="JsonElement"/> instead.
    /// </summary>
    /// <remarks>
    /// This is done because System.Text.Json doesn't currently support dynamic types. This could have unwanted side effects
    /// in the future if we're mixing JSON and non-JSON content types using the same schema. But we don't do that now so we'll
    /// cross that bridge when we come to it.
    /// </remarks>
    public class JsonElementEnricher : IOpenApiSyntaxNodeEnricher<CompilationUnitSyntax, OpenApiSchema>
    {
        public CompilationUnitSyntax Enrich(CompilationUnitSyntax target,
            OpenApiEnrichmentContext<OpenApiSchema> context)
        {
            var dynamicTypes = target
                .DescendantNodes(p =>
                    p is not QualifiedNameSyntax // Don't look inside qualified names, they can't be dynamic
                    && p is not BlockSyntax // Don't look inside methods
                    && p is not ArrowExpressionClauseSyntax)
                .OfType<IdentifierNameSyntax>()
                .Where(p => p.Identifier.ValueText == "dynamic")
                .Select(p => p.Parent is NullableTypeSyntax nullableTypeSyntax ? nullableTypeSyntax : (TypeSyntax) p)
                .ToArray();

            if (dynamicTypes.Length == 0)
            {
                return target;
            }

            return target.ReplaceNodes(
                dynamicTypes,
                (_, _) => SystemTextJsonTypes.JsonElement);
        }
    }
}
