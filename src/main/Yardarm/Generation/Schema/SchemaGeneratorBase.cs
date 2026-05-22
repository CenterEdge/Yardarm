using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema
{
    public abstract class SchemaGeneratorBase : TypeGeneratorBase<IOpenApiSchema>
    {
        protected IOpenApiSchema Schema => Element.Element;

        protected abstract NameKind NameKind { get; }

        protected SchemaGeneratorBase(ILocatedOpenApiElement<IOpenApiSchema> schemaElement, GenerationContext context,
            ITypeGenerator? parent)
            : base(schemaElement, context, parent)
        {
        }

        protected override YardarmTypeInfo GetTypeInfo()
        {
            var referenceId = Schema.GetReferenceId();
            if (referenceId != null)
            {
                NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

                var formatter = Context.NameFormatterSelector.GetFormatter(NameKind);

                return new YardarmTypeInfo(
                    SyntaxFactory.QualifiedName(ns,
                        SyntaxFactory.IdentifierName(formatter.Format(referenceId))),
                    NameKind);
            }

            if (Parent == null)
            {
                throw new InvalidOperationException(
                    $"Unable to generate schema for '{Element.Key}', it has no parent is not a component.");
            }

            QualifiedNameSyntax? name = Parent.GetChildName(Element, NameKind);
            if (name == null)
            {
                throw new InvalidOperationException($"Unable to generate schema for '{Element.Key}', parent did not provide a name.");
            }

            return new YardarmTypeInfo(name, NameKind);
        }

        public override QualifiedNameSyntax? GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind) =>
            QualifiedName((NameSyntax)TypeInfo.Name, IdentifierName(
                Context.NameFormatterSelector.GetFormatter(nameKind).Format(child.Key + "-Model")));
    }
}
