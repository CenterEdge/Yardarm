using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Generation.Schema;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    public class ParseHeadersMethodGenerator : IParseHeadersMethodGenerator
    {
        public const string ParseHeadersMethodName = "ParseHeaders";

        protected GenerationContext Context { get; }
        protected ISerializationNamespace SerializationNamespace { get; }

        public ParseHeadersMethodGenerator(GenerationContext context,
            ISerializationNamespace serializationNamespace)
        {
            Context = context ?? throw new ArgumentNullException(nameof(context));
            SerializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
        }

        public MethodDeclarationSyntax Generate(ILocatedOpenApiElement<OpenApiResponse> response) =>
            MethodDeclaration(
                    PredefinedType(Token(SyntaxKind.VoidKeyword)),
                    ParseHeadersMethodName)
                .AddModifiers(Token(SyntaxKind.PrivateKeyword))
                .WithBody(Block(GenerateStatements(response)));

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(
            ILocatedOpenApiElement<OpenApiResponse> response)
        {
            if (response.Element.Headers.Count == 0)
            {
                yield break;
            }

            var propertyNameFormatter = Context.NameFormatterSelector.GetFormatter(NameKind.Property);

            // Declare values variable to hold TryGetValue out results
            yield return LocalDeclarationStatement(VariableDeclaration(
                    WellKnownTypes.System.Collections.Generic.IEnumerableT.Name(
                        PredefinedType(Token(SyntaxKind.StringKeyword))))
                .AddVariables(VariableDeclarator("values")));

            NameSyntax valuesName = IdentifierName("values");

            foreach (var header in response.GetHeaders())
            {
                ILocatedOpenApiElement<OpenApiSchema> schemaElement = header.GetSchemaOrDefault();

                TypeSyntax typeName = Context.TypeNameProvider.GetName(schemaElement);

                InvocationExpressionSyntax deserializeExpression =
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SerializationNamespace.HeaderSerializerInstance,
                            GenericName("Deserialize")
                                .AddTypeArgumentListArguments(typeName)))
                        .AddArgumentListArguments(
                            Argument(valuesName),
                            Argument(header.Element.Explode
                                ? LiteralExpression(SyntaxKind.TrueLiteralExpression)
                                : LiteralExpression(SyntaxKind.FalseLiteralExpression)));

                yield return IfStatement(
                    WellKnownTypes.System.Net.Http.Headers.HttpHeaders.TryGetValues(
                        SyntaxHelpers.MemberAccess(IdentifierName("Message"),
                            "Headers"),
                        SyntaxHelpers.StringLiteral(header.Key),
                        valuesName),
                    Block(
                        ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                            IdentifierName(propertyNameFormatter.Format(header.Key)),
                            deserializeExpression))));
            }
        }

        public static InvocationExpressionSyntax InvokeParseHeaders(ExpressionSyntax requestInstance) =>
            InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        requestInstance,
                        IdentifierName(ParseHeadersMethodName)));
    }
}
