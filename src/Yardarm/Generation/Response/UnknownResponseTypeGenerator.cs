using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    internal class UnknownResponseTypeGenerator : TypeGeneratorBase<OpenApiUnknownResponse>
    {
        protected IResponsesNamespace ResponsesNamespace { get; }
        protected ISerializationNamespace SerializationNamespace { get; }

        public UnknownResponseTypeGenerator(ILocatedOpenApiElement<OpenApiUnknownResponse> responseElement, GenerationContext context,
            ISerializationNamespace serializationNamespace,
            IResponsesNamespace responsesNamespace)
            : base(responseElement, context)
        {
            SerializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
            ResponsesNamespace = responsesNamespace ?? throw new ArgumentNullException(nameof(responsesNamespace));
        }

        protected override TypeSyntax GetTypeName()
        {
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            return QualifiedName(ns, IdentifierName(GetClassName()));
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            string className = GetClassName();

            ClassDeclarationSyntax declaration = ClassDeclaration(className)
                .AddElementAnnotation<ClassDeclarationSyntax, OpenApiResponse>(Element, Context.ElementRegistry)
                .AddBaseListTypes(SimpleBaseType(ResponsesNamespace.UnknownResponse))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    GenerateConstructor(className));

            yield return declaration;
        }

        private ConstructorDeclarationSyntax GenerateConstructor(string className) =>
            ConstructorDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("message"))
                        .WithType(WellKnownTypes.System.Net.Http.HttpResponseMessage.Name),
                    Parameter(Identifier("typeSerializerRegistry"))
                        .WithType(SerializationNamespace.ITypeSerializerRegistry))
                .WithInitializer(ConstructorInitializer(SyntaxKind.BaseConstructorInitializer)
                    .AddArgumentListArguments(
                        Argument(IdentifierName("message")),
                        Argument(IdentifierName("typeSerializerRegistry"))))
                .WithBody(Block());

        private string GetClassName()
        {
            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);

            OpenApiOperation operation =
                Element.Parents.OfType<LocatedOpenApiElement<OpenApiOperation>>().First().Element;

            return formatter.Format($"{operation.OperationId}UnknownResponse");
        }
    }
}
