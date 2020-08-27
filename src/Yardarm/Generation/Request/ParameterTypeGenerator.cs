using System;
using System.Collections.Generic;
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

        protected override YardarmTypeInfo GetTypeInfo() => throw new NotImplementedException();

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
                // At the components root, use the parameters namespace
                return QualifiedName(
                    GetNamespace(),
                    IdentifierName(formatter.Format(Element.Element.Name)));

            }

            return name;
        }
    }
}
