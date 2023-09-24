using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.Request;
using Yardarm.Generation.Response;
using Yardarm.Names;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.Generation.MediaType
{
    public class MediaTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiMediaType>
    {
        private readonly GenerationContext _context;
        private readonly IRequestsNamespace _requestsNamespace;
        private readonly ISerializerSelector _serializerSelector;
        private readonly List<IRequestMemberGenerator> _memberGenerators;

        public MediaTypeGeneratorFactory(GenerationContext context, IRequestsNamespace requestsNamespace,
            ISerializerSelector serializerSelector, IEnumerable<IRequestMemberGenerator> memberGenerators)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(requestsNamespace);
            ArgumentNullException.ThrowIfNull(serializerSelector);
            ArgumentNullException.ThrowIfNull(memberGenerators);

            _context = context;
            _requestsNamespace = requestsNamespace;
            _serializerSelector = serializerSelector;
            _memberGenerators = memberGenerators.ToList();
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiMediaType> element, ITypeGenerator parent)
        {
            ArgumentNullException.ThrowIfNull(parent);

            return parent switch
            {
                NoopTypeGenerator<OpenApiRequestBody> requestBodyParent => new RequestMediaTypeGenerator(element, _context,
                    requestBodyParent, _requestsNamespace, _serializerSelector, _memberGenerators),
                ResponseTypeGenerator noop => new NoopTypeGenerator<OpenApiMediaType>(noop),
                _ => throw new ArgumentException("Unknown parent type for OpenApiMediaType", nameof(parent))
            };
        }
    }
}
