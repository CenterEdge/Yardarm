using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.SystemTextJson.Internal;

internal class JsonSerializationNamespace : IKnownNamespace, IJsonSerializationNamespace
{
    public NameSyntax Name { get; }
    public NameSyntax JsonDateConverter { get; }
    public NameSyntax JsonTypeSerializer { get; }
    public NameSyntax JsonHelpers { get; }
    public NameSyntax UnknownDiscriminatorHandling { get; }

    public JsonSerializationNamespace(ISerializationNamespace serializationNamespace)
    {
        ArgumentNullException.ThrowIfNull(serializationNamespace);

        Name = QualifiedName(
            serializationNamespace.Name,
            IdentifierName("Json"));

        JsonDateConverter = QualifiedName(
            Name,
            IdentifierName("JsonDateConverter"));

        JsonTypeSerializer = QualifiedName(
            Name,
            IdentifierName("JsonTypeSerializer"));

        JsonHelpers = QualifiedName(
            Name,
            IdentifierName("JsonHelpers"));

        UnknownDiscriminatorHandling = QualifiedName(
            Name,
            IdentifierName("UnknownDiscriminatorHandling"));
    }

    public InvocationExpressionSyntax GetDiscriminator(ExpressionSyntax reader, ExpressionSyntax utf8PropertyName) =>
        InvocationExpression(
            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                JsonHelpers,
                IdentifierName("GetDiscriminator")),
            ArgumentList(SeparatedList<ArgumentSyntax>([
                Argument(null, Token(SyntaxKind.RefKeyword), reader),
                Argument(utf8PropertyName) ])));

    public TypeSyntax JsonNamedStringEnumConverterName(TypeSyntax enumType) =>
        QualifiedName(Name, GenericName(Identifier("JsonNamedStringEnumConverter"),
            TypeArgumentList(SingletonSeparatedList(enumType))));

    public TypeSyntax JsonDiscriminatedObjectConverterName(TypeSyntax schemaType) =>
        QualifiedName(Name, GenericName(Identifier("JsonDiscriminatedObjectConverter"),
            TypeArgumentList(SingletonSeparatedList(schemaType))));
}
