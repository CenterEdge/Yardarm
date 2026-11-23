using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema
{
    public class DictionarySchemaGenerator(
        ILocatedOpenApiElement<IOpenApiSchema> schemaElement,
        GenerationContext context,
        ITypeGenerator? parent)
        : TypeGeneratorBase<IOpenApiSchema>(schemaElement, context, parent)
    {
        /// <inheritdoc />
        /// <remarks>
        /// The dictionary is not a generated type, so it has no name. An inline value type is named by its own
        /// generator through <see cref="GetChildName{TChild}"/>.
        /// </remarks>
        public override QualifiedNameSyntax? GetTypeName() => null;

        protected override YardarmTypeInfo CreateTypeInfo()
        {
            ILocatedOpenApiElement<IOpenApiSchema> valueSchema = GetValueSchema();

            TypeSyntax valueType = Context.TypeGeneratorRegistry.Get(valueSchema).TypeInfo.Name;
            if (valueSchema.Element.Nullable)
            {
                // Matches typed AdditionalProperties, so JSON null values deserialize
                valueType = valueType.MakeNullable();
            }

            return new YardarmTypeInfo(
                WellKnownTypes.System.Collections.Generic.DictionaryT.Name(
                    PredefinedType(Token(SyntaxKind.StringKeyword)),
                    valueType),
                isGenerated: false);
        }

        /// <inheritdoc />
        /// <remarks>
        /// An inline value type is emitted by its own generator, in the namespace and file given by its own name.
        /// </remarks>
        public override SyntaxTree? GenerateSyntaxTree() =>
            GetGeneratedValueGenerator()?.GenerateSyntaxTree();

        public override IEnumerable<MemberDeclarationSyntax> Generate() =>
            GetGeneratedValueGenerator()?.Generate() ?? [];

        public override QualifiedNameSyntax GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
        {
            QualifiedNameSyntax? name = Parent?.GetChildName(Element, nameKind);

            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(nameKind);

            if (name == null)
            {
                // At the components root, use the schema namespace
                return QualifiedName(
                    Context.NamespaceProvider.GetNamespace(Element),
                    IdentifierName(formatter.Format(Element.Key + "-Value")));
            }

            return name.WithRight(IdentifierName(
                formatter.Format(name.Right.Identifier.ValueText + "-Value")));
        }

        private ILocatedOpenApiElement<IOpenApiSchema> GetValueSchema() =>
            Element.GetAdditionalPropertiesOrDefault();

        // Referenced value schemas are generated at their own location. Inline value schemas of
        // types that aren't generated, such as strings, emit no declarations.
        private ITypeGenerator? GetGeneratedValueGenerator()
        {
            ILocatedOpenApiElement<IOpenApiSchema> valueSchema = GetValueSchema();

            return valueSchema.Element is not IOpenApiReferenceHolder
                ? Context.TypeGeneratorRegistry.Get(valueSchema)
                : null;
        }
    }
}
