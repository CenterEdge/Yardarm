using System.Collections.Generic;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Generation.Schema
{
    public interface ISchemaGenerator
    {
        void Preprocess();

        TypeSyntax GetTypeName();

        SyntaxTree? GenerateSyntaxTree();

        IEnumerable<MemberDeclarationSyntax> Generate();
    }
}
