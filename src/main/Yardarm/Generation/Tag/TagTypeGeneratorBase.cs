using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Operation;
using Yardarm.Helpers;
using Yardarm.Names;
using Yardarm.Spec;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Generation.Tag
{
    public abstract class TagTypeGeneratorBase : TypeGeneratorBase<OpenApiTag>
    {
        protected OpenApiTag Tag => Element.Element;

        protected TagTypeGeneratorBase(ILocatedOpenApiElement<OpenApiTag> tagElement, GenerationContext context)
            : base(tagElement, context, null)
        {
        }

        protected virtual IEnumerable<MethodDeclarationSyntax> GenerateOperationMethodHeader(
            ILocatedOpenApiElement<OpenApiOperation> operation)
        {
            TypeSyntax requestType = Context.TypeGeneratorRegistry.Get(operation).TypeInfo.Name;
            TypeSyntax responseType = WellKnownTypes.System.Threading.Tasks.TaskT.Name(
                Context.TypeGeneratorRegistry.Get(operation.GetResponseSet()).TypeInfo.Name);

            string methodName = Context.NameFormatterSelector.GetFormatter(NameKind.AsyncMethod)
                .Format(operation.Element.OperationId);

            var methodDeclaration = MethodDeclaration(responseType, methodName)
                .AddElementAnnotation(operation, Context.ElementRegistry)
                .AddParameterListParameters(
                    Parameter(Identifier(OperationMethodGenerator.RequestParameterName))
                        .WithType(requestType),
                    MethodHelpers.DefaultedCancellationTokenParameter());

            yield return methodDeclaration;
        }

        protected IEnumerable<ILocatedOpenApiElement<OpenApiOperation>> GetOperations() =>
            Context.Document.Paths.ToLocatedElements()
                .GetOperations()
                .Where(p => p.Element.Tags.Any(q => q.Name == Tag.Name));
    }
}
