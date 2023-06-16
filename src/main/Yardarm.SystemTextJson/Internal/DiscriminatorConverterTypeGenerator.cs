using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using Yardarm.SystemTextJson.Helpers;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson.Internal
{
    internal class DiscriminatorConverterTypeGenerator : TypeGeneratorBase<OpenApiSchema>
    {
        public static ReadOnlySpan<byte> X => new byte[] {1, 2};

        private readonly IJsonSerializationNamespace _jsonSerializationNamespace;
        private readonly NameSyntax _typeName;

        public DiscriminatorConverterTypeGenerator(ILocatedOpenApiElement<OpenApiSchema> element,
            GenerationContext context, ITypeGenerator? parent, IJsonSerializationNamespace jsonSerializationNamespace,
            IRootNamespace rootNamespace)
            : base(element, context, parent)
        {
            _jsonSerializationNamespace = jsonSerializationNamespace ??
                                          throw new ArgumentNullException(nameof(jsonSerializationNamespace));

            _typeName = BuildTypeName(rootNamespace);
        }

        protected NameSyntax BuildTypeName(IRootNamespace rootNamespace)
        {
            string rootNamespacePrefix = rootNamespace.Name + ".";
            string typeNameString = Context.TypeGeneratorRegistry.Get(Element).TypeInfo.Name.ToString();

            // Trim the root namespace to keep the length down, if present
            var className = typeNameString.StartsWith(rootNamespacePrefix)
                ? typeNameString.Substring(rootNamespacePrefix.Length).Replace(".", "-")
                : typeNameString.Replace(".", "-");

            NameSyntax ns = _jsonSerializationNamespace.Name;

            var formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);

            return QualifiedName(ns, IdentifierName(formatter.Format(className + "-JsonConverter")));
        }

        protected override YardarmTypeInfo GetTypeInfo() => new(_typeName);

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var classNameAndNamespace = (QualifiedNameSyntax)TypeInfo.Name;

            string className = classNameAndNamespace.Right.Identifier.Text;

            var schemaType = Context.TypeGeneratorRegistry.Get(Element).TypeInfo.Name;
            var baseType = SystemTextJsonTypes.Serialization.JsonConverterName(schemaType);

            var declaration = ClassDeclaration(
                    default,
                    TokenList(Token(SyntaxKind.InternalKeyword)),
                    Identifier(className),
                    null,
                    BaseList(SingletonSeparatedList<BaseTypeSyntax>(SimpleBaseType(baseType))),
                    default,
                    new SyntaxList<MemberDeclarationSyntax>(GenerateMethods(schemaType)));

            yield return declaration;
        }

        private IEnumerable<MemberDeclarationSyntax> GenerateMethods(TypeSyntax schemaType)
        {
            if (!string.IsNullOrEmpty(Element.Element.Discriminator?.PropertyName))
            {
                yield return GeneratePropertyNameProperty();
            }

            yield return GenerateCanConvert(schemaType);
            yield return GenerateRead(schemaType);
            yield return GenerateWrite(schemaType);
        }

        private PropertyDeclarationSyntax GeneratePropertyNameProperty()
        {
            byte[] propertyName = Encoding.UTF8.GetBytes(Element.Element.Discriminator.PropertyName);

            return PropertyDeclaration(
                default,
                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
                WellKnownTypes.System.ReadOnlySpan(PredefinedType(Token(SyntaxKind.ByteKeyword))),
                default,
                Identifier("PropertyName"),
                null,
                ArrowExpressionClause(ArrayCreationExpression(
                    ArrayType(
                        PredefinedType(Token(SyntaxKind.ByteKeyword)), new SyntaxList<ArrayRankSpecifierSyntax>(
                            ArrayRankSpecifier(SingletonSeparatedList<ExpressionSyntax>(OmittedArraySizeExpression())))),
                    InitializerExpression(SyntaxKind.ArrayInitializerExpression,
                        SeparatedList(propertyName.Select(p =>
                            (ExpressionSyntax)LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(p))))))),
                null,
                Token(SyntaxKind.SemicolonToken));
        }

        private MethodDeclarationSyntax GenerateCanConvert(TypeSyntax schemaType)
        {
            var expressionBody = ArrowExpressionClause(
                InvocationExpression(
                    MemberAccessExpression(
                        SyntaxKind.SimpleMemberAccessExpression,
                        TypeOfExpression(schemaType),
                        IdentifierName("IsAssignableFrom")),
                    ArgumentList(SingletonSeparatedList(Argument(IdentifierName("typeToConvert"))))));

            return MethodDeclaration(
                default,
                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)),
                PredefinedType(Token(SyntaxKind.BoolKeyword)),
                default,
                Identifier("CanConvert"),
                default,
                ParameterList(SingletonSeparatedList(
                    Parameter(
                        default,
                        TokenList(),
                        WellKnownTypes.System.Type,
                        Identifier("typeToConvert"),
                        null))),
                default,
                null,
                expressionBody,
                Token(SyntaxKind.SemicolonToken));
        }

        private MethodDeclarationSyntax GenerateRead(TypeSyntax schemaType)
        {
            BlockSyntax body;

            if (!string.IsNullOrEmpty(Element.Element.Discriminator?.PropertyName))
            {
                var mappings = GetStronglyTypedMappings()
                    .Select(mapping =>
                        SwitchExpressionArm(
                            ConstantPattern(LiteralExpression(SyntaxKind.StringLiteralExpression,
                                Literal(mapping.key))),
                            PostfixUnaryExpression(SyntaxKind.SuppressNullableWarningExpression, InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    SystemTextJsonTypes.JsonSerializer,
                                    GenericName(Identifier("Deserialize"),
                                        TypeArgumentList(SingletonSeparatedList(mapping.typeName)))),
                                ArgumentList(SeparatedList(new[]
                                {
                                    Argument(null, Token(SyntaxKind.RefKeyword), IdentifierName("reader")),
                                    Argument(IdentifierName("options"))
                                }))))))
                    .Concat(new[]
                    {
                        SwitchExpressionArm(DiscardPattern(),
                            ThrowExpression(ObjectCreationExpression(SystemTextJsonTypes.JsonException)))
                    });

                body = Block(default, new SyntaxList<StatementSyntax>(new StatementSyntax[]
                {
                    LocalDeclarationStatement(VariableDeclaration(
                        NullableType(PredefinedType(Token(SyntaxKind.StringKeyword))),
                        SingletonSeparatedList(VariableDeclarator(
                            Identifier("discriminator"),
                            null,
                            EqualsValueClause(_jsonSerializationNamespace.GetDiscriminator(
                                IdentifierName("reader"),
                                IdentifierName("PropertyName"))))))),
                    ReturnStatement(SwitchExpression(IdentifierName("discriminator"), SeparatedList(mappings)))
                }));
            }
            else
            {
                body = Block(default, new SyntaxList<StatementSyntax>(
                    ThrowStatement(ObjectCreationExpression(
                        SystemTextJsonTypes.JsonException,
                        ArgumentList(SingletonSeparatedList(Argument(
                            SyntaxHelpers.StringLiteral("Cannot deserialize oneOf schemas with no discriminator")))),
                        null))));
            }

            return MethodDeclaration(
                default,
                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)),
                schemaType,
                default,
                Identifier("Read"),
                default,
                ParameterList(SeparatedList<ParameterSyntax>(new [] {
                    Parameter(
                        default,
                        TokenList(Token(SyntaxKind.RefKeyword)),
                        SystemTextJsonTypes.Utf8JsonReader,
                        Identifier("reader"),
                        null),
                    Parameter(
                        default,
                        TokenList(),
                        WellKnownTypes.System.Type,
                        Identifier("typeToConvert"),
                        null),
                    Parameter(
                        default,
                        TokenList(),
                        SystemTextJsonTypes.JsonSerializerOptions,
                        Identifier("options"),
                        null)
                })),
                default,
                body,
                null);
        }

        private MethodDeclarationSyntax GenerateWrite(TypeSyntax schemaType)
        {
            var argumentList = ArgumentList(SeparatedList(new[]
            {
                Argument(IdentifierName("writer")),
                Argument(IdentifierName("x")),
                Argument(IdentifierName("options"))
            }));

            var mappings = GetStronglyTypedMappings()
                .Select(p => p.typeName)
                .Distinct()
                .Select(typeName =>
                    SwitchSection(
                        new SyntaxList<SwitchLabelSyntax>(CasePatternSwitchLabel(
                            DeclarationPattern(typeName, SingleVariableDesignation(Identifier("x"))),
                            Token(SyntaxKind.ColonToken))),
                        new SyntaxList<StatementSyntax>(new StatementSyntax[] {
                            ExpressionStatement(InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    SystemTextJsonTypes.JsonSerializer,
                                    GenericName(Identifier("Serialize"),
                                        TypeArgumentList(SingletonSeparatedList(typeName)))),
                                argumentList)),
                            BreakStatement()
                        } )))
                .Concat(new[]
                {
                    SwitchSection(
                        new SyntaxList<SwitchLabelSyntax>(DefaultSwitchLabel()),
                        new SyntaxList<StatementSyntax>(
                            ThrowStatement(ObjectCreationExpression(SystemTextJsonTypes.JsonException))
                        ))
                });

            var body = Block(new SyntaxList<StatementSyntax>(
                SwitchStatement(
                    IdentifierName("value"),
                    new SyntaxList<SwitchSectionSyntax>(mappings))));

            return MethodDeclaration(
                default,
                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.OverrideKeyword)),
                PredefinedType(Token(SyntaxKind.VoidKeyword)),
                default,
                Identifier("Write"),
                default,
                ParameterList(SeparatedList<ParameterSyntax>(new [] {
                    Parameter(
                        default,
                        TokenList(),
                        SystemTextJsonTypes.Utf8JsonWriter,
                        Identifier("writer"),
                        null),
                    Parameter(
                        default,
                        TokenList(),
                        schemaType,
                        Identifier("value"),
                        null),
                    Parameter(
                        default,
                        TokenList(),
                        SystemTextJsonTypes.JsonSerializerOptions,
                        Identifier("options"),
                        null)
                })),
                default,
                body,
                null);
        }

        private IEnumerable<(string key, TypeSyntax typeName)> GetStronglyTypedMappings() =>
            GetMappings(Element)
                .Select(p => (p.Key, Context.TypeGeneratorRegistry.Get(p.Schema).TypeInfo.Name))
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                .Where(p => p.Item2 != null);

        /// <summary>
        /// Collects the list of value to schema mappings defined for the type, choosing from the
        /// best source for various kinds of mappings and polymorphism.
        /// </summary>
        /// <remarks>
        /// The preferred choice is specifically defined mappings on the discriminator. However, when
        /// missing it will fallback all oneOf's defined on the type. If that is not the case, it will
        /// look for cases of allOf inheritance from schemas defined in the components section.
        /// </remarks>
        private IEnumerable<(string Key, ILocatedOpenApiElement<OpenApiSchema> Schema)> GetMappings(
            ILocatedOpenApiElement<OpenApiSchema> element)
        {
            if (element.Element.Discriminator is {Mapping.Count: > 0})
            {
                // Use specifically listed mappings
                return element.Element.Discriminator.Mapping
                    .Select(p =>
                    {
                        // TODO: We should really be parsing this rather than just checking a prefix, but the OpenAPI parser isn't exposed
                        if (p.Value.StartsWith("#/components/schemas/"))
                        {
                            string schemaName = p.Value.Substring("#/components/schemas/".Length);
                            if (Context.Document.Components.Schemas.TryGetValue(schemaName, out var schema))
                            {
                                return (p.Key, Schema: schema.CreateRoot(p.Key));
                            }
                        }

                        return (p.Key, Schema: null!);
                    })
                    // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                    .Where(p => p.Schema is not null);
            }

            if (element.Element.OneOf is {Count: > 0})
            {
                // Gather mappings from "oneOf" that get a default mapping based on the schema name
                return element.Element.OneOf
                    .Where(p => p.Reference is not null)
                    .Select(p => (p.Reference.Id, p.CreateRoot(p.Reference.Id)));
            }

            // Find other schemas that reference this one using allOf. This only applies to base
            // classes, don't try this with interfaces.
            return Context.Document.Components.Schemas
                .Where(p =>
                {
                    var firstAllOf = p.Value.AllOf?.FirstOrDefault();
                    return firstAllOf?.Reference is not null &&
                           firstAllOf.Reference.Id == element.Key;
                })
                .Select(p => (p.Key, p.Value.CreateRoot(p.Key)));
        }
    }
}
