using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Names;

namespace Yardarm.Generation.Schema
{
    public class EnumSchemaGenerator : SchemaGeneratorBase
    {
        private static readonly NameSyntax _enumMemberName =
            SyntaxFactory.ParseName("System.Runtime.Serialization.EnumMember");

        protected override NameKind NameKind => NameKind.Enum;

        protected IList<IEnumEnricher> EnumEnrichers { get; }
        protected IList<IEnumMemberEnricher> EnumMemberEnrichers { get; }

        public EnumSchemaGenerator(INamespaceProvider namespaceProvider, ITypeNameGenerator typeNameGenerator,
            INameFormatterSelector nameFormatterSelector, ISchemaGeneratorFactory schemaGeneratorFactory,
            IEnumerable<IEnumEnricher> enumEnrichers, IEnumerable<IEnumMemberEnricher> enumMemberEnrichers)
            : base(namespaceProvider, typeNameGenerator, nameFormatterSelector, schemaGeneratorFactory)
        {
            EnumEnrichers = enumEnrichers.ToArray();
            EnumMemberEnrichers = enumMemberEnrichers.ToArray();
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate(LocatedOpenApiElement<OpenApiSchema> element)
        {
            var schema = element.Element;

            var fullName = (QualifiedNameSyntax) GetTypeName(element);

            string enumName = fullName.Right.Identifier.Text;

            INameFormatter memberNameFormatter = NameFormatterSelector.GetFormatter(NameKind.EnumMember);

            EnumDeclarationSyntax declaration = SyntaxFactory.EnumDeclaration(enumName)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(schema.Enum
                    .Select(p => CreateEnumMember(element, p, memberNameFormatter)!)
                    .Where(p => p != null)
                    .ToArray());

            yield return declaration.Enrich(EnumEnrichers, element);
        }

        protected virtual EnumMemberDeclarationSyntax? CreateEnumMember(
            LocatedOpenApiElement<OpenApiSchema> schemaElement,
            IOpenApiAny value,
            INameFormatter nameFormatter)
        {
            if (value.AnyType != AnyType.Primitive)
            {
                return null;
            }

            var primitive = (IOpenApiPrimitive) value;
            if (primitive.PrimitiveType != PrimitiveType.String)
            {
                return null;
            }

            var stringPrimitive = (OpenApiPrimitive<string>)primitive;

            string memberName = nameFormatter.Format(stringPrimitive.Value);

            var memberDeclaration = SyntaxFactory.EnumMemberDeclaration(memberName)
                .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                    CreateEnumMemberAttribute(stringPrimitive.Value)));

            return memberDeclaration.Enrich(EnumMemberEnrichers, (schemaElement, value));
        }

        protected static AttributeSyntax CreateEnumMemberAttribute(string value) =>
            SyntaxFactory.Attribute(_enumMemberName)
                .AddArgumentListArguments(
                    SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value)))
                        .WithNameEquals(SyntaxFactory.NameEquals("Value")));
    }
}
