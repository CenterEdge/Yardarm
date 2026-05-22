using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    public class EnumSchemaGenerator : SchemaGeneratorBase
    {
        private static readonly NameSyntax _enumMemberName =
            SyntaxFactory.ParseName("System.Runtime.Serialization.EnumMember");

        protected override NameKind NameKind => NameKind.Enum;

        public EnumSchemaGenerator(ILocatedOpenApiElement<IOpenApiSchema> schemaElement, GenerationContext context,
            ITypeGenerator? parent)
            : base(schemaElement, context, parent)
        {
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var fullName = (QualifiedNameSyntax) TypeInfo.Name;

            string enumName = fullName.Right.Identifier.Text;

            INameFormatter memberNameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.EnumMember);

            var namingContext = new NamingContext();

            yield return SyntaxFactory.EnumDeclaration(enumName)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(Schema.Enum
                    .Select(p => CreateEnumMember(Element, p, memberNameFormatter, namingContext)!)
                    .Where(p => p != null)
                    .ToArray());
        }

        protected virtual EnumMemberDeclarationSyntax? CreateEnumMember(
            ILocatedOpenApiElement<IOpenApiSchema> schemaElement,
            JsonNode? value,
            INameFormatter nameFormatter,
            NamingContext namingContext)
        {
            string? stringValue = value?.GetValue<string>();
            if (stringValue is null)
            {
                return null;
            }

            string memberName = namingContext.RegisterName(nameFormatter.Format(stringValue));

            return SyntaxFactory.EnumMemberDeclaration(memberName)
                .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                    CreateEnumMemberAttribute(stringValue))
                    .WithTrailingTrivia(SyntaxFactory.ElasticCarriageReturnLineFeed));
        }

        protected static AttributeSyntax CreateEnumMemberAttribute(string value) =>
            SyntaxFactory.Attribute(_enumMemberName)
                .AddArgumentListArguments(
                    SyntaxFactory.AttributeArgument(
                            SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(value)))
                        .WithNameEquals(SyntaxFactory.NameEquals("Value")));
    }
}
