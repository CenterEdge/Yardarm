using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using Microsoft.OpenApi.Models;
using Yardarm.Enrichment;
using Yardarm.Names;

namespace Yardarm.Generation.Schema
{
    /// <summary>
    /// Generates the generic OneOf&gt;...&lt; types for various discriminated unions.
    /// </summary>
    internal class OneOfSchemaGenerator : SchemaGeneratorBase
    {
        protected override NameKind NameKind => NameKind.Class;

        public OneOfSchemaGenerator(LocatedOpenApiElement<OpenApiSchema> schemaElement, GenerationContext context)
            : base(schemaElement, context)
        {
        }

        public override IEnumerable<MemberDeclarationSyntax> Generate()
        {
            var classNameAndNamespace = (QualifiedNameSyntax)GetTypeName();

            NameSyntax ns = classNameAndNamespace.Left;
            SimpleNameSyntax className = classNameAndNamespace.Right;

            var syntaxTree = Generate(ns, className, Schema.OneOf.Select(p => SchemaElement.CreateChild(p, "")));

            var classDeclaration = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>().First();

            yield return classDeclaration.Enrich(Context.Enrichers.ClassEnrichers, SchemaElement);
        }

        public SyntaxTree Generate(NameSyntax ns, SimpleNameSyntax identifier, IEnumerable<LocatedOpenApiElement<OpenApiSchema>> values)
        {
            var builder = new StringBuilder(1024 * 10);

            TypeSyntax[] typeNames = values.Select(p => Context.TypeNameGenerator.GetName(p))
                .ToArray();

            builder.AppendLine($@"namespace {ns}
{{
    public abstract class {identifier} : System.IEquatable<{identifier}>
    {{
        private {identifier}() {{}}

        public abstract bool Equals({identifier} other);");

            AddImplicitOperations(builder, identifier.ToString(), typeNames);
            AddSubTypes(builder, identifier.ToString(), typeNames);

            builder.AppendLine(@"    }
}");

            return CSharpSyntaxTree.ParseText(SourceText.From(builder.ToString()),
                CSharpParseOptions.Default.WithLanguageVersion(LanguageVersion.CSharp8));
        }

        private void AddImplicitOperations(StringBuilder builder, string identifier, IEnumerable<TypeSyntax> typeNames)
        {
            foreach (var typeName in typeNames.OfType<QualifiedNameSyntax>())
            {
                builder.AppendLine(
                    @$"        public static implicit operator {identifier}({typeName} value) =>
            new {identifier}.{typeName.Right}(value);");
            }
        }

        private void AddSubTypes(StringBuilder builder, string identifier, IEnumerable<TypeSyntax> typeNames)
        {
            foreach (var typeName in typeNames.OfType<QualifiedNameSyntax>())
            {
                string subClassName = typeName.Right.ToString();

                builder.AppendLine($@"        public sealed class {subClassName} : {identifier}
        {{
            public {typeName} Value {{ get; }}

            public {subClassName}({typeName} value)
            {{
                Value = value;
            }}

            public static implicit operator {typeName}({subClassName} subClass) => subClass.Value;

            public override bool Equals(object? obj)
            {{
                if (!(obj is {subClassName} subClass)) return false;
                return Equals(subClass);
            }}

            public override bool Equals({identifier} other)
            {{
                if (!(other is {subClassName} subClass)) return false;
                return System.Collections.Generic.EqualityComparer<{typeName}>.Default.Equals(Value, subClass.Value);
            }}

            public bool Equals({subClassName} other)
            {{
                if (other == null) return false;
                return System.Collections.Generic.EqualityComparer<{typeName}>.Default.Equals(Value, other.Value);
            }}

            public overrides int GetHashCode() => Value?.GetHashCode() ?? 0;
        }}");
            }
        }
    }
}
