using System.Collections.Generic;
using System.Diagnostics;
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
    public abstract class TagTypeGeneratorBase(
        ILocatedOpenApiElement<OpenApiTag> tagElement,
        GenerationContext context,
        IOperationNameProvider operationNameProvider)
        : TypeGeneratorBase<OpenApiTag>(tagElement, context, null)
    {
        protected OpenApiTag Tag => Element.Element;

        protected virtual IEnumerable<MethodDeclarationSyntax> GenerateOperationMethodHeader(
            ILocatedOpenApiElement<OpenApiOperation> operation)
        {
            TypeSyntax requestType = Context.TypeGeneratorRegistry.Get(operation).TypeInfo.Name;
            TypeSyntax responseType = WellKnownTypes.System.Threading.Tasks.TaskT.Name(
                Context.TypeGeneratorRegistry.Get(operation.GetResponseSet()).TypeInfo.Name);

            string? operationName = operationNameProvider.GetOperationName(operation);
            Debug.Assert(operationName is not null);

            string methodName = Context.NameFormatterSelector.GetFormatter(NameKind.AsyncMethod)
                .Format(operationName);

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
                .WhereOperationHasName(operationNameProvider)
                .Where(p => p.Element.Tags.Any(q => q.Name == Tag.Name));
    }
}
