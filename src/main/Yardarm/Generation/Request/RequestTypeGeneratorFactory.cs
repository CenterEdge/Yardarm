using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.OpenApi.Models;
using Yardarm.Generation.MediaType;
using Yardarm.Names;
using Yardarm.Serialization;
using Yardarm.Spec;

namespace Yardarm.Generation.Request
{
    public class RequestTypeGeneratorFactory : ITypeGeneratorFactory<OpenApiOperation>
    {
        private readonly GenerationContext _context;
        private readonly IMediaTypeSelector _mediaTypeSelector;
        private readonly IList<IRequestMemberGenerator> _memberGenerators;
        private readonly IRequestsNamespace _requestsNamespace;
        private readonly ISerializerSelector _serializerSelector;

        public RequestTypeGeneratorFactory(GenerationContext context, IMediaTypeSelector mediaTypeSelector,
            IEnumerable<IRequestMemberGenerator> memberGenerators,
            IRequestsNamespace requestsNamespace, ISerializerSelector serializerSelector)
        {
            ArgumentNullException.ThrowIfNull(context);
            ArgumentNullException.ThrowIfNull(mediaTypeSelector);
            ArgumentNullException.ThrowIfNull(memberGenerators);
            ArgumentNullException.ThrowIfNull(requestsNamespace);
            ArgumentNullException.ThrowIfNull(serializerSelector);

            _context = context;
            _mediaTypeSelector = mediaTypeSelector;
            _memberGenerators = memberGenerators.ToArray();
            _requestsNamespace = requestsNamespace;
            _serializerSelector = serializerSelector;
        }

        public ITypeGenerator Create(ILocatedOpenApiElement<OpenApiOperation> element, ITypeGenerator? parent) =>
            new RequestTypeGenerator(element, _context, _mediaTypeSelector, _memberGenerators,
                _requestsNamespace, _serializerSelector);
    }
}
