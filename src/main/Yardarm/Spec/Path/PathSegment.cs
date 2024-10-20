using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Spec.Path
{
    public readonly struct PathSegment
    {
        public string Value { get; }

        public PathSegmentType Type { get; }

        public PathSegment(string value, PathSegmentType type)
        {
            Value = value;
            Type = type;
        }

        public void Deconstruct(out string value, out PathSegmentType type)
        {
            value = Value;
            type = Type;
        }

        public bool Equals(PathSegment other) => Value == other.Value && Type == other.Type;

        public override bool Equals(object? obj) => obj is PathSegment other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(Value, (int) Type);

        public InterpolatedStringContentSyntax ToInterpolatedStringContentSyntax(Func<PathSegment, InterpolationSyntax> parameterInterpreter) =>
            Type == PathSegmentType.Text
                ? InterpolatedStringText(
                    Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, Value, Value, TriviaList()))
                : parameterInterpreter.Invoke(this);
    }
}
