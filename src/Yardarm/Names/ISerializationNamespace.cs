using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names
{
    // ReSharper disable InconsistentNaming
    public interface ISerializationNamespace : IKnownNamespace
    {
        NameSyntax ITypeSerializer { get; }
        NameSyntax ITypeSerializerRegistry { get; }
        NameSyntax PathSegmentStyle { get; }
        NameSyntax PathSegmentSerializer { get; }
        ExpressionSyntax PathSegmentSerializerInstance { get; }
        NameSyntax TypeSerializerRegistryExtensions { get; }
        NameSyntax UnknownMediaTypeException { get; }
    }
}
