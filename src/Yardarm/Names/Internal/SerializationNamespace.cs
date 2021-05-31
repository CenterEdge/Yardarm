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
        public NameSyntax ITypeSerializer { get; }
        public NameSyntax ITypeSerializerRegistry { get; }
        public NameSyntax MultipartEncodingAttribute { get; }
        public NameSyntax MultipartFormDataSerializer { get; }
        public NameSyntax MultipartPropertyAttribute { get; }
        public NameSyntax PathSegmentStyle { get; }
        public NameSyntax PathSegmentSerializer { get; }
        public ExpressionSyntax PathSegmentSerializerInstance { get; }
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

            ITypeSerializer = QualifiedName(
                Name,
                IdentifierName("ITypeSerializer"));

            ITypeSerializerRegistry = QualifiedName(
                Name,
                IdentifierName("ITypeSerializerRegistry"));

            MultipartEncodingAttribute = QualifiedName(
                Name,
                IdentifierName("MultipartEncodingAttribute"));

            MultipartFormDataSerializer = QualifiedName(
                Name,
                IdentifierName("MultipartFormDataSerializer"));

            MultipartPropertyAttribute = QualifiedName(
                Name,
                IdentifierName("MultipartPropertyAttribute"));

            PathSegmentStyle = QualifiedName(
                Name,
                IdentifierName("PathSegmentStyle"));

            PathSegmentSerializer = QualifiedName(
                Name,
                IdentifierName("PathSegmentSerializer"));

            PathSegmentSerializerInstance = MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                PathSegmentSerializer,
                IdentifierName("Instance"));

            TypeSerializerRegistryExtensions = QualifiedName(
                Name,
                IdentifierName("TypeSerializerRegistryExtensions"));

            UnknownMediaTypeException = QualifiedName(
                Name,
                IdentifierName("UnknownMediaTypeException"));
        }
    }
}
