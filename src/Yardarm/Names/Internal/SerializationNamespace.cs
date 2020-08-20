using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Names.Internal
{
    // ReSharper disable InconsistentNaming
    internal class SerializationNamespace : ISerializationNamespace
    {
        public NameSyntax Name { get; }
        public NameSyntax ITypeSerializer { get; }
        public NameSyntax ITypeSerializerRegistry { get; }
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

            ITypeSerializer = QualifiedName(
                Name,
                IdentifierName("ITypeSerializer"));

            ITypeSerializerRegistry = QualifiedName(
                Name,
                IdentifierName("ITypeSerializerRegistry"));

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
