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
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Tag
{
    public class TagTypeGenerator : TypeGeneratorBase<OpenApiTag>
    {
        public const string HttpClientFieldName = "_httpClient";
        public const string TypeSerializerRegistryFieldName = "_typeSerializerRegistry";
        public const string AuthenticatorsFieldName = "_authenticators";

        private readonly ISerializationNamespace _serializationNamespace;
        private readonly IAuthenticationNamespace _authenticationNamespace;
        private readonly IOperationMethodGenerator _operationMethodGenerator;

        protected OpenApiTag Tag => Element.Element;

        public TagTypeGenerator(ILocatedOpenApiElement<OpenApiTag> tagElement, GenerationContext context,
            ISerializationNamespace serializationNamespace, IAuthenticationNamespace authenticationNamespace,
            IOperationMethodGenerator operationMethodGenerator)
            : base(tagElement, context)
        {
            _serializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
            _authenticationNamespace = authenticationNamespace ??
                                       throw new ArgumentNullException(nameof(authenticationNamespace));
            _operationMethodGenerator = operationMethodGenerator ?? throw new ArgumentNullException(nameof(operationMethodGenerator));
        }

        protected override TypeSyntax GetTypeName() =>
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
            string className = GetClassName();

            var declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddBaseListTypes(SimpleBaseType(GetTypeName()))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddMembers(GenerateFields()
                    .Concat<MemberDeclarationSyntax>(GenerateConstructors(className))
                    .Concat(
                        GetOperations()
                            .SelectMany(GenerateOperationMethodHeader,
                                (operation, method) => new {operation, method})
                            .Select(p => p.method
                                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.AsyncKeyword))
                                .WithBody(_operationMethodGenerator.Generate(p.operation))))
                    .ToArray());

            return declaration;
        }

        protected virtual IEnumerable<FieldDeclarationSyntax> GenerateFields()
        {
            yield return FieldDeclaration(VariableDeclaration(WellKnownTypes.System.Net.Http.HttpClient.Name)
                    .AddVariables(
                        VariableDeclarator(HttpClientFieldName)))
                .AddModifiers(
                    Token(SyntaxKind.PrivateKeyword),
                    Token(SyntaxKind.ReadOnlyKeyword));

            yield return FieldDeclaration(VariableDeclaration(_serializationNamespace.ITypeSerializerRegistry)
                    .AddVariables(
                        VariableDeclarator(TypeSerializerRegistryFieldName)))
                .AddModifiers(
                    Token(SyntaxKind.PrivateKeyword),
                    Token(SyntaxKind.ReadOnlyKeyword));

            yield return FieldDeclaration(VariableDeclaration(_authenticationNamespace.Authenticators)
                    .AddVariables(
                        VariableDeclarator(AuthenticatorsFieldName)))
                .AddModifiers(
                    Token(SyntaxKind.PrivateKeyword),
                    Token(SyntaxKind.ReadOnlyKeyword));
        }

        protected virtual IEnumerable<ConstructorDeclarationSyntax> GenerateConstructors(string className)
        {
            yield return ConstructorDeclaration(className)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("httpClient"))
                        .WithType(WellKnownTypes.System.Net.Http.HttpClient.Name),
                    Parameter(Identifier("typeSerializerRegistry"))
                        .WithType(_serializationNamespace.ITypeSerializerRegistry),
                    Parameter(Identifier("authenticators"))
                        .WithType(_authenticationNamespace.Authenticators))
                .WithBody(Block(
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(HttpClientFieldName),
                        MethodHelpers.ArgumentOrThrowIfNull("httpClient"))),
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(TypeSerializerRegistryFieldName),
                        MethodHelpers.ArgumentOrThrowIfNull("typeSerializerRegistry"))),
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(AuthenticatorsFieldName),
                        MethodHelpers.ArgumentOrThrowIfNull("authenticators")))));
        }

        protected virtual IEnumerable<MethodDeclarationSyntax> GenerateOperationMethodHeader(
            ILocatedOpenApiElement<OpenApiOperation> operation)
        {
            TypeSyntax requestType = Context.TypeNameProvider.GetName(operation);
            TypeSyntax responseType = WellKnownTypes.System.Threading.Tasks.TaskT.Name(
                Context.TypeNameProvider.GetName(operation.GetResponseSet()));

            string methodName = Context.NameFormatterSelector.GetFormatter(NameKind.AsyncMethod)
                .Format(operation.Element.OperationId);

            var methodDeclaration = MethodDeclaration(responseType, methodName)
                .AddElementAnnotation(operation, Context.ElementRegistry)
                .AddParameterListParameters(
                    Parameter(Identifier(OperationMethodGenerator.RequestParameterName))
                        .WithType(requestType),
                    MethodHelpers.DefaultedCancellationTokenParameter());

            yield return methodDeclaration;
        }

        private string GetInterfaceName() => Context.NameFormatterSelector.GetFormatter(NameKind.Interface).Format(Tag.Name);

        private string GetClassName() => Context.NameFormatterSelector.GetFormatter(NameKind.Class).Format(Tag.Name);

        private IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> GetOperations() =>
            Context.Document.Paths.ToLocatedElements()
                .GetOperations()
                .Where(p => p.Element.Tags.Any(q => q.Name == Tag.Name));
    }
}
