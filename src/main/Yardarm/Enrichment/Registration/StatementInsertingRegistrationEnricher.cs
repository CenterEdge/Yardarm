using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Enrichment.Registration;

/// <summary>
/// Common implementation for an <see cref="IRegistrationEnricher"> that inserts statements before the return
/// at the end of the method.
/// </summary>
public abstract class StatementInsertingRegistrationEnricher : IRegistrationEnricher
{
    public BlockSyntax Enrich(BlockSyntax target)
    {
        int returnStatementIndex = target.Statements.LastIndexOf(p => p is ReturnStatementSyntax);
        ExpressionSyntax? returnExpression = returnStatementIndex >= 0
            ? ((ReturnStatementSyntax)target.Statements[returnStatementIndex]).Expression
            : null;

        IEnumerable<StatementSyntax> newStatements = GenerateStatements(returnExpression);
        if (newStatements == Enumerable.Empty<StatementSyntax>() ||
            newStatements is ICollection<StatementSyntax> { Count: 0 })
        {
            // No statements added, short-circuit
            return target;
        }

        return target.WithStatements(
            target.Statements.InsertRange(returnStatementIndex, newStatements));
    }

    /// <summary>
    /// Generates the statements to insert before the return statement.
    /// </summary>
    /// <param name="returnExpression">The expression being returned in the return statement.</param>
    /// <returns></returns>
    protected abstract IEnumerable<StatementSyntax> GenerateStatements(ExpressionSyntax? returnExpression);
}
