using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema;

public abstract class SchemaGeneratorBase(
    ILocatedOpenApiElement<IOpenApiSchema> schemaElement,
    GenerationContext context,
    ITypeGenerator? parent)
    : TypeGeneratorBase<IOpenApiSchema>(schemaElement, context, parent)
{
    protected IOpenApiSchema Schema => Element.Element;

    protected abstract NameKind NameKind { get; }

    /// <inheritdoc />
    public override QualifiedNameSyntax? GetTypeName()
    {
        if (Element.IsRoot)
        {
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);
            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind);

            return QualifiedName(ns, IdentifierName(formatter.Format(Element.Key)));
        }

        if (Schema is IOpenApiReferenceHolder)
        {
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind);

            return QualifiedName(ns, IdentifierName(formatter.Format(Schema.GetReferenceId()!)));
        }

        return Parent?.GetChildName(Element, NameKind);
    }

    /// <inheritdoc />
    protected override YardarmTypeInfo CreateTypeInfo()
    {
        var typeName = GetTypeName();
        if (typeName is null)
        {
            ThrowHelpers.ThrowInvalidOperationException(
                $"Unable to generate schema for '{Element.Key}', it has no parent or the parent did not provide a name.");
        }

        return new(typeName, NameKind);
    }

    public override QualifiedNameSyntax? GetChildName<TChild>(ILocatedOpenApiElement<TChild> child, NameKind nameKind) =>
        QualifiedName((NameSyntax)TypeInfo.Name, IdentifierName(
            Context.NameFormatterSelector.GetFormatter(nameKind).Format(child.Key + "-Model")));
}
