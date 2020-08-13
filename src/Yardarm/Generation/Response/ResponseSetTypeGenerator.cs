using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ResponseBaseInterfaceTypeGenerator _responseBaseInterfaceTypeGenerator;
        private readonly IHttpResponseCodeNameProvider _httpResponseCodeNameProvider;

        protected OpenApiResponses Responses => Element.Element;
        protected OpenApiOperation Operation { get; }

        public ResponseSetTypeGenerator(LocatedOpenApiElement<OpenApiResponses> element, GenerationContext context,
            ResponseBaseInterfaceTypeGenerator responseBaseInterfaceTypeGenerator,
            IHttpResponseCodeNameProvider httpResponseCodeNameProvider)
            : base(element, context)
        {
            _responseBaseInterfaceTypeGenerator = responseBaseInterfaceTypeGenerator ??
                                                  throw new ArgumentNullException(
                                                      nameof(responseBaseInterfaceTypeGenerator));
            _httpResponseCodeNameProvider = httpResponseCodeNameProvider ??
                                            throw new ArgumentNullException(nameof(httpResponseCodeNameProvider));

            Operation = (OpenApiOperation)element.Parents[0].Element;
        }

        public override TypeSyntax GetTypeName()
        {
            var ns = Context.NamespaceProvider.GetNamespace(Element);

            return QualifiedName(ns, IdentifierName(GetInterfaceName()));
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            TypeSyntax interfaceNameAndNamespace = GetTypeName();

            var baseTypeFeature = Context.GenerationServices.GetRequiredService<IResponseBaseTypeRegistry>();

            foreach (var response in Responses
                .Select(p => Element.CreateChild(p.Value, p.Key)))
            {
                // Register the referenced response to implement this interface

                baseTypeFeature.AddBaseType(response,
                    SyntaxFactory.SimpleBaseType(interfaceNameAndNamespace));
            }

            var interfaceName = GetInterfaceName();

            InterfaceDeclarationSyntax declaration = InterfaceDeclaration(interfaceName)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SimpleBaseType(_responseBaseInterfaceTypeGenerator.GetTypeName()));

            yield return declaration;
        }

        private string GetInterfaceName() => Context.NameFormatterSelector.GetFormatter(NameKind.Interface).Format(Operation.OperationId + "Response");
    }
}
