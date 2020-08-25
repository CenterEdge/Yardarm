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

            if (Schema.Reference is null && !(parent is ILocatedOpenApiElement<OpenApiSchema>) &&
                parent.Parents().OfType<ILocatedOpenApiElement<OpenApiRequestBody>>().Any())
            {
                // We just want to name this based on the request body, without appending SchemaModel, if the immediate
                // parent is NOT a schema but we're within a request body. If the immediate parent is a schema, we're nested
                // further and still need to apply naming rules from the parent's key
                return parentName;
            }

            return SyntaxFactory.QualifiedName((QualifiedNameSyntax) parentName,
                SyntaxFactory.IdentifierName(formatter.Format(Element.Key + "Model")));
        }
    }
}
