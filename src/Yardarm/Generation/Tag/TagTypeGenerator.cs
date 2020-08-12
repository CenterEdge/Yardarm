using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Tag
{
    public class TagTypeGenerator : TypeGeneratorBase<OpenApiTag>
    {
        protected OpenApiTag Tag => Element.Element;

        public TagTypeGenerator(LocatedOpenApiElement<OpenApiTag> tagElement, GenerationContext context)
            : base(tagElement, context)
        {
        }

        public override TypeSyntax GetTypeName() =>
            SyntaxFactory.QualifiedName(
                Context.NamespaceProvider.GetNamespace(Element),
                SyntaxFactory.IdentifierName(GetInterfaceName()));

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            yield return GenerateInterface();
            yield return GenerateClass();
        }

        protected virtual MemberDeclarationSyntax GenerateInterface()
        {
            var declaration = InterfaceDeclaration(GetInterfaceName())
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    GetOperations()
                        .SelectMany(GenerateOperationMethodHeader,
                            (operation, method) => new {operation, method})
                        .Select(p => p.method
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))
                            .Enrich(Context.Enrichers.Requests.RequestInterfaceMethod, p.operation))
                        .ToArray<MemberDeclarationSyntax>());

            return declaration;
        }

        protected virtual MemberDeclarationSyntax GenerateClass()
        {
            var declaration = ClassDeclaration(GetClassName())
                .AddBaseListTypes(SimpleBaseType(GetTypeName()))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    GetOperations()
                        .SelectMany(GenerateOperationMethodHeader,
                            (operation, method) => new {operation, method})
                        .Select(p => p.method
                            .AddModifiers(Token(SyntaxKind.PublicKeyword))
                            .WithBody(Block(
                                ThrowStatement(ObjectCreationExpression(
                                        QualifiedName(IdentifierName("System"), IdentifierName("NotImplementedException")))
                                        .WithArgumentList(ArgumentList()))
                                    .WithSemicolonToken(Token(SyntaxKind.SemicolonToken))))
                            .Enrich(Context.Enrichers.Requests.RequestClassMethod, p.operation))
                        .ToArray<MemberDeclarationSyntax>());

            return declaration;
        }

        protected virtual IEnumerable<MethodDeclarationSyntax> GenerateOperationMethodHeader(
            LocatedOpenApiElement<OpenApiOperation> operation)
        {
            TypeSyntax requestType = Context.TypeNameProvider.GetName(operation);
            TypeSyntax responseType = WellKnownTypes.TaskT(Context.TypeNameProvider.GetName(operation.CreateChild(operation.Element.Responses, "")));

            string methodName = Context.NameFormatterSelector.GetFormatter(NameKind.AsyncMethod)
                .Format(operation.Element.OperationId);

            var methodDeclaration = SyntaxFactory.MethodDeclaration(responseType, methodName)
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                        .WithType(requestType),
                    SyntaxHelpers.DefaultedCancellationTokenParameter());

            yield return methodDeclaration;
        }

        private string GetInterfaceName() => Context.NameFormatterSelector.GetFormatter(NameKind.Interface).Format(Tag.Name);

        private string GetClassName() => Context.NameFormatterSelector.GetFormatter(NameKind.Class).Format(Tag.Name);

        private IEnumerable<LocatedOpenApiElement<OpenApiOperation>> GetOperations() =>
            Context.Document.Paths.ToLocatedElements()
                .GetOperations()
                .Where(p => p.Element.Tags.Any(q => q.Name == Tag.Name));
    }
}
