using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Helpers;
using Yardarm.Names;

namespace Yardarm.Generation.Schema
{
    internal class ObjectSchemaGenerator : ISchemaGenerator
    {
        private readonly ITypeNameGenerator _typeNameGenerator;
        private readonly INameFormatterSelector _nameFormatterSelector;
        private readonly ISchemaGeneratorFactory _schemaGeneratorFactory;
        private readonly List<ISchemaClassEnricher> _classEnrichers;
        private readonly List<IPropertyEnricher> _propertyEnrichers;

        public ObjectSchemaGenerator(ITypeNameGenerator typeNameGenerator, INameFormatterSelector nameFormatterSelector,
            ISchemaGeneratorFactory schemaGeneratorFactory, IEnumerable<ISchemaClassEnricher> classEnrichers,
            IEnumerable<IPropertyEnricher> propertyEnrichers)
        {
            _typeNameGenerator = typeNameGenerator ?? throw new ArgumentNullException(nameof(typeNameGenerator));
            _nameFormatterSelector =
                nameFormatterSelector ?? throw new ArgumentNullException(nameof(nameFormatterSelector));
            _schemaGeneratorFactory =
                schemaGeneratorFactory ?? throw new ArgumentNullException(nameof(schemaGeneratorFactory));
            _classEnrichers = classEnrichers.ToList();
            _propertyEnrichers = propertyEnrichers.ToList();
        }

        public SyntaxTree Generate(OpenApiSchema schema, string key)
        {
            var element = new LocatedOpenApiElement<OpenApiSchema>(schema, key);

            if (!(_typeNameGenerator.GetName(element) is QualifiedNameSyntax classNameAndNamespace))
            {
                throw new InvalidOperationException($"Name must be a {nameof(QualifiedNameSyntax)}.");
            }

            var ns = classNameAndNamespace.Left;

            return CSharpSyntaxTree.Create(SyntaxFactory.CompilationUnit()
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(ns)
                        .AddMembers(Generate(element))));
        }

        public MemberDeclarationSyntax Generate(LocatedOpenApiElement element)
        {
            if (!(element is LocatedOpenApiElement<OpenApiSchema> schemaElement))
            {
                throw new ArgumentException("LocatedOpenApiElement must contain an OpenApiSchema");
            }

            var schema = schemaElement.Element;

            if (!(_typeNameGenerator.GetName(element) is QualifiedNameSyntax classNameAndNamespace))
            {
                throw new InvalidOperationException($"Name must be a {nameof(QualifiedNameSyntax)}.");
            }

            string className = classNameAndNamespace.Right.Identifier.Text;

            var newParents = element.Parents.Push(element);

            ClassDeclarationSyntax declaration = SyntaxFactory.ClassDeclaration(className)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(SyntaxFactory.ConstructorDeclaration(className)
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                    .WithBody(SyntaxFactory.Block()))
                .AddMembers(schema.Properties
                    .Select(p => CreateProperty(new LocatedOpenApiElement<OpenApiSchema>(p.Value, p.Key, newParents))).ToArray());

            MemberDeclarationSyntax[] childSchemas = schema.Properties
                .Where(property => property.Value.Reference == null)
                .Select(property =>
                {
                    ISchemaGenerator generator = _schemaGeneratorFactory.Create(property.Key, property.Value);

                    // This isn't a schema reference, so the child property may require schema generation
                    return generator.Generate(new LocatedOpenApiElement<OpenApiSchema>(property.Value, property.Key, newParents))!;
                })
                .Where(p => p != null)
                .ToArray();

            if (childSchemas.Length > 0)
            {
                declaration = declaration.AddMembers(childSchemas);
            }

            declaration = _classEnrichers.Aggregate(declaration, (p, enricher) => enricher.Enrich(p, schemaElement));

            return declaration;
        }

        protected virtual MemberDeclarationSyntax CreateProperty(LocatedOpenApiElement<OpenApiSchema> element)
        {
            var propertyName = _nameFormatterSelector.GetFormatter(NameKind.Property).Format(element.Key);

            var typeName = _typeNameGenerator.GetName(element);

            var property = SyntaxFactory.PropertyDeclaration(typeName, propertyName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            return _propertyEnrichers.Aggregate(property, (p, enricher) => enricher.Enrich(p, element));
        }
    }
}
