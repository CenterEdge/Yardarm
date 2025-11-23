using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    public class GetBodyMethodGenerator : IResponseMethodGenerator
    {
        public const string GetBodyMethodName = "GetBodyAsync";

        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected GenerationContext Context { get; }
        protected ISerializationNamespace SerializationNamespace { get; }

        public GetBodyMethodGenerator(IMediaTypeSelector mediaTypeSelector, GenerationContext context,
            ISerializationNamespace serializationNamespace)
        {
            ArgumentNullException.ThrowIfNull(mediaTypeSelector);
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(serializationNamespace);

            MediaTypeSelector = mediaTypeSelector;
            Context = context;
            SerializationNamespace = serializationNamespace;
        }

        public IEnumerable<BaseMethodDeclarationSyntax> Generate(ILocatedOpenApiElement<OpenApiResponse> response, string className)
        {
            if (!response.IsRoot && response.Element.Reference != null)
            {
                // Do not generator for responses within operations that are references to components, these will inherit
                // their get body method from the component base class
                yield break;
            }

            if (response.Element.Content == null)
            {
                yield break;
            }

            ILocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(response);
            ILocatedOpenApiElement<OpenApiSchema>? schema = mediaType?.GetSchema();
            if (schema == null)
            {
                yield break;
            }

            ITypeGenerator schemaGenerator = Context.TypeGeneratorRegistry.Get(schema);

            TypeSyntax returnType = schemaGenerator.TypeInfo.Name;

            yield return MethodDeclaration(
                default,
                TokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword)),
                WellKnownTypes.System.Threading.Tasks.ValueTaskT.Name(returnType),
                null,
                Identifier(GetBodyMethodName),
                null,
                ParameterList(SingletonSeparatedList(MethodHelpers.DefaultedCancellationTokenParameter())),
                default,
                Block(GenerateStatements(response, returnType)),
                null);
        }

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(
            ILocatedOpenApiElement<OpenApiResponse> response, TypeSyntax returnType)
        {
            // Return from _body field if not null, otherwise deserialize and set the _body field

            static ReturnStatementSyntax BuildReturnStatement(ExpressionSyntax taskExpression) =>
                ReturnStatement(AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                    IdentifierName(ResponseTypeGenerator.BodyFieldName),
                    SyntaxHelpers.AwaitConfiguredFalse(taskExpression)));

            if (!returnType.IsEquivalentTo(WellKnownTypes.System.IO.Stream.Name))
            {
                yield return BuildReturnStatement(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SerializationNamespace.TypeSerializerRegistryExtensions,
                        GenericName(Identifier("DeserializeAsync"),
                            TypeArgumentList(SingletonSeparatedList(returnType)))),
                    ArgumentList(SeparatedList(new[]
                    {
                        Argument(IdentifierName("TypeSerializerRegistry")), Argument(MemberAccessExpression(
                            SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("Message"),
                            IdentifierName("Content"))),
                        Argument(NameColon("cancellationToken"), default, IdentifierName("cancellationToken"))
                    }))));
            }
            else
            {
                // We're dealing with System.IO.Stream so we can just return the stream without deserializing.
                // However, we need to deal with the lack of cancellation tokens in the .NET Standard 2.0 version.

                ExpressionSyntax bodyTaskExpression = Context.PreprocessorSymbols.Contains("NET5_0_OR_GREATER")
                    ? InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("Message"),
                                IdentifierName("Content")),
                            IdentifierName("ReadAsStreamAsync")),
                        ArgumentList(SingletonSeparatedList(
                            Argument(IdentifierName("cancellationToken"))
                        )))
                    : InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                IdentifierName("Message"),
                                IdentifierName("Content")),
                            IdentifierName("ReadAsStreamAsync")));

                yield return BuildReturnStatement(bodyTaskExpression);
            }
        }

        public static InvocationExpressionSyntax InvokeGetBody(ExpressionSyntax requestInstance) =>
            InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        requestInstance,
                        IdentifierName(GetBodyMethodName)));
    }
}
