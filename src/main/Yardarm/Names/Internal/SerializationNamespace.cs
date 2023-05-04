using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Names.Internal
{
    // ReSharper disable InconsistentNaming
    internal class SerializationNamespace : ISerializationNamespace
    {
        public NameSyntax HeaderSerializer { get; }
        public ExpressionSyntax HeaderSerializerInstance { get; }
        public NameSyntax Name { get; }
        public NameSyntax ISerializationData { get; }
        public NameSyntax ITypeSerializer { get; }
        public NameSyntax ITypeSerializerRegistry { get; }
        public NameSyntax MultipartFieldDetails { get; }
        public NameSyntax MultipartFormDataSerializer { get; }
        public NameSyntax PathSegmentStyle { get; }
        public NameSyntax PathSegmentSerializer { get; }
        public ExpressionSyntax PathSegmentSerializerInstance { get; }
        public NameSyntax QueryStringBuilder { get; }
        public NameSyntax TypeSerializerRegistryExtensions { get; }
        public NameSyntax UnknownMediaTypeException { get; }

        public SerializationNamespace(IRootNamespace rootNamespace)
        {
            if (rootNamespace == null)
            {
                throw new ArgumentNullException(nameof(rootNamespace));
            }

            Name = QualifiedName(rootNamespace.Name, IdentifierName("Serialization"));

            HeaderSerializer = QualifiedName(
                Name,
                IdentifierName("HeaderSerializer"));

            HeaderSerializerInstance = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                HeaderSerializer,
                IdentifierName("Instance"));

            ISerializationData = QualifiedName(
                Name,
                IdentifierName("ISerializationData"));

            ITypeSerializer = QualifiedName(
                Name,
                IdentifierName("ITypeSerializer"));

            ITypeSerializerRegistry = QualifiedName(
                Name,
                IdentifierName("ITypeSerializerRegistry"));

            MultipartFieldDetails = QualifiedName(
                Name,
                IdentifierName("MultipartFieldDetails"));

            MultipartFormDataSerializer = QualifiedName(
                Name,
                IdentifierName("MultipartFormDataSerializer"));

            PathSegmentStyle = QualifiedName(
                Name,
                IdentifierName("PathSegmentStyle"));

            PathSegmentSerializer = QualifiedName(
                Name,
                IdentifierName("PathSegmentSerializer"));
            
            QueryStringBuilder = QualifiedName(
                Name,
                IdentifierName("QueryStringBuilder"));

            TypeSerializerRegistryExtensions = QualifiedName(
                Name,
                IdentifierName("TypeSerializerRegistryExtensions"));

            UnknownMediaTypeException = QualifiedName(
                Name,
                IdentifierName("UnknownMediaTypeException"));
        }

        public NameSyntax MultipartFormDataSerializationData(TypeSyntax type) =>
            QualifiedName(
                Name,
                GenericName(Identifier("MultipartFormDataSerializationData"),
                    TypeArgumentList(SingletonSeparatedList(type))));

        public NameSyntax MultipartPropertyInfo(TypeSyntax type) =>
            QualifiedName(
                Name,
                GenericName(Identifier("MultipartPropertyInfo"),
                    TypeArgumentList(SingletonSeparatedList(type))));
    }
}
