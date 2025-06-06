﻿using System.Diagnostics.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable MemberHidesStaticFromOuterClass
    public static partial class WellKnownTypes
    {
        public static partial class System
        {
            public static NameSyntax Name { get; } = AliasQualifiedName(
                IdentifierName(Token(SyntaxKind.GlobalKeyword)),
                IdentifierName("System"));

            public static class ArgumentNullException
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("ArgumentNullException"));
            }

            public static class Collections
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("Collections"));

                public static class Generic
                {
                    public static NameSyntax Name { get; } = QualifiedName(
                        Collections.Name,
                        IdentifierName("Generic"));

                    public static class DictionaryT
                    {
                        public static NameSyntax Name(TypeSyntax keyType, TypeSyntax valueType) =>
                            QualifiedName(
                                Generic.Name,
                                GenericName(
                                    Identifier("Dictionary"),
                                    TypeArgumentList(SeparatedList(new [] {keyType, valueType}))));

                        public static bool IsOfType(TypeSyntax nameSyntax,
                            [NotNullWhen(true)] out TypeSyntax? keyArgument,
                            [NotNullWhen(true)] out TypeSyntax? valueArgument)
                        {
                            if (nameSyntax is QualifiedNameSyntax qualifiedName && qualifiedName.Left.IsEquivalentTo(Generic.Name))
                            {
                                if (qualifiedName.Right is GenericNameSyntax genericName
                                    && genericName.Identifier.ValueText == "Dictionary"
                                    && genericName.TypeArgumentList.Arguments.Count == 2)
                                {
                                    keyArgument = genericName.TypeArgumentList.Arguments[0];
                                    valueArgument = genericName.TypeArgumentList.Arguments[1];
                                    return true;
                                }
                            }

                            keyArgument = null;
                            valueArgument = null;
                            return false;
                        }
                    }

                    public static class IDictionaryT
                    {
                        public static NameSyntax Name(TypeSyntax keyType, TypeSyntax valueType) =>
                            QualifiedName(
                                Generic.Name,
                                GenericName(
                                    Identifier("IDictionary"),
                                    TypeArgumentList(SeparatedList(new [] {keyType, valueType}))));
                    }

                    public static class IEnumerableT
                    {
                        public static NameSyntax Name(TypeSyntax itemType) =>
                            QualifiedName(
                                Generic.Name,
                                GenericName(
                                    Identifier("IEnumerable"),
                                    TypeArgumentList(SingletonSeparatedList(itemType))));
                    }

                    public static class ListT
                    {
                        public static NameSyntax Name(TypeSyntax itemType) =>
                            QualifiedName(
                                Generic.Name,
                                GenericName(
                                    Identifier("List"),
                                    TypeArgumentList(SingletonSeparatedList(itemType))));

                        public static bool IsOfType(TypeSyntax nameSyntax, [NotNullWhen(true)] out TypeSyntax? genericArgument)
                        {
                            if (nameSyntax is not QualifiedNameSyntax qualifiedName
                                || !qualifiedName.Left.IsEquivalentTo(Generic.Name)
                                || qualifiedName.Right is not GenericNameSyntax
                                {
                                    Identifier.ValueText: "List",
                                    TypeArgumentList.Arguments.Count: 1
                                } genericName)
                            {
                                genericArgument = null;
                                return false;
                            }

                            genericArgument = genericName.TypeArgumentList.Arguments[0];
                            return true;
                        }
                    }

                    public static class KeyValuePair
                    {
                        public static NameSyntax Name(TypeSyntax keyType, TypeSyntax valueType) =>
                            QualifiedName(
                                Generic.Name,
                                GenericName(
                                    Identifier("KeyValuePair"),
                                    TypeArgumentList(SeparatedList(new[] {
                                        keyType,
                                        valueType
                                    }))));
                    }
                }
            }

            public static class Convert
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("Convert"));

                public static InvocationExpressionSyntax ToBase64String(
                    ExpressionSyntax bytesExpression) =>
                    InvocationExpression(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            Name,
                            IdentifierName("ToBase64String")),
                        ArgumentList(
                            SingletonSeparatedList(
                                Argument(bytesExpression)
                            )));
            }

            public static class ComponentModel
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("ComponentModel"));

                public static class DataAnnotations
                {
                    public static NameSyntax Name { get; } = QualifiedName(
                        ComponentModel.Name,
                        IdentifierName("DataAnnotations"));

                    public static class RequiredAttribute
                    {
                        public static NameSyntax Name { get; } = QualifiedName(
                            DataAnnotations.Name,
                            IdentifierName("RequiredAttribute"));
                    }
                }
            }

            public static class Diagnostics
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("Diagnostics"));

                public static class CodeAnalysis
                {
                    public static NameSyntax Name { get; } = QualifiedName(
                        Diagnostics.Name,
                        IdentifierName("CodeAnalysis"));

                    public static class UnconditionalSuppressMessageAttribute
                    {
                        public static NameSyntax Name { get; } = QualifiedName(
                            CodeAnalysis.Name,
                            IdentifierName("UnconditionalSuppressMessageAttribute"));
                    }
                }
            }

            public static class IDisposable
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("IDisposable"));
            }

            public static class IO
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("IO"));

                public static class Stream
                {
                    public static NameSyntax Name { get; } = QualifiedName(
                        IO.Name,
                        IdentifierName("Stream"));
                }
            }

            public static partial class Net
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("Net"));

                public static class HttpStatusCode
                {
                    public static NameSyntax Name { get; } = QualifiedName(
                        Net.Name,
                        IdentifierName("HttpStatusCode"));
                }
            }

            public static class ObsoleteAttribute
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("ObsoleteAttribute"));
            }

            public static NameSyntax ReadOnlySpan(TypeSyntax elementType) =>
                QualifiedName(
                    Name,
                    GenericName(
                        Identifier("ReadOnlySpan"),
                        TypeArgumentList(SingletonSeparatedList(elementType))));

            public static class Text
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("Text"));

                public static class Encoding
                {
                    public static NameSyntax Name { get; } = QualifiedName(
                        Text.Name,
                        IdentifierName("Encoding"));

                    public static ExpressionSyntax UTF8 { get; } = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        Encoding.Name,
                        IdentifierName("UTF8"));

                    public static InvocationExpressionSyntax GetBytes(
                        ExpressionSyntax encodingExpression, ExpressionSyntax stringExpression) =>
                        InvocationExpression(MemberAccessExpression(
                                SyntaxKind.SimpleMemberAccessExpression,
                                encodingExpression,
                                IdentifierName("GetBytes")),
                            ArgumentList(
                                SingletonSeparatedList(
                                    Argument(stringExpression)
                                )));
                }
            }

            public static class Threading
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("Threading"));

                public static class CancellationToken
                {
                    public static NameSyntax Name { get; } = QualifiedName(
                        Threading.Name,
                        IdentifierName("CancellationToken"));
                }

                public static class Tasks
                {
                    public static NameSyntax Name { get; } = QualifiedName(
                        Threading.Name,
                        IdentifierName("Tasks"));

                    public static class Task
                    {
                        public static NameSyntax Name => QualifiedName(
                            Tasks.Name,
                            IdentifierName("Task"));
                    }

                    public static class TaskT
                    {
                        public static NameSyntax Name(TypeSyntax resultType) =>
                            QualifiedName(
                                Tasks.Name,
                                GenericName(
                                    Identifier("Task"),
                                    TypeArgumentList(SingletonSeparatedList(resultType))));
                    }

                    public static class ValueTask
                    {
                        public static NameSyntax Name => QualifiedName(
                            Tasks.Name,
                            IdentifierName("ValueTask"));
                    }

                    public static class ValueTaskT
                    {
                        public static NameSyntax Name(TypeSyntax resultType) =>
                            QualifiedName(
                                Tasks.Name,
                                GenericName(
                                    Identifier("ValueTask"),
                                    TypeArgumentList(SingletonSeparatedList(resultType))));
                    }
                }
            }

            public static NameSyntax Type { get; } = QualifiedName(
                System.Name,
                IdentifierName("Type"));

            public static class Uri
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("Uri"));
            }

            public static class UriKind
            {
                public static NameSyntax Name { get; } = QualifiedName(
                    System.Name,
                    IdentifierName("UriKind"));

                public static ExpressionSyntax RelativeOrAbsolute { get; } =
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        Name,
                        IdentifierName("RelativeOrAbsolute"));
            }
        }
    }
}
