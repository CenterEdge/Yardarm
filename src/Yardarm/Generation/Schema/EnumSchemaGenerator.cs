using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.Schema
{
    public class EnumSchemaGenerator : SchemaGeneratorBase
    {
        private static readonly NameSyntax _enumMemberName =
            SyntaxFactory.ParseName("System.Runtime.Serialization.EnumMember");

        protected override NameKind NameKind => NameKind.Enum;

        public EnumSchemaGenerator(ILocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context,
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
            ILocatedOpenApiElement<OpenApiSchema> schemaElement,
            IOpenApiAny value,
            INameFormatter nameFormatter,
            NamingContext namingContext)
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

            string memberName = namingContext.RegisterName(nameFormatter.Format(stringPrimitive.Value));

            return SyntaxFactory.EnumMemberDeclaration(memberName)
                .AddAttributeLists(SyntaxFactory.AttributeList().AddAttributes(
                    CreateEnumMemberAttribute(stringPrimitive.Value))
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
