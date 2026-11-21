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
        public override QualifiedNameSyntax? GetTypeName() => null;

        protected override YardarmTypeInfo CreateTypeInfo() =>
            new(WellKnownTypes.System.Collections.Generic.DictionaryT.Name(
                    PredefinedType(Token(SyntaxKind.StringKeyword)),
                    GetValueGenerator().TypeInfo.Name),
                isGenerated: false);

        /// <inheritdoc />
        /// <remarks>
        /// The dictionary itself is not generated, but an inline value schema of a generated type,
        /// such as an object or enum, is emitted in its place.
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

        private ITypeGenerator GetValueGenerator() =>
            Context.TypeGeneratorRegistry.Get(GetValueSchema());

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
