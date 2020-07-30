using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Interfaces;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Helpers;

namespace Yardarm.Names
{
    public class DefaultTypeNameGenerator : ITypeNameGenerator
    {
        public static TypeSyntax String { get; } =
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.StringKeyword));

        public static TypeSyntax Int { get; } =
            SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.IntKeyword));

        private readonly INameFormatterSelector _nameFormatterSelector;
        protected INamespaceProvider NamespaceProvider { get; }

        public DefaultTypeNameGenerator(INamespaceProvider namespaceProvider, INameFormatterSelector nameFormatterSelector)
        {
            _nameFormatterSelector = nameFormatterSelector ?? throw new ArgumentNullException(nameof(nameFormatterSelector));
            NamespaceProvider = namespaceProvider;
        }

        public virtual TypeSyntax GetName(IOpenApiReferenceable component, IEnumerable<OpenApiPathElement> parents, string key) =>
            component switch
            {
                OpenApiSchema schema => GetSchemaName(schema, parents, key),
                _ => throw new InvalidOperationException($"Invalid component type {component.GetType().FullName}")
            };

        protected virtual TypeSyntax GetSchemaName(OpenApiSchema schema, IEnumerable<OpenApiPathElement> parents,
            string key) =>
            schema.Type switch
            {
                "object" => GetObjectSchemaName(schema, parents, key),
                "string" => String,
                "number" => Int,
                "array" => String, // TODO: Arrays
                _ => throw new InvalidOperationException($"Unknown schema type {schema.Type}")
            };

        protected virtual QualifiedNameSyntax GetObjectSchemaName(OpenApiSchema schema, IEnumerable<OpenApiPathElement> parents, string key)
        {
            var formatter = _nameFormatterSelector.GetFormatter(NameKind.Class);

            if (schema.Reference != null)
            {
                var ns = NamespaceProvider.GetSchemaNamespace(schema);

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(schema.Reference.Id)));
            }

            var (parent, additionalParents) = parents.Pop();
            var parentName = GetName(parent.Parent, additionalParents, parent.Key);

            return SyntaxFactory.QualifiedName((QualifiedNameSyntax) parentName,
                SyntaxFactory.IdentifierName(_nameFormatterSelector.GetFormatter(NameKind.Class).Format(key + "Model")));
        }
    }
}
