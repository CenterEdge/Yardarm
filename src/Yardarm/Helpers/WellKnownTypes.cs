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
            public static NameSyntax Name => IdentifierName("System");

            public static class Collections
            {
                public static NameSyntax Name => QualifiedName(
                    System.Name,
                    IdentifierName("Collections"));

                public static class Generic
                {
                    public static NameSyntax Name => QualifiedName(
                        Collections.Name,
                        IdentifierName("Generic"));

                    public static class ListT
                    {
                        public static NameSyntax Name(TypeSyntax itemType) =>
                            QualifiedName(
                                Generic.Name,
                                GenericName(
                                    Identifier("List"),
                                    TypeArgumentList(SingletonSeparatedList(itemType))));
                    }
                }
            }

            public static class ComponentModel
            {
                public static NameSyntax Name => QualifiedName(
                    System.Name,
                    IdentifierName("ComponentModel"));

                public static class DataAnnotations
                {
                    public static NameSyntax Name => QualifiedName(
                        ComponentModel.Name,
                        IdentifierName("DataAnnotations"));

                    public static class RequiredAttribute
                    {
                        public static NameSyntax Name => QualifiedName(
                            DataAnnotations.Name,
                            IdentifierName("RequiredAttribute"));
                    }
                }
            }

            public static class IDisposable
            {
                public static NameSyntax Name => QualifiedName(
                    System.Name,
                    IdentifierName("IDisposable"));
            }

            public static partial class Net
            {
                public static NameSyntax Name => QualifiedName(
                    System.Name,
                    IdentifierName("Net"));

                public static class HttpStatusCode
                {
                    public static NameSyntax Name => QualifiedName(
                        Net.Name,
                        IdentifierName("HttpStatusCode"));
                }
            }

            public static class Threading
            {
                public static NameSyntax Name => QualifiedName(
                    System.Name,
                    IdentifierName("Threading"));

                public static class CancellationToken
                {
                    public static NameSyntax Name => QualifiedName(
                        Threading.Name,
                        IdentifierName("CancellationToken"));
                }

                public static class Tasks
                {
                    public static NameSyntax Name => QualifiedName(
                        Threading.Name,
                        IdentifierName("Tasks"));

                    public static class TaskT
                    {
                        public static NameSyntax Name(TypeSyntax resultType) =>
                            QualifiedName(
                                Tasks.Name,
                                GenericName(
                                    Identifier("Task"),
                                    TypeArgumentList(SingletonSeparatedList(resultType))));
                    }
                }
            }
        }
    }
}
