using System;
using System.Collections.Generic;
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

    /// <inheritdoc />
    /// <remarks>
    /// The schema's type name, or the header's own name when the schema has none, such as a dictionary.
    /// This supplies the namespace and file path for any inline types the schema emits.
    /// </remarks>
    public override QualifiedNameSyntax GetTypeName()
    {
        ILocatedOpenApiElement<IOpenApiSchema> schema = Element.GetSchemaOrDefault();

        return Context.TypeGeneratorRegistry.Get(schema).GetTypeName()
            ?? GetChildName(schema, NameKind.Class);
    }

    protected override YardarmTypeInfo CreateTypeInfo() =>
        Context.TypeGeneratorRegistry.Get(Element.GetSchemaOrDefault()).TypeInfo;

    public override IEnumerable<MemberDeclarationSyntax> Generate()
    {
        ILocatedOpenApiElement<IOpenApiSchema> schema = Element.GetSchemaOrDefault();

        // A referenced schema is generated at its own location
        return schema.IsReference
            ? []
            : Context.TypeGeneratorRegistry.Get(schema).Generate();
    }

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
