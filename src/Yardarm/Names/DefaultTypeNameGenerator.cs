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
        private readonly INameFormatterSelector _nameFormatterSelector;
        protected INamespaceProvider NamespaceProvider { get; }

        public DefaultTypeNameGenerator(INamespaceProvider namespaceProvider, INameFormatterSelector nameFormatterSelector)
        {
            _nameFormatterSelector = nameFormatterSelector ?? throw new ArgumentNullException(nameof(nameFormatterSelector));
            NamespaceProvider = namespaceProvider;
        }

        public virtual QualifiedNameSyntax GetName(IOpenApiReferenceable component, IEnumerable<OpenApiPathElement> parents, string key) =>
            component switch
            {
                OpenApiSchema schema => GetSchemaName(schema, parents, key),
                _ => throw new InvalidOperationException($"Invalid component type {component.GetType().FullName}")
            };

        protected virtual QualifiedNameSyntax GetSchemaName(OpenApiSchema schema, IEnumerable<OpenApiPathElement> parents, string key)
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

            return SyntaxFactory.QualifiedName(parentName,
                SyntaxFactory.IdentifierName(_nameFormatterSelector.GetFormatter(NameKind.Class).Format(key + "Model")));
        }
    }
}
