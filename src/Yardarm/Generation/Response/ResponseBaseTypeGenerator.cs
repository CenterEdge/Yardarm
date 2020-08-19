using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Helpers;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Response
{
    public class ResponseBaseTypeGenerator : TypeGeneratorBase
    {
        private const string BaseClassName = "OperationResponse";

        public const string TypeSerializerRegistryPropertyName = "TypeSerializerRegistry";

        private readonly IRootNamespace _rootNamespace;
        private readonly ISerializationNamespace _serializationNamespace;
        private readonly ResponseBaseInterfaceTypeGenerator _responseBaseInterfaceTypeGenerator;

        public ResponseBaseTypeGenerator(GenerationContext context,
            IRootNamespace rootNamespace,
            ISerializationNamespace serializationNamespace,
            ResponseBaseInterfaceTypeGenerator responseBaseInterfaceTypeGenerator)
            : base(context)
        {
            _rootNamespace = rootNamespace ?? throw new ArgumentNullException(nameof(rootNamespace));
            _serializationNamespace = serializationNamespace ?? throw new ArgumentNullException(nameof(serializationNamespace));
            _responseBaseInterfaceTypeGenerator = responseBaseInterfaceTypeGenerator ??
                                                  throw new ArgumentNullException(
                                                      nameof(responseBaseInterfaceTypeGenerator));
        }

        protected override TypeSyntax GetTypeName() =>
            QualifiedName(_rootNamespace.Name, IdentifierName(BaseClassName));

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            ClassDeclarationSyntax declaration = ClassDeclaration(BaseClassName)
                .AddBaseListTypes(
                    SimpleBaseType(_responseBaseInterfaceTypeGenerator.TypeName))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.AbstractKeyword))
                .AddMembers(
                    GenerateConstructor(),
                    GenerateMessageProperty(),
                    GenerateTypeSerializerRegistryProperty(),
                    GenerateIsSuccessStatusCodeProperty(),
                    GenerateStatusCodeProperty(),
                    GenerateDisposeMethod());

            yield return declaration;
        }

        #region Constructors

        private ConstructorDeclarationSyntax GenerateConstructor() =>
            ConstructorDeclaration(BaseClassName)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddParameterListParameters(
                    Parameter(Identifier("message"))
                        .WithType(WellKnownTypes.System.Net.Http.HttpResponseMessage.Name),
                    Parameter(Identifier("typeSerializerRegistry"))
                        .WithType(_serializationNamespace.ITypeSerializerRegistry))
                .WithBody(Block(
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(ResponseBaseInterfaceTypeGenerator.MessageProperty),
                        SyntaxHelpers.ParameterWithNullCheck("message"))),
                    ExpressionStatement(AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
                        IdentifierName(ResponseBaseTypeGenerator.TypeSerializerRegistryPropertyName),
                        SyntaxHelpers.ParameterWithNullCheck("typeSerializerRegistry")))
                    ));

        #endregion

        #region Properties

        private PropertyDeclarationSyntax GenerateMessageProperty() =>
            PropertyDeclaration(WellKnownTypes.System.Net.Http.HttpResponseMessage.Name,
                    Identifier(ResponseBaseInterfaceTypeGenerator.MessageProperty))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        private PropertyDeclarationSyntax GenerateTypeSerializerRegistryProperty() =>
            PropertyDeclaration(_serializationNamespace.ITypeSerializerRegistry,
                    Identifier(ResponseBaseTypeGenerator.TypeSerializerRegistryPropertyName))
                .AddModifiers(Token(SyntaxKind.ProtectedKeyword))
                .AddAccessorListAccessors(
                    AccessorDeclaration(SyntaxKind.GetAccessorDeclaration).WithSemicolonToken(Token(SyntaxKind.SemicolonToken)));

        private PropertyDeclarationSyntax GenerateIsSuccessStatusCodeProperty() =>
            PropertyDeclaration(PredefinedType(Token(SyntaxKind.BoolKeyword)),
                    Identifier(ResponseBaseInterfaceTypeGenerator.IsSuccessStatusCodeProperty))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithExpressionBody(ArrowExpressionClause(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(ResponseBaseInterfaceTypeGenerator.MessageProperty), IdentifierName("IsSuccessStatusCode"))));

        private PropertyDeclarationSyntax GenerateStatusCodeProperty() =>
            PropertyDeclaration(WellKnownTypes.System.Net.HttpStatusCode.Name,
                    Identifier(ResponseBaseInterfaceTypeGenerator.StatusCodeProperty))
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .WithExpressionBody(ArrowExpressionClause(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(ResponseBaseInterfaceTypeGenerator.MessageProperty), IdentifierName("StatusCode"))));

        #endregion

        #region Methods

        private MethodDeclarationSyntax GenerateDisposeMethod() =>
            MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), Identifier("Dispose"))
                .AddModifiers(
                    Token(SyntaxKind.PublicKeyword),
                    Token(SyntaxKind.VirtualKeyword))
                .WithBody(Block().AddStatements(ExpressionStatement(
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(ResponseBaseInterfaceTypeGenerator.MessageProperty),
                        IdentifierName("Dispose"))))));

        #endregion
    }
}
