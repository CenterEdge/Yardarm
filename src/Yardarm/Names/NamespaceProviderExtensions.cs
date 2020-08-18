using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Names
{
    public static class NamespaceProviderExtensions
    {
        public static NameSyntax GetSerializationNamespace(this INamespaceProvider namespaceProvider) =>
            QualifiedName(
                namespaceProvider.GetRootNamespace(),
                IdentifierName("Serialization"));

        public static NameSyntax GetITypeSerializer(this INamespaceProvider namespaceProvider) =>
            QualifiedName(
                namespaceProvider.GetSerializationNamespace(),
                IdentifierName("ITypeSerializer"));

        public static NameSyntax GetITypeSerializerRegistry(this INamespaceProvider namespaceProvider) =>
            QualifiedName(
                namespaceProvider.GetSerializationNamespace(),
                IdentifierName("ITypeSerializerRegistry"));

        public static NameSyntax GetTypeSerializerRegistryExtensions(this INamespaceProvider namespaceProvider) =>
            QualifiedName(
                namespaceProvider.GetSerializationNamespace(),
                IdentifierName("TypeSerializerRegistryExtensions"));
    }
}
