using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;

namespace Yardarm.Generation.Schema
{
    internal class ObjectSchemaGenerator : ISchemaGenerator
    {
        private readonly ITypeNameGenerator _typeNameGenerator;
        private readonly INameFormatterSelector _nameFormatterSelector;
        private readonly ISchemaGeneratorFactory _schemaGeneratorFactory;

        public ObjectSchemaGenerator(ITypeNameGenerator typeNameGenerator, INameFormatterSelector nameFormatterSelector,
            ISchemaGeneratorFactory schemaGeneratorFactory)
        {
            _typeNameGenerator = typeNameGenerator ?? throw new ArgumentNullException(nameof(typeNameGenerator));
            _nameFormatterSelector =
                nameFormatterSelector ?? throw new ArgumentNullException(nameof(nameFormatterSelector));
            _schemaGeneratorFactory =
                schemaGeneratorFactory ?? throw new ArgumentNullException(nameof(schemaGeneratorFactory));
        }

        public SyntaxTree Generate(OpenApiSchema schema, string key)
        {
            if (!(_typeNameGenerator.GetName(schema, Enumerable.Empty<OpenApiPathElement>(), key) is QualifiedNameSyntax classNameAndNamespace))
            {
                throw new InvalidOperationException($"Name must be a {nameof(QualifiedNameSyntax)}.");
            }

            var ns = classNameAndNamespace.Left;

            return CSharpSyntaxTree.Create(SyntaxFactory.CompilationUnit()
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(ns)
                        .AddMembers(Generate(schema, Array.Empty<OpenApiPathElement>(), key))));
        }

        public MemberDeclarationSyntax Generate(OpenApiSchema schema, OpenApiPathElement[] parents, string key)
        {
            if (!(_typeNameGenerator.GetName(schema, parents, key) is QualifiedNameSyntax classNameAndNamespace))
            {
                throw new InvalidOperationException($"Name must be a {nameof(QualifiedNameSyntax)}.");
            }

            string className = classNameAndNamespace.Right.Identifier.Text;

            var newParents = parents.Push(new OpenApiPathElement(schema, key));

            ClassDeclarationSyntax? declaration = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(SyntaxFactory.ConstructorDeclaration(className)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block()))
                .AddMembers(schema.Properties.Select(p => CreateProperty(p.Key, p.Value, newParents)).ToArray());

            MemberDeclarationSyntax[] childSchemas = schema.Properties
                .Where(property => property.Value.Reference == null)
                .Select(property =>
                {
                    ISchemaGenerator generator = _schemaGeneratorFactory.Create(property.Key, property.Value);

                    // This isn't a schema reference, so the child property may require schema generation
                    return generator.Generate(property.Value, newParents, property.Key)!;
                })
                .Where(p => p != null)
                .ToArray();

            if (childSchemas.Length > 0)
            {
                declaration = declaration.AddMembers(childSchemas);
            }

            return declaration;
        }

        protected virtual MemberDeclarationSyntax CreateProperty(string name, OpenApiSchema type, OpenApiPathElement[] parents)
        {
            var propertyName = _nameFormatterSelector.GetFormatter(NameKind.Property).Format(name);

            var typeName = _typeNameGenerator.GetName(type, parents, name);

            return SyntaxFactory.PropertyDeclaration(typeName, propertyName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }
    }
}
