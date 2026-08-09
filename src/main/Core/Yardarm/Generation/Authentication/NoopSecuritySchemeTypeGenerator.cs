using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Authentication
{
    public class NoopSecuritySchemeTypeGenerator : SecuritySchemeTypeGenerator
    {
        private readonly ILogger<NoopSecuritySchemeTypeGenerator> _logger;

        public NoopSecuritySchemeTypeGenerator(ILocatedOpenApiElement<OpenApiSecurityScheme> securitySchemeElement, GenerationContext context,
            IAuthenticationNamespace authenticationNamespace, ILogger<NoopSecuritySchemeTypeGenerator> logger)
            : base(securitySchemeElement, context, authenticationNamespace)
        {
            ArgumentNullException.ThrowIfNull(logger);

            _logger = logger;
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            _logger.LogWarning("Unsupported security scheme '{schemeName}', implementing no-op placeholder.", SecurityScheme.Name ?? Element.Key);

            return base.Generate();
        }

        protected override BlockSyntax GenerateApplyAsyncBody() => Block(
            MethodHelpers.ThrowIfArgumentNull(MessageParameterName),

            ReturnStatement(LiteralExpression(SyntaxKind.DefaultLiteralExpression)));
    }
}
