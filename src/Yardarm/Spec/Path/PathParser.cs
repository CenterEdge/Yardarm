using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Spec.Path
{
    public static class PathParser
    {
        public static PathSegment[] Parse(string path)
        {
            if (path == null)
            {
                throw new ArgumentNullException(nameof(path));
            }

            return ParseInternal(path).ToArray();
        }

        private static IEnumerable<PathSegment> ParseInternal(string path)
        {
            int index = 0;

            if (path.StartsWith("/"))
            {
                index = 1;
            }

            while (index < path.Length)
            {
                int nextParameter = path.IndexOf('{', index);
                if (nextParameter > index || nextParameter < 0)
                {
                    yield return new PathSegment(
                        path.Substring(index, (nextParameter < 0 ? path.Length : nextParameter) - index),
                        PathSegmentType.Text);
                }

                if (nextParameter >= 0)
                {
                    int endParameter = path.IndexOf('}', nextParameter);
                    if (endParameter < 0)
                    {
                        throw new InvalidOperationException("Invalid path segment, missing closing }");
                    }

                    string parameterName = path.Substring(nextParameter + 1, endParameter - nextParameter - 1);

                    yield return new PathSegment(parameterName, PathSegmentType.Parameter);

                    index = endParameter + 1;
                }
                else
                {
                    index = path.Length;
                }
            }
        }

        public static InterpolatedStringExpressionSyntax ToInterpolatedStringExpression(
            this IEnumerable<PathSegment> pathSegments, Func<PathSegment, ExpressionSyntax> parameterInterpreter) =>
            InterpolatedStringExpression(Token(SyntaxKind.InterpolatedStringStartToken),
                List(pathSegments.Select(p => p.ToInterpolatedStringContentSyntax(parameterInterpreter))));
    }
}
