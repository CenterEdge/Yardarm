using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi;
using Yardarm.Generation.MediaType;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Serialization;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    public class GetBodyMethodGenerator : IResponseMethodGenerator
    {
        public const string GetBodyMethodName = "GetBodyAsync";

        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected ISerializerSelector SerializerSelector { get; }
        protected GenerationContext Context { get; }
        protected ISerializationNamespace SerializationNamespace { get; }
        protected IResponseBodyResolver ResponseBodyResolver { get; }

        public GetBodyMethodGenerator(IMediaTypeSelector mediaTypeSelector, ISerializerSelector serializerSelector,
            GenerationContext context, ISerializationNamespace serializationNamespace, IResponseBodyResolver responseBodyResolver)
        {
            ArgumentNullException.ThrowIfNull(mediaTypeSelector);
            ArgumentNullException.ThrowIfNull(serializerSelector);
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(serializationNamespace);
            ArgumentNullException.ThrowIfNull(responseBodyResolver);

            MediaTypeSelector = mediaTypeSelector;
            SerializerSelector = serializerSelector;
            Context = context;
            SerializationNamespace = serializationNamespace;
            ResponseBodyResolver = responseBodyResolver;
        }

        public IEnumerable<BaseMethodDeclarationSyntax> Generate(ILocatedOpenApiElement<IOpenApiResponse> response, string className)
        {
            if (response.Element is IOpenApiReferenceHolder)
            {
                // Do not generator for responses within operations that are references to components, these will inherit
                // their get body method from the component base class
                yield break;
            }

            if (response.Element.Content == null)
            {
                yield break;
            }

            TypeSyntax? returnType = ResponseBodyResolver.Resolve(response)?.BodyType;
            if (returnType == null)
            {
                yield break;
            }

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
            ILocatedOpenApiElement<IOpenApiResponse> response, TypeSyntax returnType)
        {
            // Return from _body field if not null, otherwise deserialize and set the _body field

            static ReturnStatementSyntax BuildReturnStatement(ExpressionSyntax taskExpression) =>
                ReturnStatement(AssignmentExpression(SyntaxKind.CoalesceAssignmentExpression,
                    IdentifierName(ResponseTypeGenerator.BodyFieldName),
                    SyntaxHelpers.AwaitConfiguredFalse(taskExpression)));

            if (!returnType.IsEquivalentTo(WellKnownTypes.System.IO.Stream.Name))
            {
                // List bodies, including sequential media types such as JSON Lines and streamed bodies, deserialize the items
                // with the item type known at compile time. Serializers which only support single values fall back to
                // regular deserialization. This allows for compatible media types that aren't selected, such as JSON Lines,
                // since the serializer is chosen by the Content-Type of the response.
                SimpleNameSyntax deserializeMethod =
                    ResponseBodyResolver.Resolve(response)?.ItemType is { } itemType
                        ? GenericName(Identifier("DeserializeSequenceAsync"),
                            TypeArgumentList(SeparatedList([returnType, itemType])))
                        : GenericName(Identifier("DeserializeAsync"),
                            TypeArgumentList(SingletonSeparatedList(returnType)));

                yield return BuildReturnStatement(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SerializationNamespace.TypeSerializerRegistryExtensions,
                        deserializeMethod),
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
