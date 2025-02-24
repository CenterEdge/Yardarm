using System;
using System.Collections.Generic;
using System.Linq;
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
        public const string GeneratorCategory = "DiscriminatorConverter";

        private const string TrimJustification =
            "This is trim and AOT safe so long as the options came from a JsonSerializerContext because JsonDerivedType was used to include the descendants we're referencing.";

        private static readonly SyntaxList<AttributeListSyntax> s_suppressTrimAndAotWarnings =
            List(new[]
            {
                AttributeList(SingletonSeparatedList(Attribute(
                    WellKnownTypes.System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessageAttribute.Name,
                    AttributeArgumentList(SeparatedList(new[]
                    {
                        AttributeArgument(SyntaxHelpers.StringLiteral("ReflectionAnalysis")),
                        AttributeArgument(SyntaxHelpers.StringLiteral("IL2026")),
                        AttributeArgument(
                            NameEquals(IdentifierName("Justification")),
                            null,
                            SyntaxHelpers.StringLiteral(TrimJustification))
                    }))))),
                AttributeList(SingletonSeparatedList(Attribute(
                    WellKnownTypes.System.Diagnostics.CodeAnalysis.UnconditionalSuppressMessageAttribute.Name,
                    AttributeArgumentList(SeparatedList(new[]
                    {
                        AttributeArgument(SyntaxHelpers.StringLiteral("Aot")),
                        AttributeArgument(SyntaxHelpers.StringLiteral("IL3050:RequiresDynamicCode")),
                        AttributeArgument(
                            NameEquals(IdentifierName("Justification")),
                            null,
                            SyntaxHelpers.StringLiteral(TrimJustification))
                    })))))
            });

        private readonly IJsonSerializationNamespace _jsonSerializationNamespace;
        private readonly NameSyntax _typeName;

        public DiscriminatorConverterTypeGenerator(ILocatedOpenApiElement<OpenApiSchema> element,
            GenerationContext context, ITypeGenerator? parent, IJsonSerializationNamespace jsonSerializationNamespace,
            IRootNamespace rootNamespace)
            : base(element, context, parent)
        {
            ArgumentNullException.ThrowIfNull(jsonSerializationNamespace);
            ArgumentNullException.ThrowIfNull(rootNamespace);

            _jsonSerializationNamespace = jsonSerializationNamespace;
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
                    TokenList(Token(SyntaxKind.PublicKeyword)),
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
            string propertyName = Element.Element.Discriminator.PropertyName;

            return PropertyDeclaration(
                default,
                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)),
                WellKnownTypes.System.ReadOnlySpan(PredefinedType(Token(SyntaxKind.ByteKeyword))),
                default,
                Identifier("PropertyName"),
                null,
                ArrowExpressionClause(LiteralExpression(
                    SyntaxKind.Utf8StringLiteralExpression,
                    Token(default, SyntaxKind.Utf8StringLiteralToken, SymbolDisplay.FormatLiteral(propertyName, quote: true) + "u8", propertyName, default))),
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
                var mappings = SchemaHelper.GetDiscriminatorMappings(Context, Element)
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
                s_suppressTrimAndAotWarnings,
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

            var mappings = SchemaHelper.GetDiscriminatorMappings(Context, Element)
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
                s_suppressTrimAndAotWarnings,
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
    }
}
