using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names
{
    // ReSharper disable InconsistentNaming
    public interface ISerializationNamespace : IKnownNamespace
    {
        NameSyntax HeaderSerializer { get; }
        NameSyntax ISerializationData { get; }
        NameSyntax ITypeSerializer { get; }
        NameSyntax ITypeSerializerRegistry { get; }
        NameSyntax MultipartFieldDetails { get; }
        NameSyntax MultipartFormDataSerializer { get; }
        NameSyntax PathSegmentStyle { get; }
        NameSyntax PathSegmentSerializer { get; }
        NameSyntax QueryStringBuilder { get; }
        NameSyntax TypeSerializerRegistryExtensions { get; }
        NameSyntax UnknownMediaTypeException { get; }

        NameSyntax MultipartFormDataSerializationData(TypeSyntax type);
        NameSyntax MultipartPropertyInfo(TypeSyntax type);
    }
}
