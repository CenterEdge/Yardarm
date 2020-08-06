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
    public abstract class SchemaGeneratorBase : ISchemaGenerator
    {
        protected LocatedOpenApiElement<OpenApiSchema> SchemaElement { get; }
        protected GenerationContext Context { get; }

        protected OpenApiSchema Schema => SchemaElement.Element;

        protected abstract NameKind NameKind { get; }

        protected SchemaGeneratorBase(LocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context)
        {
            SchemaElement = schemaElement ?? throw new ArgumentNullException(nameof(schemaElement));
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public virtual void Preprocess()
        {
        }

        public virtual TypeSyntax GetTypeName()
        {
            var formatter = Context.NameFormatterSelector.GetFormatter(NameKind);

            if (Schema.Reference != null)
            {
                var ns = Context.NamespaceProvider.GetSchemaNamespace(SchemaElement);

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(Schema.Reference.Id)));
            }

            var parent = SchemaElement.Parents[0];
            var parentName = Context.TypeNameGenerator.GetName(parent);

            if (SchemaElement.Key == "")
            {
                // This can occur for request bodies
                return parentName;
            }

            return SyntaxFactory.QualifiedName((QualifiedNameSyntax) parentName,
                SyntaxFactory.IdentifierName(Context.NameFormatterSelector.GetFormatter(NameKind.Class).Format(SchemaElement.Key + "Model")));
        }

        public virtual SyntaxTree? GenerateSyntaxTree()
        {
            var members = Generate().ToArray();
            if (members.Length == 0)
            {
                return null;
            }

            var classNameAndNamespace = (QualifiedNameSyntax)GetTypeName();

            NameSyntax ns = classNameAndNamespace.Left;

            return CSharpSyntaxTree.Create(SyntaxFactory.CompilationUnit()
                .AddMembers(
                    SyntaxFactory.NamespaceDeclaration(ns)
                        .AddMembers(members))
                .NormalizeWhitespace());
        }

        public abstract IEnumerable<MemberDeclarationSyntax> Generate();
    }
}
