using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    public class HeaderTypeGenerator : TypeGeneratorBase<OpenApiHeader>
    {
        public HeaderTypeGenerator(ILocatedOpenApiElement<OpenApiHeader> element, GenerationContext context,
            ITypeGenerator? parent) : base(element, context, parent)
        {
        }

        protected override YardarmTypeInfo GetTypeInfo() =>
            Context.TypeGeneratorRegistry.Get(Element.GetSchemaOrDefault()).TypeInfo;

        public override IEnumerable<MemberDeclarationSyntax> Generate() =>
            Context.TypeGeneratorRegistry.Get(Element.GetSchemaOrDefault()).Generate();

        /// <summary>
        /// Namespace if the parameter is in components.
        /// </summary>
        protected override NameSyntax GetNamespace() => Context.NamespaceProvider.GetNamespace(Element);

        public override QualifiedNameSyntax GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
        {
            QualifiedNameSyntax? name = Parent?.GetChildName(Element, nameKind);

            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(nameKind);

            if (name == null)
            {
                // At the components root, use the responses namespace
                return QualifiedName(
                    GetNamespace(),
                    IdentifierName(formatter.Format(Element.Key)));

            }

            return name;
        }
    }
}
