using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.SystemTextJson
{
    public interface IJsonSerializationNamespace
    {
        NameSyntax Name { get; }
        NameSyntax JsonTypeSerializer { get; }

        TypeSyntax JsonStringEnumConverter(TypeSyntax valueType);
    }
}
