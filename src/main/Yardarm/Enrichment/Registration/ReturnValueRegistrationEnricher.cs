using System.Collections.Generic;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Registration;

/// <summary>
/// Common implementation for an <see cref="IRegistrationEnricher"> that modifies the return value of a method.
/// </summary>
public abstract class ReturnValueRegistrationEnricher : StatementInsertingRegistrationEnricher
{
    protected override sealed IEnumerable<StatementSyntax> GenerateStatements(ExpressionSyntax? returnExpression)
    {
        if (returnExpression is not IdentifierNameSyntax identifier)
        {
            return [];
        }

        ExpressionSyntax newReturnValue = EnrichReturnValue(identifier);
        if (newReturnValue == returnExpression)
        {
            // Was not changed
            return [];
        }

        ExpressionStatementSyntax statement = ExpressionStatement(AssignmentExpression(
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression,
            identifier,
            newReturnValue));

        return [statement];
    }

    protected abstract ExpressionSyntax EnrichReturnValue(ExpressionSyntax expression);
}
