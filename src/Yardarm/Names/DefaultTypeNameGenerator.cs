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

        public virtual TypeSyntax GetName(LocatedOpenApiElement element) =>
            element switch
            {
                LocatedOpenApiElement<OpenApiSchema> schemaElement => GetSchemaName(schemaElement),
                _ => throw new InvalidOperationException($"Invalid component type {element.Element.GetType().FullName}")
            };

        protected virtual TypeSyntax GetSchemaName(LocatedOpenApiElement<OpenApiSchema> element) =>
            element.Element.Type switch
            {
                "object" => GetObjectSchemaName(element),
                "string" => String,
                "number" => Int,
                "array" => String, // TODO: Arrays
                _ => throw new InvalidOperationException($"Unknown schema type {element.Element.Type}")
            };

        protected virtual QualifiedNameSyntax GetObjectSchemaName(LocatedOpenApiElement<OpenApiSchema> schemaElement)
        {
            var schema = schemaElement.Element;
            var formatter = _nameFormatterSelector.GetFormatter(NameKind.Class);

            if (schema.Reference != null)
            {
                var ns = NamespaceProvider.GetSchemaNamespace(schema);

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(schema.Reference.Id)));
            }

            var parent = schemaElement.Parents[0];
            var parentName = GetName(parent);

            return SyntaxFactory.QualifiedName((QualifiedNameSyntax) parentName,
                SyntaxFactory.IdentifierName(_nameFormatterSelector.GetFormatter(NameKind.Class).Format(schemaElement.Key + "Model")));
        }
    }
}
