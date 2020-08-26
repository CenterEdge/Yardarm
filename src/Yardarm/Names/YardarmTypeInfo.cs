using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Names
{
    public sealed class YardarmTypeInfo
    {
        public TypeSyntax Name { get; }

        public NameKind Kind { get; }

        public bool IsGenerated { get; }

        public YardarmTypeInfo(TypeSyntax name, NameKind kind = NameKind.Class, bool isGenerated = true)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Kind = kind;
            IsGenerated = isGenerated;
        }

        private bool Equals(YardarmTypeInfo other) => Name.Equals(other.Name) && Kind == other.Kind && IsGenerated == other.IsGenerated;

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

        public override int GetHashCode() => HashCode.Combine(Name, (int) Kind, IsGenerated);
    }
}
