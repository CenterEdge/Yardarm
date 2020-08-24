using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    public abstract class SchemaGeneratorBase : TypeGeneratorBase<OpenApiSchema>
    {
        protected OpenApiSchema Schema => Element.Element;

        protected abstract NameKind NameKind { get; }

        protected SchemaGeneratorBase(ILocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context)
            : base(schemaElement, context)
        {
        }

        protected override TypeSyntax GetTypeName()
        {
            var formatter = Context.NameFormatterSelector.GetFormatter(NameKind);

            if (Schema.Reference != null)
            {
                var ns = Context.NamespaceProvider.GetNamespace(Element);

                return SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(Schema.Reference.Id)));
            }

            var parent = Element.Parent!;
            var parentName = Context.TypeNameProvider.GetName(parent);

            if (parent.Parents().OfType<ILocatedOpenApiElement<OpenApiRequestBody>>().Any())
            {
                // We just want to name this based on the request body, without appending SchemaModel
                return parentName;
            }

            return SyntaxFactory.QualifiedName((QualifiedNameSyntax) parentName,
                SyntaxFactory.IdentifierName(formatter.Format(Element.Key + "Model")));
        }
    }
}
