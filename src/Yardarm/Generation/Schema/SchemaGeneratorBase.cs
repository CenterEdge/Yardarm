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
                var ns = NamespaceProvider.GetSchemaNamespace(schema);

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(schema.Reference.Id)));
            }

            var parent = schemaElement.Parents[0];
            var parentName = TypeNameGenerator.GetName(parent);

            return SyntaxFactory.QualifiedName((QualifiedNameSyntax) parentName,
                SyntaxFactory.IdentifierName(NameFormatterSelector.GetFormatter(NameKind.Class).Format(schemaElement.Key + "Model")));
        }

        public abstract SyntaxTree? GenerateSyntaxTree(LocatedOpenApiElement<OpenApiSchema> element);
        public abstract MemberDeclarationSyntax? Generate(LocatedOpenApiElement<OpenApiSchema> element);
    }
}
