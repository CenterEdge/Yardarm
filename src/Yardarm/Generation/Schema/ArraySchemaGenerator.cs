using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema
{
    public class ArraySchemaGenerator : TypeGeneratorBase<OpenApiSchema>
    {
        protected OpenApiSchema Schema => Element.Element;

        public ArraySchemaGenerator(ILocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context,
            ITypeGenerator? parent)
            : base(schemaElement, context, parent)
        {
        }

        protected override YardarmTypeInfo GetTypeInfo()
        {
            TypeSyntax itemTypeName = Context.TypeGeneratorRegistry.Get(GetItemSchema()).TypeInfo.Name;

            return new YardarmTypeInfo(
                WellKnownTypes.System.Collections.Generic.ListT.Name(itemTypeName),
                isGenerated: false);
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            ILocatedOpenApiElement<OpenApiSchema> itemSchema = GetItemSchema();

            return itemSchema.Element.Reference is null
                ? Context.TypeGeneratorRegistry.Get(itemSchema).Generate()
                : Enumerable.Empty<MemberDeclarationSyntax>();
        }

        private ILocatedOpenApiElement<OpenApiSchema> GetItemSchema() =>
            Element.GetItemSchemaOrDefault();

        /// <summary>
        /// Namespace if the array is rooted.
        /// </summary>
        /// <returns></returns>
        protected override NameSyntax GetNamespace() => GetChildName(GetItemSchema(), NameKind.Enum).Left;

        public override QualifiedNameSyntax GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
        {
            QualifiedNameSyntax? name = Parent?.GetChildName(Element, nameKind);

            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(nameKind);

            if (name == null)
            {
                // At the components root, use the schema namespace
                return QualifiedName(
                    Context.NamespaceProvider.GetNamespace(Element),
                    IdentifierName(formatter.Format(Element.Key + "-Item")));

            }

            return name.WithRight(IdentifierName(
                formatter.Format(name.Right.Identifier.ValueText + "-Item")));
        }
    }
}
