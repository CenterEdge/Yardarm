using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment.Responses;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    public class ResponseSetTypeGenerator : TypeGeneratorBase<OpenApiResponses>
    {
        protected OpenApiResponses ResponseSet => Element.Element;
        protected OpenApiOperation Operation { get; }
        protected IResponsesNamespace ResponsesNamespace { get; }

        public ResponseSetTypeGenerator(ILocatedOpenApiElement<OpenApiResponses> element, GenerationContext context,
            IResponsesNamespace responsesNamespace)
            : base(element, context, null)
        {
            ResponsesNamespace = responsesNamespace ?? throw new ArgumentNullException(nameof(responsesNamespace));

            Operation = element.Parent?.Element as OpenApiOperation ?? throw new ArgumentException("Parent must be an OpenApiOperation", nameof(element));
        }

        protected override YardarmTypeInfo GetTypeInfo()
        {
            var ns = Context.NamespaceProvider.GetNamespace(Element);

            return new YardarmTypeInfo(
                QualifiedName(ns, IdentifierName(GetInterfaceName())),
                NameKind.Interface);
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            TypeSyntax interfaceNameAndNamespace = TypeInfo.Name;

            var baseTypeFeature = Context.GenerationServices.GetRequiredService<IResponseBaseTypeRegistry>();

            foreach (var response in Element.GetResponses())
            {
                // Register the referenced response to implement this interface

                baseTypeFeature.AddBaseType(response,
                    SimpleBaseType(interfaceNameAndNamespace));
            }

            baseTypeFeature.AddBaseType(Element.GetUnknownResponse(),
                SimpleBaseType(interfaceNameAndNamespace));

            var interfaceName = GetInterfaceName();

            InterfaceDeclarationSyntax declaration = InterfaceDeclaration(interfaceName)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SimpleBaseType(ResponsesNamespace.IOperationResponse));

            yield return declaration;
        }

        private string GetInterfaceName() => Context.NameFormatterSelector.GetFormatter(NameKind.Interface).Format(Operation.OperationId + "Response");
    }
}
