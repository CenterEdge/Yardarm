using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Yardarm.Enrichment;
using Yardarm.Names;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.NodaTime;

/// <summary>
/// Enriches the JsonSerializerOptionsAttribute on the JsonSerizalizerContext class when System.Text.Json is used.
/// </summary>
public sealed class NodaTimeJsonSourceGenerationOptionsEnricher(
    ISerializationNamespace serializationNamespace)
    : IEnricher<AttributeSyntax>
{
    public AttributeSyntax Enrich(AttributeSyntax target)
    {
        AttributeArgumentSyntax? currentArgument = target.ArgumentList?.Arguments
            .FirstOrDefault(p => p.NameEquals?.Name.Identifier.ValueText == "Converters");

        TypeOfExpressionSyntax typeOfExpression =
            TypeOfExpression(QualifiedName(
                QualifiedName(serializationNamespace.Name, IdentifierName("Json")),
                IdentifierName("YardarmNodaTimeJsonConverterFactory")));
        ExpressionSyntax expression = currentArgument?.Expression switch
        {
            CollectionExpressionSyntax collectionExpression =>
                collectionExpression.AddElements(ExpressionElement(typeOfExpression)),
            ArrayCreationExpressionSyntax arrayCreationExpression => arrayCreationExpression.WithInitializer(
                arrayCreationExpression.Initializer!.AddExpressions(typeOfExpression)),
            _ => CollectionExpression(SingletonSeparatedList<CollectionElementSyntax>(ExpressionElement(typeOfExpression)))
        };

        AttributeArgumentSyntax newArgument = currentArgument?.WithExpression(expression)
            ?? AttributeArgument(
                nameEquals: NameEquals(IdentifierName("Converters")),
                nameColon: null,
                expression);

        if (target.ArgumentList is null)
        {
            // No arguments defined, add a new argument list
            return target.WithArgumentList(AttributeArgumentList(SingletonSeparatedList(newArgument)));
        }
        else if (currentArgument is null)
        {
            // No converters defined, add a new argument
            return target.WithArgumentList(target.ArgumentList.AddArguments(newArgument));
        }
        else
        {
            // Adding an additional converter
            return target.WithArgumentList(target.ArgumentList.ReplaceNode(currentArgument, newArgument));
        }
    }
}
