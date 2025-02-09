using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Enrichment.Registration;

/// <summary>
/// Common implementation for an <see cref="IRegistrationEnricher"> that modifies the return value of a method.
/// </summary>
public abstract class ReturnValueRegistrationEnricher : IRegistrationEnricher
{
    public BlockSyntax Enrich(BlockSyntax target)
    {
        int returnStatementIndex = target.Statements.LastIndexOf(p => p is ReturnStatementSyntax);
        if (returnStatementIndex < 0)
        {
            return target;
        }

        ExpressionSyntax? returnExpression = ((ReturnStatementSyntax)target.Statements[returnStatementIndex]).Expression;
        if (returnExpression is not IdentifierNameSyntax identifier)
        {
            return target;
        }

        ExpressionSyntax newReturnValue = EnrichReturnValue(identifier);
        if (newReturnValue == returnExpression)
        {
            // Was not changed
            return target;
        }

        ExpressionStatementSyntax statement = ExpressionStatement(AssignmentExpression(
            Microsoft.CodeAnalysis.CSharp.SyntaxKind.SimpleAssignmentExpression,
            identifier,
            newReturnValue));

        return target.WithStatements(
            target.Statements.Insert(returnStatementIndex, statement));
    }

    protected abstract ExpressionSyntax EnrichReturnValue(ExpressionSyntax expression);
}
