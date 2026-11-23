using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response;

public class HeaderTypeGenerator : TypeGeneratorBase<IOpenApiHeader>
{
    public HeaderTypeGenerator(ILocatedOpenApiElement<IOpenApiHeader> element, GenerationContext context,
        ITypeGenerator? parent) : base(element, context, parent)
    {
    }

    public override QualifiedNameSyntax? GetTypeName()
        => Context.TypeGeneratorRegistry.Get(Element.GetSchemaOrDefault()).GetTypeName();

    protected override YardarmTypeInfo CreateTypeInfo() =>
        Context.TypeGeneratorRegistry.Get(Element.GetSchemaOrDefault()).TypeInfo;

    public override SyntaxTree? GenerateSyntaxTree()
    {
        ILocatedOpenApiElement<IOpenApiSchema> schema = Element.GetSchemaOrDefault();
        ITypeGenerator schemaGenerator = Context.TypeGeneratorRegistry.Get(schema);

        if (schemaGenerator.GetTypeName() is null)
        {
            // A schema with no name of its own, such as a dictionary, emits any inline types it contains
            // in the files given by their own names. A referenced schema emits them at its own location.
            return schema.IsReference ? null : schemaGenerator.GenerateSyntaxTree();
        }

        return base.GenerateSyntaxTree();
    }

    public override IEnumerable<MemberDeclarationSyntax> Generate() =>
        Context.TypeGeneratorRegistry.Get(Element.GetSchemaOrDefault()).Generate();

    public override QualifiedNameSyntax GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind)
    {
        QualifiedNameSyntax? name = Parent?.GetChildName(Element, nameKind);

        INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(nameKind);

        if (name == null)
        {
            // At the components root, use the responses namespace
            return QualifiedName(
                Context.NamespaceProvider.GetNamespace(Element),
                IdentifierName(formatter.Format(Element.Key)));

        }

        return name;
    }
}
