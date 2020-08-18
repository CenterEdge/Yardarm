using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Helpers
{
    // ReSharper disable InconsistentNaming
    // ReSharper disable MemberHidesStaticFromOuterClass
    public static partial class WellKnownTypes
    {
        public static partial class System
        {
            public static partial class Net
            {
                public static class Http
                {
                    public static NameSyntax Name { get; } = QualifiedName(
                        Net.Name,
                        IdentifierName("Http"));

                    public static class Headers
                    {
                        public static NameSyntax Name { get; } = QualifiedName(
                            Http.Name,
                            IdentifierName("Headers"));

                        public static class MediaTypeWithQualityHeaderValue
                        {
                            public static NameSyntax Name { get; } = QualifiedName(
                                Headers.Name,
                                IdentifierName("MediaTypeWithQualityHeaderValue"));
                        }
                    }

                    public static class HttpClient
                    {
                        public static NameSyntax Name { get; } = QualifiedName(
                            Http.Name,
                            IdentifierName("HttpClient"));
                    }

                    public static class HttpContent
                    {
                        public static NameSyntax Name { get; } = QualifiedName(
                            Http.Name,
                            IdentifierName("HttpContent"));
                    }

                    public static class HttpMethod
                    {
                        public static NameSyntax Name { get; } = QualifiedName(
                            Http.Name,
                            IdentifierName("HttpMethod"));
                    }

                    public static class HttpRequestMessage
                    {
                        public static NameSyntax Name { get; } = QualifiedName(
                            Http.Name,
                            IdentifierName("HttpRequestMessage"));
                    }

                    public static class HttpResponseMessage
                    {
                        public static NameSyntax Name { get; } = QualifiedName(
                            Http.Name,
                            IdentifierName("HttpResponseMessage"));
                    }

                    public static class StreamContent
                    {
                        public static NameSyntax Name { get; } = QualifiedName(
                            Http.Name,
                            IdentifierName("StreamContent"));
                    }

                    public static class StringContent
                    {
                        public static NameSyntax Name { get; } = QualifiedName(
                            Http.Name,
                            IdentifierName("StringContent"));
                    }
                }
            }
        }
    }
}
