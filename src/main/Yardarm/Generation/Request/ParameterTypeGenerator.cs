using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request
{
    public class ParameterTypeGenerator : TypeGeneratorBase<OpenApiParameter>
    {
        public ParameterTypeGenerator(ILocatedOpenApiElement<OpenApiParameter> element, GenerationContext context,
            ITypeGenerator? parent) : base(element, context, parent)
        {
        }

        protected override YardarmTypeInfo GetTypeInfo()
        {
            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            return new YardarmTypeInfo(QualifiedName(ns, IdentifierName(formatter.Format(Element.Key))));
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            string className = ((QualifiedNameSyntax)TypeInfo.Name).Right.Identifier.ValueText;

            var schema = Element.GetSchemaOrDefault();
            if (schema.IsReference)
            {
                // References don't generate members
                return [];
            }

            var members = Context.TypeGeneratorRegistry.Get(schema).Generate().ToArray();
            if (members.Length == 0)
            {
                // If there are no nested schemas we don't need the parent parameter class either
                return [];
            }

            ClassDeclarationSyntax declaration = ClassDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                Identifier(className),
                typeParameterList: default,
                baseList: default,
                constraintClauses: default,
                members: List(members))
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddGeneratorAnnotation(this);

            return [declaration];
        }

        public override QualifiedNameSyntax GetChildName<TChild>(ILocatedOpenApiElement<TChild> child,
            NameKind nameKind) =>
            QualifiedName((NameSyntax)TypeInfo.Name, IdentifierName(
                Context.NameFormatterSelector.GetFormatter(nameKind).Format(child.Key + "-Model")));
    }
}
