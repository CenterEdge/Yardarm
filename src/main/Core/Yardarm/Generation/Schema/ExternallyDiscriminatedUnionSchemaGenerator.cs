using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Schema;

/// <summary>
/// Generates a union where each case is discriminated based on a single property name
/// outside of the case itself.
/// </summary>
/// <remarks>
/// <para>
/// For example, <code>{"dog":{"breed":"Labrador"}}</code> or <code>{"cat":{"color":"black"}}</code>
/// where "dog" and "cat" discriminate the union case, and "breed" and "color" are the properties of the respective cases.
/// </para>
/// </remarks>
internal class ExternallyDiscriminatedUnionSchemaGenerator(
    ILocatedOpenApiElement<IOpenApiSchema> schemaElement,
    GenerationContext context,
    ITypeGenerator? parent,
    IRootNamespace rootNamespace)
    : SchemaGeneratorBase(schemaElement, context, parent)
{
    public const string UnknownCaseName = "__UnknownCase";

    protected override NameKind NameKind => NameKind.Struct;

    public override IEnumerable<MemberDeclarationSyntax> Generate()
    {
        var classNameAndNamespace = (QualifiedNameSyntax)TypeInfo.Name;

        string unionName = classNameAndNamespace.Right.Identifier.Text;
        SyntaxToken unionNameIdentifier = Identifier(unionName);

        // Hand roll the union, rather than using the C# 15 union keyword, so that we can add attributes to the constructors
        // to support deserialization.

        yield return StructDeclaration(
            attributeLists: SingletonList(AttributeList(SingletonSeparatedList(
                Attribute(WellKnownTypes.System.Runtime.CompilerServices.UnionAttribute)))),
            modifiers: TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.ReadOnlyKeyword)),
            keyword: Token(SyntaxKind.StructKeyword),
            identifier: Identifier(unionName),
            typeParameterList: null,
            baseList: BaseList(SingletonSeparatedList<BaseTypeSyntax>(
                SimpleBaseType(WellKnownTypes.System.Runtime.CompilerServices.IUnion))),
            constraintClauses: default,
            openBraceToken: Token(SyntaxKind.OpenBraceToken),
            members: List(GetUnionMembers(unionNameIdentifier)),
            closeBraceToken: Token(SyntaxKind.CloseBraceToken),
            semicolonToken: default)
            .AddElementAnnotation(Element, Context.ElementRegistry)
            .AddGeneratorAnnotation(this);
    }

    private IEnumerable<MemberDeclarationSyntax> GetUnionMembers(SyntaxToken unionNameIdentifier)
    {
        // IUnion.Value property
        yield return PropertyDeclaration(
            attributeLists: default,
            modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
            type: NullableType(PredefinedType(Token(SyntaxKind.ObjectKeyword))),
            explicitInterfaceSpecifier: null,
            identifier: Identifier("Value"),
            accessorList: AccessorList(SingletonList(AccessorDeclaration(
                SyntaxKind.GetAccessorDeclaration,
                attributeLists: default,
                modifiers: default,
                keyword: Token(SyntaxKind.GetKeyword),
                body: null,
                expressionBody: null,
                semicolonToken: Token(SyntaxKind.SemicolonToken)))));

        // Constructors for each union case
        foreach (var unionCase in Element.Element.AnyOf?
            .Select(p => {
                KeyValuePair<string, IOpenApiSchema> caseProperty = p.Properties!.First();

                return (caseProperty.Key, Element: LocatedOpenApiElement.CreateRoot(caseProperty.Value, caseProperty.Value.GetReferenceId()!));
            }) ?? [])
        {
            ITypeGenerator caseType = Context.TypeGeneratorRegistry.Get(unionCase.Element);

            yield return CreateUnionConstructor(unionNameIdentifier, caseType.TypeInfo.Name, unionCase.Key);
        }

        // Unknown case
        yield return CreateUnionConstructor(unionNameIdentifier,
            QualifiedName(QualifiedName(rootNamespace.Name, IdentifierName("Models")), IdentifierName("UnknownCase")));
    }

    private ConstructorDeclarationSyntax CreateUnionConstructor(SyntaxToken unionNameIdentifier, TypeSyntax caseType, string? caseName = null)
        => ConstructorDeclaration(
            attributeLists: caseName is not null
                ? SingletonList(
                    AttributeList(SingletonSeparatedList(
                        Attribute(
                            QualifiedName(QualifiedName(rootNamespace.Name, IdentifierName("Internal")), IdentifierName("UnionCaseName")),
                            AttributeArgumentList(SingletonSeparatedList(AttributeArgument(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(caseName))))))))
                    .WithTrailingTrivia(ElasticCarriageReturnLineFeed))
                : default,
            modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
            identifier: unionNameIdentifier,
            parameterList: ParameterList(SingletonSeparatedList(Parameter(
                attributeLists: default,
                modifiers: default,
                type: caseType,
                identifier: Identifier("value"),
                @default: null))),
            initializer: null,
            body: null,
            expressionBody: ArrowExpressionClause(AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                IdentifierName("Value"),
                IdentifierName("value"))),
            semicolonToken: Token(SyntaxKind.SemicolonToken));

    /// <summary>
    /// Returns true if the given schema is eligible to be generated as a property discriminated union.
    /// </summary>
    /// <remarks>
    /// To be eligible, the schema must be anyOf-based, with no other source of properties, and each anyOf schema must be an object
    /// with a single required property. The name and type of the property must be unique across all anyOf schemas, and the type of the
    /// property must be a reference to a component schema (nested schemas are not supported).
    /// </remarks>
    public static bool IsEligible(ILocatedOpenApiElement<IOpenApiSchema> schema, ITypeGeneratorRegistry typeGeneratorRegistry)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(typeGeneratorRegistry);

        if (schema.Element.AnyOf is not { Count: > 0 }
            || schema.Element.OneOf is { Count: > 0 }
            || schema.Element.AllOf is { Count: > 0 }
            || schema.Element.Properties is { Count: > 0 }
            || (schema.Element.Type.HasValue && !schema.Element.IsType(JsonSchemaType.Object))
            || schema.Element.AdditionalProperties is not null)
        {
            return false;
        }

        var propertyNames = new HashSet<string>();

        // Can't use a HashSet here because the duplicate check we need to perform is more complex and the
        // hash code won't necessarily match between two matching TypeSyntax objects.
        var propertyTypes = new List<TypeSyntax>();

        foreach (IOpenApiSchema unionCase in schema.Element.AnyOf ?? [])
        {
            if ((unionCase.Properties?.Count ?? 0) != 1
                || (unionCase.Type.HasValue && !unionCase.IsType(JsonSchemaType.Object))
                || unionCase.Nullable
                || unionCase.AdditionalProperties is not null)
            {
                // Not a single property object
                return false;
            }

            (string propertyName, IOpenApiSchema propertySchema) = unionCase.Properties!.First();

            if (unionCase.Required is null
                || !unionCase.Required.Contains(propertyName)
                || propertySchema is not IOpenApiReferenceHolder
                || propertySchema.GetReferenceId() is not string schemaName)
            {
                // Property is not required or is not a reference to a component schema
                return false;
            }

            if (!propertyNames.Add(propertyName))
            {
                // Duplicate property name
                return false;
            }

            if (schemaName == schema.Key)
            {
                // Element contains a component reference to its parent schema
                return false;
            }

            // Building the child element must occur after checking that this is a reference to a component schema,
            // otherwise we can encounter infinite recursion trying to resolve the parent that is being built when
            // this method is called.
            ILocatedOpenApiElement<IOpenApiSchema> locatedPropertySchema = schema.CreateChild(propertySchema, propertyName);

            YardarmTypeInfo typeInfo = typeGeneratorRegistry.Get(locatedPropertySchema).TypeInfo;
            foreach (TypeSyntax propertyType in propertyTypes)
            {
                if (propertyType.IsEquivalentTo(typeInfo.Name))
                {
                    // Duplicate property type
                    return false;
                }
            }

            propertyTypes.Add(typeInfo.Name);
        }

        return true;
    }
}
