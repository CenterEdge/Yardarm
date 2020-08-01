using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.NewtonsoftJson.Helpers
{
    internal static class JsonHelpers
    {
        public static NameSyntax NewtonsoftJson() => SyntaxFactory.QualifiedName(
            SyntaxFactory.IdentifierName("Newtonsoft"),
            SyntaxFactory.IdentifierName("Json"));

        public static NameSyntax JsonPropertyAttributeName() => SyntaxFactory.QualifiedName(
            NewtonsoftJson(),
            SyntaxFactory.IdentifierName("JsonProperty"));
    }
}
