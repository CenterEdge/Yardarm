using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Names;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class MediaTypeGenerator : TypeGeneratorBase<OpenApiMediaType>
    {
        public MediaTypeGenerator(ILocatedOpenApiElement<OpenApiMediaType> element, GenerationContext context)
            : base(element, context)
        {
        }

        protected override YardarmTypeInfo GetTypeInfo() =>
            Context.TypeGeneratorRegistry.Get(Element.Parent!).TypeInfo;

        public override IEnumerable<MemberDeclarationSyntax> Generate() => Enumerable.Empty<MemberDeclarationSyntax>();
    }
}
