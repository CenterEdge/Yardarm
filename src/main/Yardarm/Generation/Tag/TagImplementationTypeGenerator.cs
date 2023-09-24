using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
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
    public class TagImplementationTypeGenerator : TagTypeGeneratorBase
    {
        public const string HttpClientFieldName = "_httpClient";
        public const string TypeSerializerRegistryFieldName = "_typeSerializerRegistry";
        public const string AuthenticatorsFieldName = "_authenticators";

        private readonly ISerializationNamespace _serializationNamespace;
        private readonly IAuthenticationNamespace _authenticationNamespace;
        private readonly IOperationMethodGenerator _operationMethodGenerator;

        public TagImplementationTypeGenerator(ILocatedOpenApiElement<OpenApiTag> tagElement, GenerationContext context,
            ISerializationNamespace serializationNamespace, IAuthenticationNamespace authenticationNamespace,
            IOperationMethodGenerator operationMethodGenerator)
            : base(tagElement, context)
        {
            ArgumentNullException.ThrowIfNull(serializationNamespace);
            ArgumentNullException.ThrowIfNull(authenticationNamespace);
            ArgumentNullException.ThrowIfNull(operationMethodGenerator);

            _serializationNamespace = serializationNamespace;
            _authenticationNamespace = authenticationNamespace;
            _operationMethodGenerator = operationMethodGenerator;
        }

        protected override YardarmTypeInfo GetTypeInfo() =>
            new YardarmTypeInfo(
            QualifiedName(
                Context.NamespaceProvider.GetNamespace(Element),
                IdentifierName(GetClassName())),
                    NameKind.Class);

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            string className = GetClassName();

            var baseType = Context.TypeGeneratorRegistry.Get<OpenApiTag>(Element);

            var declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddBaseListTypes(SimpleBaseType(baseType.TypeInfo.Name))
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

            yield return declaration;
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
            yield return ConstructorDeclaration(
                default,
                TokenList(Token(SyntaxKind.PublicKeyword)),
                Identifier(className),
                ParameterList(SeparatedList(new[] {
                    Parameter(default, default, WellKnownTypes.System.Net.Http.HttpClient.Name, Identifier("httpClient"), null),
                    Parameter(default, default, _serializationNamespace.ITypeSerializerRegistry, Identifier("typeSerializerRegistry"), null),
                    Parameter(default ,default, _authenticationNamespace.Authenticators, Identifier("authenticators"), null)
                })),
                default,
                Block(
                    MethodHelpers.ThrowIfArgumentNull("httpClient"),
                    MethodHelpers.ThrowIfArgumentNull("typeSerializerRegistry"),
                    MethodHelpers.ThrowIfArgumentNull("authenticators"),
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(HttpClientFieldName),
                        IdentifierName("httpClient"))),
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(TypeSerializerRegistryFieldName),
                        IdentifierName("typeSerializerRegistry"))),
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(AuthenticatorsFieldName),
                        IdentifierName("authenticators")))));
        }

        private string GetClassName() => Context.NameFormatterSelector.GetFormatter(NameKind.Class).Format(Tag.Name);
    }
}
