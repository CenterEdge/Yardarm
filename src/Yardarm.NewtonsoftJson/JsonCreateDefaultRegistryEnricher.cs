using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Enrichment;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NewtonsoftJson
{
    public class JsonCreateDefaultRegistryEnricher : ICreateDefaultRegistryEnricher
    {
        private readonly GenerationContext _context;

        public int Priority => 0;

        public JsonCreateDefaultRegistryEnricher(GenerationContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public ExpressionSyntax Enrich(ExpressionSyntax target)
        {
            TypeSyntax jsonTypeSerializerName = QualifiedName(
                _context.NamespaceProvider.GetSerializationNamespace(),
                IdentifierName("JsonTypeSerializer"));

            return InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    target,
                    IdentifierName("Add")))
                .AddArgumentListArguments(
                    Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        jsonTypeSerializerName,
                        IdentifierName("SupportedMediaTypes"))),
                    Argument(ObjectCreationExpression(jsonTypeSerializerName)));
        }
    }
}
