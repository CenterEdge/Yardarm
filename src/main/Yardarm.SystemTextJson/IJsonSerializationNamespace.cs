using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.SystemTextJson
{
    public interface IJsonSerializationNamespace
    {
        NameSyntax Name { get; }
        NameSyntax JsonTypeSerializer { get; }

        TypeSyntax JsonStringEnumConverter(TypeSyntax valueType);

        InvocationExpressionSyntax GetDiscriminator(ExpressionSyntax reader, ExpressionSyntax utf8PropertyName);
    }
}
