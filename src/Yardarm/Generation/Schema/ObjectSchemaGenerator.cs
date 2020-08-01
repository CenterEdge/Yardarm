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
    internal class ObjectSchemaGenerator : SchemaGeneratorBase
    {
        protected IList<ISchemaClassEnricher> ClassEnrichers { get; }
        protected IList<IPropertyEnricher> PropertyEnrichers { get; }

        protected override NameKind NameKind => NameKind.Class;

        public ObjectSchemaGenerator(INamespaceProvider namespaceProvider, ITypeNameGenerator typeNameGenerator,
            INameFormatterSelector nameFormatterSelector, ISchemaGeneratorFactory schemaGeneratorFactory,
            IEnumerable<ISchemaClassEnricher> classEnrichers, IEnumerable<IPropertyEnricher> propertyEnrichers)
            : base(namespaceProvider, typeNameGenerator, nameFormatterSelector, schemaGeneratorFactory)
        {
            ClassEnrichers = classEnrichers.ToArray();
            PropertyEnrichers = propertyEnrichers.ToArray();
        }

        public override SyntaxTree GenerateSyntaxTree(LocatedOpenApiElement<OpenApiSchema> element)
        {
            var classNameAndNamespace = (QualifiedNameSyntax)GetTypeName(element);

            var ns = classNameAndNamespace.Left;

            return CSharpSyntaxTree.Create(SyntaxFactory.CompilationUnit()
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(ns)
                        .AddMembers(Generate(element)))
                .NormalizeWhitespace());
        }

        public override MemberDeclarationSyntax Generate(LocatedOpenApiElement<OpenApiSchema> element)
        {
            var schema = element.Element;

            var classNameAndNamespace = (QualifiedNameSyntax)GetTypeName(element);

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
                    var propertyElement =
                        new LocatedOpenApiElement<OpenApiSchema>(property.Value, property.Key, newParents);

                    ISchemaGenerator generator = SchemaGeneratorFactory.Get(propertyElement);

                    // This isn't a schema reference, so the child property may require schema generation
                    return generator.Generate(propertyElement)!;
                })
                .Where(p => p != null)
                .ToArray();

            if (childSchemas.Length > 0)
            {
                declaration = declaration.AddMembers(childSchemas);
            }

            return declaration.Enrich(ClassEnrichers, element);
        }

        protected virtual MemberDeclarationSyntax CreateProperty(LocatedOpenApiElement<OpenApiSchema> element)
        {
            var propertyName = NameFormatterSelector.GetFormatter(NameKind.Property).Format(element.Key);

            var typeName = TypeNameGenerator.GetName(element);

            var property = SyntaxFactory.PropertyDeclaration(typeName, propertyName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)),
                    SyntaxFactory.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                        .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken)));

            return property.Enrich(PropertyEnrichers, element);
        }
    }
}
