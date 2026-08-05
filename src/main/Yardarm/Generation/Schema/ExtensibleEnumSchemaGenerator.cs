using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema;

/// <summary>
/// Generates a static class containing string constants for an OpenAPI schema that has the "x-extensible-enum" extension.
/// Generates well-known values as constants in a static class which is located and named using the
/// usual schema class naming techniques. The properties referencing this schema will be generated as a string.
/// </summary>
internal class ExtensibleEnumSchemaGenerator(
    ILocatedOpenApiElement<OpenApiSchema> schemaElement,
    GenerationContext context,
    ITypeGenerator? parent,
    List<string> wellKnownValues)
    : SchemaGeneratorBase(schemaElement, context, parent)
{
    protected override NameKind NameKind => NameKind.Class;

    protected override YardarmTypeInfo CreateTypeInfo() => new(
        PredefinedType(Token(SyntaxKind.StringKeyword)), isGenerated: false);

    public override IEnumerable<MemberDeclarationSyntax> Generate()
    {
        var classNameAndNamespace = GetTypeName();
        if (classNameAndNamespace is null)
        {
            Yardarm.Helpers.ThrowHelpers.ThrowInvalidOperationException(
                $"Unable to generate extensible enum for '{Element.Key}', no name was returned by GetTypeName.");
        }

        string className = classNameAndNamespace.Right.Identifier.Text;

        yield return ClassDeclaration(
            attributeLists: default,
            modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
            identifier: Identifier(className),
            typeParameterList: null,
            baseList: null,
            constraintClauses: default,
            members: List(GetConstantDeclarations()));
    }

    private IEnumerable<MemberDeclarationSyntax> GetConstantDeclarations()
    {
        var nameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.EnumMember);

        foreach (string value in wellKnownValues)
        {
            yield return FieldDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ConstKeyword)),
                declaration: VariableDeclaration(
                    PredefinedType(Token(SyntaxKind.StringKeyword)),
                    SeparatedList(
                    [
                        VariableDeclarator(
                            Identifier(nameFormatter.Format(value)),
                            argumentList: null,
                            initializer: EqualsValueClause(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value))))
                    ])));
        }
    }
}
