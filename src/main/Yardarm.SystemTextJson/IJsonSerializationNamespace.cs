using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.SystemTextJson
{
    public interface IJsonSerializationNamespace
    {
        NameSyntax Name { get; }
        NameSyntax JsonDateConverter { get; }
        NameSyntax JsonTypeSerializer { get; }

        InvocationExpressionSyntax GetDiscriminator(ExpressionSyntax reader, ExpressionSyntax utf8PropertyName);
    }
}
