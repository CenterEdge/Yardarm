using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;

namespace Yardarm.Generation.Schema
{
    public abstract class SchemaGeneratorBase : ISchemaGenerator
    {
        protected INamespaceProvider NamespaceProvider { get; }
        protected ITypeNameGenerator TypeNameGenerator { get; }
        protected INameFormatterSelector NameFormatterSelector { get; }
        protected ISchemaGeneratorFactory SchemaGeneratorFactory { get; }

        protected abstract NameKind NameKind { get; }

        protected SchemaGeneratorBase(INamespaceProvider namespaceProvider, ITypeNameGenerator typeNameGenerator,
            INameFormatterSelector nameFormatterSelector, ISchemaGeneratorFactory schemaGeneratorFactory)
        {
            NamespaceProvider = namespaceProvider;
            TypeNameGenerator = typeNameGenerator;
            NameFormatterSelector = nameFormatterSelector;
            SchemaGeneratorFactory = schemaGeneratorFactory;
        }

        public virtual TypeSyntax GetTypeName(LocatedOpenApiElement<OpenApiSchema> schemaElement)
        {
            var schema = schemaElement.Element;
            var formatter = NameFormatterSelector.GetFormatter(NameKind);

            if (schema.Reference != null)
            {
                var ns = NamespaceProvider.GetSchemaNamespace(schemaElement);

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(schema.Reference.Id)));
            }

            var parent = schemaElement.Parents[0];
            var parentName = TypeNameGenerator.GetName(parent);

            if (schemaElement.Key == "")
            {
                // This can occur for request bodies
                return parentName;
            }

            return SyntaxFactory.QualifiedName((QualifiedNameSyntax) parentName,
                SyntaxFactory.IdentifierName(NameFormatterSelector.GetFormatter(NameKind.Class).Format(schemaElement.Key + "Model")));
        }

        public virtual SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiSchema> element)
        {
            var members = Generate(element).ToArray();
            if (members.Length == 0)
            {
                return null;
            }

            var classNameAndNamespace = (QualifiedNameSyntax)GetTypeName(element);

            NameSyntax ns = classNameAndNamespace.Left;

            return CSharpSyntaxTree.Create(SyntaxFactory.CompilationUnit()
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(ns)
                        .AddMembers(members))
                .NormalizeWhitespace());
        }

        public abstract IEnumerable<MemberDeclarationSyntax> Generate(LocatedOpenApiElement<OpenApiSchema> element);
    }
}
