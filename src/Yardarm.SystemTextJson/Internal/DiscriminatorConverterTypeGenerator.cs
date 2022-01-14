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

        public DiscriminatorConverterTypeGenerator(ILocatedOpenApiElement<OpenApiSchema> element,
            GenerationContext context, ITypeGenerator? parent, IJsonSerializationNamespace jsonSerializationNamespace)
            : base(element, context, parent)
        {
            _jsonSerializationNamespace = jsonSerializationNamespace ?? throw new ArgumentNullException(nameof(jsonSerializationNamespace));
        }

        protected override YardarmTypeInfo GetTypeInfo()
        {
            var schema = Element.Element;

            if (schema.Reference != null)
            {
                NameSyntax ns = _jsonSerializationNamespace.Name;

                var formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);

                return new YardarmTypeInfo(
                    QualifiedName(ns, IdentifierName(formatter.Format(schema.Reference.Id + "-JsonConverter"))),
                    NameKind.Class);
            }

            if (Parent == null)
            {
                throw new InvalidOperationException(
                    $"Unable to generate schema for '{Element.Key}', it has no parent is not a component.");
            }

            QualifiedNameSyntax? name = Parent.GetChildName(Element, NameKind.Class);
            if (name == null)
            {
                throw new InvalidOperationException($"Unable to generate schema for '{Element.Key}', parent did not provide a name.");
            }

            return new YardarmTypeInfo(name, NameKind.Class);
        }

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
            yield return GeneratePropertyNameProperty();
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
                null);
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
                expressionBody);
        }

        private MethodDeclarationSyntax GenerateRead(TypeSyntax schemaType)
        {
            var mappings = GetStronglyTypedMappings()
                .Select(mapping =>
                    SwitchExpressionArm(
                        ConstantPattern(LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(mapping.key))),
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

            var body = Block(default, new SyntaxList<StatementSyntax>(new StatementSyntax[] {
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
                        new SyntaxList<StatementSyntax>(new StatementSyntax[] {
                            ThrowStatement(ObjectCreationExpression(SystemTextJsonTypes.JsonException)),
                            BreakStatement()
                        } ))
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
            Element.Element.Discriminator.Mapping
                .Select(mapping =>
                {
                    var referencedSchema = Element.Element.OneOf
                        .FirstOrDefault(p => p.Reference?.ReferenceV3 == mapping.Value);

                    var locatedReferencedSchema = referencedSchema?.CreateRoot(referencedSchema.Reference.Id);

                    TypeSyntax? typeName = null;
                    if (locatedReferencedSchema != null)
                    {
                        typeName = Context.TypeGeneratorRegistry.Get(locatedReferencedSchema).TypeInfo.Name;
                    }

                    return (key: mapping.Key, typeName: typeName!);
                })
                .Where(p => p.typeName != null);
    }
}
