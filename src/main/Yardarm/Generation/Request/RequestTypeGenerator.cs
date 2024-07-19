using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Generation.Operation;
using Yardarm.Generation.Request.Internal;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Serialization;
using Yardarm.Spec;
using Yardarm.Spec.Path;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Request
{
    public class RequestTypeGenerator(
        ILocatedOpenApiElement<OpenApiOperation> operationElement,
        GenerationContext context,
        IMediaTypeSelector mediaTypeSelector,
        IEnumerable<IRequestMemberGenerator> memberGenerators,
        IRequestsNamespace requestsNamespace,
        ISerializerSelector serializerSelector,
        IOperationNameProvider operationNameProvider)
        : TypeGeneratorBase<OpenApiOperation>(operationElement, context, null)
    {
        protected IMediaTypeSelector MediaTypeSelector { get; } = mediaTypeSelector;
        protected IEnumerable<IRequestMemberGenerator> MemberGenerators { get; } = memberGenerators;
        protected ISerializerSelector SerializerSelector { get; } = serializerSelector;

        protected IRequestsNamespace RequestsNamespace { get; } = requestsNamespace;

        protected OpenApiOperation Operation => Element.Element;

        protected override YardarmTypeInfo GetTypeInfo()
        {
            INameFormatter formatter = Context.NameFormatterSelector.GetFormatter(NameKind.Class);
            NameSyntax ns = Context.NamespaceProvider.GetNamespace(Element);

            string? operationName = operationNameProvider.GetOperationName(Element);
            Debug.Assert(operationName is not null);

            return new YardarmTypeInfo(QualifiedName(ns,
                IdentifierName(formatter.Format(operationName + "Request"))));
        }

        public override QualifiedNameSyntax GetChildName<TChild>(ILocatedOpenApiElement<TChild> child,
            NameKind nameKind) =>
            QualifiedName((NameSyntax)TypeInfo.Name, IdentifierName(
                Context.NameFormatterSelector.GetFormatter(nameKind).Format(child.Key + "-Model")));

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            string className = ((QualifiedNameSyntax)TypeInfo.Name).Right.Identifier.ValueText;

            ClassDeclarationSyntax declaration = ClassDeclaration(className)
                .AddElementAnnotation(Element, Context.ElementRegistry)
                .AddGeneratorAnnotation(this)
                .AddModifiers(Token(SyntaxKind.PublicKeyword))
                .AddBaseListTypes(SimpleBaseType(RequestsNamespace.OperationRequest));

            declaration = declaration.AddMembers(
                GenerateParameterProperties(className)
                    .Concat(MemberGenerators
                        .SelectMany(p => p.Generate(Element, null)))
                    .ToArray());

            yield return declaration;

            if (Element.GetRequestBody()?.GetMediaTypes().Any(p => SerializerSelector.Select(p) == null) ?? false)
            {
                var buildContentMethod = declaration.Members
                    .OfType<MethodDeclarationSyntax>()
                    .First(p => p.Identifier.Text == BuildContentMethodGenerator.BuildContentMethodName);

                var httpContentGenerator =
                    new HttpContentRequestTypeGenerator(Element, Context, this, buildContentMethod, operationNameProvider);

                foreach (var otherMember in httpContentGenerator.Generate())
                {
                    yield return otherMember;
                }
            }
        }

        protected virtual IEnumerable<MemberDeclarationSyntax> GenerateParameterProperties(string className)
        {
            var foundParameters = new HashSet<string>(StringComparer.Ordinal);

            foreach (var parameter in Element.GetAllParameters())
            {
                foundParameters.Add(parameter.Key);

                var schema = parameter.GetSchemaOrDefault();

                yield return CreatePropertyDeclaration(parameter, schema, GetPropertyName(parameter.Key, className));

                if (parameter.Element.Reference == null && schema.Element.Reference == null)
                {
                    foreach (var member in Context.TypeGeneratorRegistry.Get(schema).Generate())
                    {
                        yield return member;
                    }
                }
            }

            if (Element.Parent is ILocatedOpenApiElement<OpenApiPathItem> pathItemElement)
            {
                // Generate properties for route parameters not explicitly defined in the parameters list.
                // Assume they are required strings with a default value of "".

                foreach (PathSegment routeParameter in PathParser.Parse(pathItemElement.Key))
                {
                    if (routeParameter.Type != PathSegmentType.Parameter || foundParameters.Contains(routeParameter.Value))
                    {
                        continue;
                    }

                    string propertyName = GetPropertyName(routeParameter.Value, className);

                    yield return PropertyDeclaration(
                        attributeLists: default,
                        modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                        type: PredefinedType(Token(SyntaxKind.StringKeyword)),
                        explicitInterfaceSpecifier: null,
                        identifier: Identifier(propertyName),
                        accessorList: AccessorList(List([
                            AccessorDeclaration(
                                SyntaxKind.GetAccessorDeclaration,
                                attributeLists: default,
                                modifiers: default,
                                keyword: Token(SyntaxKind.GetKeyword),
                                body: null,
                                expressionBody: null,
                                semicolonToken: Token(SyntaxKind.SemicolonToken)),
                            AccessorDeclaration(
                                SyntaxKind.SetAccessorDeclaration,
                                attributeLists: default,
                                modifiers: default,
                                keyword: Token(SyntaxKind.SetKeyword),
                                body: null,
                                expressionBody: null,
                                semicolonToken: Token(SyntaxKind.SemicolonToken))
                        ])),
                        expressionBody: null,
                        initializer: EqualsValueClause(SyntaxHelpers.StringLiteral("")),
                        semicolonToken: Token(SyntaxKind.SemicolonToken));
                }
            }
        }

        protected virtual PropertyDeclarationSyntax CreatePropertyDeclaration(ILocatedOpenApiElement<OpenApiParameter> parameter,
            ILocatedOpenApiElement<OpenApiSchema> schema, string propertyName)
        {
            TypeSyntax typeName = Context.TypeGeneratorRegistry.Get(schema).TypeInfo.Name;

            return PropertyDeclaration(
                attributeLists: default,
                modifiers: TokenList(Token(SyntaxKind.PublicKeyword)),
                type: typeName,
                explicitInterfaceSpecifier: null,
                identifier: Identifier(propertyName),
                accessorList: AccessorList(List([
                    AccessorDeclaration(
                        SyntaxKind.GetAccessorDeclaration,
                        attributeLists: default,
                        modifiers: default,
                        keyword: Token(SyntaxKind.GetKeyword),
                        body: null,
                        expressionBody: null,
                        semicolonToken: Token(SyntaxKind.SemicolonToken)),
                    AccessorDeclaration(
                        SyntaxKind.SetAccessorDeclaration,
                        attributeLists: default,
                        modifiers: default,
                        keyword: Token(SyntaxKind.SetKeyword),
                        body: null,
                        expressionBody: null,
                        semicolonToken: Token(SyntaxKind.SemicolonToken))
                ])))
                .AddElementAnnotation(parameter, Context.ElementRegistry);
        }

        private string GetPropertyName(string parameterName, string className)
        {
            string propertyName = Context.NameFormatterSelector.GetFormatter(NameKind.Property).Format(parameterName);

            if (propertyName == className)
            {
                propertyName += "Value";
            }

            return propertyName;
        }
    }
}
