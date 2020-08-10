using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Helpers;
using Yardarm.Names;

namespace Yardarm.Generation.Tag
{
    public class TagInterfaceTypeGenerator : TypeGeneratorBase<OpenApiTag>
    {
        protected OpenApiTag Tag => Element.Element;

        public TagInterfaceTypeGenerator(LocatedOpenApiElement<OpenApiTag> tagElement, GenerationContext context)
            : base(tagElement, context)
        {
        }

        public override TypeSyntax GetTypeName() =>
            SyntaxFactory.QualifiedName(
                Context.NamespaceProvider.GetNamespace(Element),
                SyntaxFactory.IdentifierName(GetInterfaceName()));

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var declaration = SyntaxFactory.InterfaceDeclaration(GetInterfaceName())
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    GetOperations().SelectMany(GenerateOperationMethod).ToArray());

            yield return declaration;
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> GenerateOperationMethod(
            LocatedOpenApiElement<OpenApiOperation> operation)
        {
            TypeSyntax requestType = Context.TypeNameProvider.GetName(operation);
            TypeSyntax responseType = SyntaxHelpers.TaskT(SyntaxFactory.IdentifierName("dynamic"));

            string methodName = Context.NameFormatterSelector.GetFormatter(NameKind.AsyncMethod)
                .Format(operation.Element.OperationId);

            var methodDeclaration = SyntaxFactory.MethodDeclaration(responseType, methodName)
                .AddParameterListParameters(
                    SyntaxFactory.Parameter(SyntaxFactory.Identifier("request"))
                        .WithType(requestType),
                    SyntaxHelpers.DefaultedCancellationTokenParameter())
                .WithSemicolonToken(SyntaxFactory.Token(SyntaxKind.SemicolonToken));

            yield return methodDeclaration.Enrich(Context.Enrichers.OperationMethod, operation);
        }

        private string GetInterfaceName() => Context.NameFormatterSelector.GetFormatter(NameKind.Interface).Format(Tag.Name);

        private IEnumerable<LocatedOpenApiElement<OpenApiOperation>> GetOperations() =>
            Context.Document.Paths.ToLocatedElements()
                .GetOperations()
                .Where(p => p.Element.Tags.Any(q => q.Name == Tag.Name));
    }
}
