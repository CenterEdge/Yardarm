using System;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Spec.Path
{
    public readonly struct PathSegment
    {
        private static readonly char[] _trimChars = {'.', ';', '*'};

        public string Value { get; }

        public PathSegmentType Type { get; }

        /// <summary>
        /// Value trimmed of special chars.
        /// </summary>
        public string TrimmedName => Value.Trim(_trimChars);

        /// <summary>
        /// Provides the PathSegmentStyle enum value to use when serializing.
        /// </summary>
        public IdentifierNameSyntax Style => Value[0] switch
        {
            '.' => IdentifierName("Label"),
            ';' => IdentifierName("Matrix"),
            _ => IdentifierName("Simple")
        };

        /// <summary>
        /// Provides the explode parameter to use when serializing.
        /// </summary>
        public ExpressionSyntax Explode => LiteralExpression(Value.EndsWith("*")
            ? SyntaxKind.TrueLiteralExpression
            : SyntaxKind.FalseLiteralExpression);

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

        public InterpolatedStringContentSyntax ToInterpolatedStringContentSyntax(Func<PathSegment, ExpressionSyntax> parameterInterpreter) =>
            Type == PathSegmentType.Text
                ? (InterpolatedStringContentSyntax) InterpolatedStringText(
                    Token(TriviaList(), SyntaxKind.InterpolatedStringTextToken, Value, Value, TriviaList()))
                : Interpolation(parameterInterpreter.Invoke(this));
    }
}
