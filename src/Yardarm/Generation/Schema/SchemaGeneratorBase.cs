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

        protected override YardarmTypeInfo GetTypeInfo()
        {
            var formatter = Context.NameFormatterSelector.GetFormatter(NameKind);

            if (Schema.Reference != null)
            {
                var ns = Context.NamespaceProvider.GetNamespace(Element);

                return new YardarmTypeInfo(
                    SyntaxFactory.QualifiedName(ns,
                        SyntaxFactory.IdentifierName(formatter.Format(Schema.Reference.Id))),
                    NameKind);
            }

            if (Element.Parent != null)
            {
                var parent = Element.Parent;
                var parentTypeInfo = Context.TypeGeneratorRegistry.Get(parent).TypeInfo;

                if (Schema.Reference is null && !(parent is ILocatedOpenApiElement<OpenApiSchema>) &&
                    parent.Parents().OfType<ILocatedOpenApiElement<OpenApiRequestBody>>().Any())
                {
                    // We just want to name this based on the request body, without appending SchemaModel, if the immediate
                    // parent is NOT a schema but we're within a request body. If the immediate parent is a schema, we're nested
                    // further and still need to apply naming rules from the parent's key
                    return parentTypeInfo;
                }

                return new YardarmTypeInfo(SyntaxFactory.QualifiedName((QualifiedNameSyntax)parentTypeInfo.Name,
                    SyntaxFactory.IdentifierName(formatter.Format(Element.Key + "Model"))),
                    NameKind);
            }
            else
            {
                // This is probably an array item from a components array schema. We don't have a parent class to put this in,
                // since we're using List<T> to hold the items.

                var ns = Context.NamespaceProvider.GetNamespace(Element);

                return new YardarmTypeInfo(SyntaxFactory.QualifiedName(ns,
                    SyntaxFactory.IdentifierName(formatter.Format(Element.Key))),
                    NameKind);
            }
        }
    }
}
