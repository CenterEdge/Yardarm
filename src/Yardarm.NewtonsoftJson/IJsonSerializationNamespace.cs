using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.NewtonsoftJson
{
    public interface IJsonSerializationNamespace
    {
        NameSyntax Name { get; }
        NameSyntax DiscriminatorConverter { get; }
        NameSyntax JsonTypeSerializer { get; }

        TypeSyntax AdditionalPropertiesDictionary(TypeSyntax valueType);
    }
}
