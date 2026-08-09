using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Yardarm.Generation
{
    public interface ISyntaxTreeGenerator
    {
        IEnumerable<SyntaxTree> Generate();
    }
}
