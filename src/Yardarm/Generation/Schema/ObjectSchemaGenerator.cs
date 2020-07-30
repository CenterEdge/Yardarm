using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
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
            var classNameAndNamespace = _typeNameGenerator.GetName(schema, Enumerable.Empty<OpenApiPathElement>(), key);

            var ns = classNameAndNamespace.Left;

            return CSharpSyntaxTree.Create(SyntaxFactory.CompilationUnit()
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(ns)
                        .AddMembers(Generate(schema, Array.Empty<OpenApiPathElement>(), key))));
        }

        public MemberDeclarationSyntax Generate(OpenApiSchema schema, OpenApiPathElement[] parents, string key)
        {
            QualifiedNameSyntax classNameAndNamespace = _typeNameGenerator.GetName(schema, parents, key);

            string className = classNameAndNamespace.Right.Identifier.Text;

            ClassDeclarationSyntax? declaration = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(SyntaxFactory.ConstructorDeclaration(className)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block()))
                .AddMembers(schema.Properties.Select(p => CreateProperty(p.Key, p.Value)).ToArray());

            MemberDeclarationSyntax[] childSchemas = schema.Properties
                .Where(property => property.Value.Reference == null)
                .Select(property =>
                {
                    ISchemaGenerator generator = _schemaGeneratorFactory.Create(property.Key, property.Value);

                    var newParents = new OpenApiPathElement[parents.Length + 1];
                    newParents[0] = new OpenApiPathElement(schema, key);

                    if (parents.Length > 0)
                    {
                        parents.AsSpan().CopyTo(newParents.AsSpan(1));
                    }

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

        protected virtual MemberDeclarationSyntax CreateProperty(string name, OpenApiSchema type)
        {
            var propertyName = _nameFormatterSelector.GetFormatter(NameKind.Property).Format(name);

            return SyntaxFactory.PropertyDeclaration(SyntaxFactory.ParseTypeName("string"), propertyName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));
        }
    }
}
