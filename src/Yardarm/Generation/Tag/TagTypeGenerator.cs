using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using Yardarm.Spec.Path;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Tag
{
    public class TagTypeGenerator : TypeGeneratorBase<OpenApiTag>
    {
        public const string RequestParameterName = "request";

        private readonly IOperationMethodGenerator _operationMethodGenerator;

        protected OpenApiTag Tag => Element.Element;

        public TagTypeGenerator(LocatedOpenApiElement<OpenApiTag> tagElement, GenerationContext context,
            IOperationMethodGenerator operationMethodGenerator)
            : base(tagElement, context)
        {
            _operationMethodGenerator = operationMethodGenerator ?? throw new ArgumentNullException(nameof(operationMethodGenerator));
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
                            .WithSemicolonToken(Token(SyntaxKind.SemicolonToken)))
                        .ToArray<MemberDeclarationSyntax>());

            return declaration;
        }

        protected virtual MemberDeclarationSyntax GenerateClass()
        {
            var declaration = ClassDeclaration(GetClassName())
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddBaseListTypes(SimpleBaseType(GetTypeName()))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(
                    GetOperations()
                        .SelectMany(GenerateOperationMethodHeader,
                            (operation, method) => new {operation, method})
                        .Select(p => p.method
                            .AddModifiers(Token(SyntaxKind.PublicKeyword))
                            .WithBody(_operationMethodGenerator.Generate(p.operation)))
                        .ToArray<MemberDeclarationSyntax>());

            return declaration;
        }

        protected virtual IEnumerable<MethodDeclarationSyntax> GenerateOperationMethodHeader(
            LocatedOpenApiElement<OpenApiOperation> operation)
        {
            TypeSyntax requestType = Context.TypeNameProvider.GetName(operation);
            TypeSyntax responseType = WellKnownTypes.System.Threading.Tasks.TaskT.Name(
                Context.TypeNameProvider.GetName(operation.CreateChild(operation.Element.Responses, "")));

            string methodName = Context.NameFormatterSelector.GetFormatter(NameKind.AsyncMethod)
                .Format(operation.Element.OperationId);

            var methodDeclaration = MethodDeclaration(responseType, methodName)
                .AddElementAnnotation(operation, Context.ElementRegistry)
                .AddParameterListParameters(
                    Parameter(Identifier(RequestParameterName))
                        .WithType(requestType),
                    MethodHelpers.DefaultedCancellationTokenParameter());

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
