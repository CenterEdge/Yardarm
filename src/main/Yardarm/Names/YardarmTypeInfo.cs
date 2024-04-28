using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names
{
    public sealed class YardarmTypeInfo
    {
        public TypeSyntax Name { get; }

        public NameKind Kind { get; }

        [Flags]
        private enum Flags : byte
        {
            None = 0,
            Generated = 1,
            RequiresDynamicSerialization = 2
        }

        private readonly Flags _flags;

        public bool IsGenerated => _flags.HasFlag(Flags.Generated);

        public bool RequiresDynamicSerialization => _flags.HasFlag(Flags.RequiresDynamicSerialization);

        public YardarmTypeInfo(TypeSyntax name, NameKind kind, bool isGenerated)
            : this(name, kind, isGenerated, false)
        {
        }

        public YardarmTypeInfo(TypeSyntax name, NameKind kind = NameKind.Class, bool isGenerated = true,
            bool requiresDynamicSerialization = false)
        {
            ArgumentNullException.ThrowIfNull(name);

            Name = name;
            Kind = kind;

            _flags = (isGenerated ? Flags.Generated : Flags.None) |
                     (requiresDynamicSerialization ? Flags.RequiresDynamicSerialization : Flags.None);
        }

        private bool Equals(YardarmTypeInfo other) => Name.Equals(other.Name) && Kind == other.Kind && _flags == other._flags;

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != typeof(YardarmTypeInfo))
            {
                return false;
            }

            return Equals((YardarmTypeInfo) obj);
        }

        public override int GetHashCode() => HashCode.Combine(Name, (int) Kind, _flags);
    }
}
