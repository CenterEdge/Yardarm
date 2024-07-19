using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    internal class UnknownResponseTypeGenerator(
        ILocatedOpenApiElement<OpenApiUnknownResponse> responseElement,
        GenerationContext context,
        ISerializationNamespace serializationNamespace,
        IResponsesNamespace responsesNamespace,
        IOperationNameProvider operationNameProvider) :
        TypeGeneratorBase<OpenApiUnknownResponse>(responseElement, context, null)
    {
        protected IResponsesNamespace ResponsesNamespace { get; } = responsesNamespace;
        protected ISerializationNamespace SerializationNamespace { get; } = serializationNamespace;

        protected override YardarmTypeInfo GetTypeInfo()
        {
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            return new YardarmTypeInfo(QualifiedName(ns, IdentifierName(GetClassName())));
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

            ILocatedOpenApiElement<OpenApiOperation> operation =
                Element.Parents().OfType<ILocatedOpenApiElement<OpenApiOperation>>().First();

            string? operationName = operationNameProvider.GetOperationName(operation);
            Debug.Assert(operationName is not null);

            return formatter.Format($"{operationName}UnknownResponse");
        }
    }
}
