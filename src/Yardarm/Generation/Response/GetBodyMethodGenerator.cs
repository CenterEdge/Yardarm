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
    public class GetBodyMethodGenerator : IGetBodyMethodGenerator
    {
        public const string GetBodyMethodName = "GetBodyAsync";

        protected IMediaTypeSelector MediaTypeSelector { get; }
        protected GenerationContext Context { get; }
        protected ISerializationNamespace SerializationNamespace { get; }

        public GetBodyMethodGenerator(IMediaTypeSelector mediaTypeSelector, GenerationContext context,
            ISerializationNamespace serializationNamespace)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            Context = context ?? throw new ArgumentNullException(nameof(context));
            SerializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
        }

        public MethodDeclarationSyntax? Generate(ILocatedOpenApiElement<OpenApiResponse> response)
        {
            if (response.Element.Content == null)
            {
                return null;
            }

            ILocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(response);
            ILocatedOpenApiElement<OpenApiSchema>? schema = mediaType?.GetSchema();
            if (schema == null)
            {
                return null;
            }

            ITypeGenerator schemaGenerator = Context.TypeGeneratorRegistry.Get(schema);

            TypeSyntax returnType = schemaGenerator.TypeInfo.Name;

            return MethodDeclaration(
                    WellKnownTypes.System.Threading.Tasks.ValueTaskT.Name(returnType),
                    GetBodyMethodName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword))
                .WithBody(Block(GenerateStatements(response, returnType)));
        }

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(
            ILocatedOpenApiElement<OpenApiResponse> response, TypeSyntax returnType)
        {
            yield return ReturnStatement(SyntaxHelpers.AwaitConfiguredFalse(
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        SerializationNamespace.TypeSerializerRegistryExtensions,
                        GenericName("DeserializeAsync")
                            .AddTypeArgumentListArguments(returnType)))
                    .AddArgumentListArguments(
                        Argument(IdentifierName("TypeSerializerRegistry")),
                        Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("Message"),
                            IdentifierName("Content"))))));
        }

        public static InvocationExpressionSyntax InvokeGetBody(ExpressionSyntax requestInstance) =>
            InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        requestInstance,
                        IdentifierName(GetBodyMethodName)));
    }
}
