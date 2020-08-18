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

        public GetBodyMethodGenerator(IMediaTypeSelector mediaTypeSelector, GenerationContext context)
        {
            MediaTypeSelector = mediaTypeSelector ?? throw new ArgumentNullException(nameof(mediaTypeSelector));
            Context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public MethodDeclarationSyntax? Generate(LocatedOpenApiElement<OpenApiResponse> response)
        {
            if (response.Element.Content == null)
            {
                return null;
            }

            LocatedOpenApiElement<OpenApiMediaType>? mediaType = MediaTypeSelector.Select(response);
            if (mediaType?.Element.Schema == null)
            {
                return null;
            }

            ITypeGenerator schemaGenerator = Context.SchemaGeneratorRegistry.Get(
                mediaType.CreateChild(mediaType.Element.Schema, "Body"));

            TypeSyntax returnType = schemaGenerator.TypeName;

            return MethodDeclaration(
                    WellKnownTypes.System.Threading.Tasks.ValueTaskT.Name(returnType),
                    GetBodyMethodName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword))
                .WithBody(Block(GenerateStatements(response, returnType)));
        }

        protected virtual IEnumerable<StatementSyntax> GenerateStatements(
            LocatedOpenApiElement<OpenApiResponse> response, TypeSyntax returnType)
        {
            yield return ReturnStatement(SyntaxHelpers.AwaitConfiguredFalse(
                InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        Context.NamespaceProvider.GetTypeSerializerRegistryExtensions(),
                        GenericName("DeserializeAsync")
                            .AddTypeArgumentListArguments(returnType)))
                    .AddArgumentListArguments(
                        Argument(IdentifierName(ResponseBaseTypeGenerator.TypeSerializerRegistryPropertyName)),
                        Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName(ResponseBaseInterfaceTypeGenerator.MessageProperty),
                            IdentifierName("Content"))))));
        }

        public static InvocationExpressionSyntax InvokeGetBody(ExpressionSyntax requestInstance) =>
            InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        requestInstance,
                        IdentifierName(GetBodyMethodName)));
    }
}
