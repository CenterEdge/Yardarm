using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema;

/// <summary>
/// Generates a readonly record struct that can contain any non-null string for an OpenAPI schema that has the "x-extensible-enum" extension.
/// Generates well-known values as static readonly properties. The generated type implements IExtensibleEnum{TSelf}" and
/// provides implicit conversions to and from string.
/// </summary>
internal class ExtensibleEnumSchemaGenerator(
    GenerationContext context,
    IRootNamespace rootNamespace,
    ILocatedOpenApiElement<IOpenApiSchema> schemaElement,
    ITypeGenerator? parent,
    List<string> wellKnownValues)
    : SchemaGeneratorBase(schemaElement, context, parent)
{
    protected override NameKind NameKind => NameKind.Class;

    public override IEnumerable<MemberDeclarationSyntax> Generate()
    {
        var classNameAndNamespace = GetTypeName();
        if (classNameAndNamespace is null)
        {
            Yardarm.Helpers.ThrowHelpers.ThrowInvalidOperationException(
                $"Unable to generate extensible enum for '{Element.Key}', no name was returned by GetTypeName.");
        }

        string className = classNameAndNamespace.Right.Identifier.Text;

        QualifiedNameSyntax literalsNamespace =
            QualifiedName(QualifiedName(rootNamespace.Name, IdentifierName("Serialization")), IdentifierName("Literals"));

        NameSyntax interfaceName = QualifiedName(QualifiedName(rootNamespace.Name, IdentifierName("Models")),
            GenericName(
                Identifier("IExtensibleEnum"),
                TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(className)))));

        NameSyntax converterName = QualifiedName(QualifiedName(literalsNamespace, IdentifierName("Converters")),
            GenericName(
                Identifier("ExtensibleEnumLiteralConverter"),
                TypeArgumentList(SingletonSeparatedList<TypeSyntax>(IdentifierName(className)))));

        yield return RecordDeclaration(
            kind: SyntaxKind.RecordStructDeclaration,
            attributeLists: SingletonList(AttributeList(SingletonSeparatedList(
                Attribute(
                    QualifiedName(literalsNamespace, IdentifierName("LiteralConverter")),
                    AttributeArgumentList(SingletonSeparatedList(AttributeArgument(TypeOfExpression(converterName)))))))),
            modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword)),
            keyword: Token(SyntaxKind.RecordKeyword),
            classOrStructKeyword: Token(SyntaxKind.StructKeyword),
            identifier: Identifier(className),
            typeParameterList: null,
            parameterList: ParameterList(SingletonSeparatedList(
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: PredefinedType(Token(SyntaxKind.StringKeyword)),
                    identifier: Identifier("Value"),
                    @default: null))),
            baseList: BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(interfaceName))),
            constraintClauses: default,
            openBraceToken: Token(SyntaxKind.OpenBraceToken),
            members: List([
                ..GetConstantDeclarations(className),
                ..GetHelperMethods(className, interfaceName)
            ]),
            closeBraceToken: Token(SyntaxKind.CloseBraceToken),
            semicolonToken: default)
            .AddGeneratorAnnotation(this)
            .AddElementAnnotation(Element, Context.ElementRegistry);
    }

    private IEnumerable<MemberDeclarationSyntax> GetConstantDeclarations(string className)
    {
        var nameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.EnumMember);

        foreach (string value in wellKnownValues)
        {
            yield return FieldDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword), Token(SyntaxKind.ReadOnlyKeyword)),
                declaration: VariableDeclaration(
                    IdentifierName(className),
                    SingletonSeparatedList(
                        VariableDeclarator(
                            Identifier(nameFormatter.Format(value)),
                            argumentList: null,
                            initializer: EqualsValueClause(ImplicitObjectCreationExpression(
                                argumentList: ArgumentList(SingletonSeparatedList(
                                    Argument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value))))),
                                initializer: null))))));
        }
    }

    private IEnumerable<MemberDeclarationSyntax> GetHelperMethods(string className, NameSyntax interfaceName)
    {
        // Override ToString() to return the Value property
        yield return MethodDeclaration(
            attributeLists: default,
            modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)),
            returnType: PredefinedType(Token(SyntaxKind.StringKeyword)),
            explicitInterfaceSpecifier: default,
            identifier: Identifier("ToString"),
            typeParameterList: default,
            parameterList: ParameterList(),
            constraintClauses: default,
            body: null,
            expressionBody: ArrowExpressionClause(IdentifierName("Value")),
            semicolonToken: Token(SyntaxKind.SemicolonToken));

        // Implicit conversion to string
        yield return ConversionOperatorDeclaration(
            attributeLists: default,
            modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
            implicitOrExplicitKeyword: Token(SyntaxKind.ImplicitKeyword),
            operatorKeyword: Token(SyntaxKind.OperatorKeyword),
            type: PredefinedType(Token(SyntaxKind.StringKeyword)),
            parameterList: ParameterList(SingletonSeparatedList(
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: IdentifierName(className),
                    identifier: Identifier("value"),
                    @default: null))),
            body: null,
            expressionBody: ArrowExpressionClause(MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("value"),
                IdentifierName("Value"))),
            semicolonToken: Token(SyntaxKind.SemicolonToken));

        // Implicit conversion from string
        yield return ConversionOperatorDeclaration(
            attributeLists: default,
            modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
            implicitOrExplicitKeyword: Token(SyntaxKind.ImplicitKeyword),
            operatorKeyword: Token(SyntaxKind.OperatorKeyword),
            type: IdentifierName(className),
            parameterList: ParameterList(SingletonSeparatedList(
                Parameter(
                    attributeLists: default,
                    modifiers: default,
                    type: PredefinedType(Token(SyntaxKind.StringKeyword)),
                    identifier: Identifier("value"),
                    @default: null))),
            body: Block(List([
                MethodHelpers.ThrowIfArgumentNull("value"),
                ReturnStatement(ImplicitObjectCreationExpression(
                    argumentList: ArgumentList(SingletonSeparatedList(
                        Argument(IdentifierName("value")))),
                    initializer: null))
            ])),
            expressionBody: null,
            semicolonToken: default);

        // Static create method to satisfy the IExtensibleEnum<T> interface requirement
        if (Context.CurrentTargetFramework.Version.Major >= 7)
        {
            yield return MethodDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.StaticKeyword)),
                returnType: IdentifierName(className),
                explicitInterfaceSpecifier: ExplicitInterfaceSpecifier(interfaceName),
                identifier: Identifier("Create"),
                typeParameterList: default,
                parameterList: ParameterList(SingletonSeparatedList(
                    Parameter(
                        attributeLists: default,
                        modifiers: default,
                        type: PredefinedType(Token(SyntaxKind.StringKeyword)),
                        identifier: Identifier("value"),
                        @default: null))),
                constraintClauses: default,
                body: Block(List([
                MethodHelpers.ThrowIfArgumentNull("value"),
                ReturnStatement(ImplicitObjectCreationExpression(
                    argumentList: ArgumentList(SingletonSeparatedList(
                        Argument(IdentifierName("value")))),
                    initializer: null))
                ])),
                expressionBody: null,
                semicolonToken: default);
        }
    }
}
