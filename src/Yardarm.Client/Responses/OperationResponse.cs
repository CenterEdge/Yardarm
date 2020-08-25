using System;
using System.Net;
using System.Net.Http;
using RootNamespace.Serialization;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Responses
{
    public abstract class OperationResponse : IOperationResponse
    {
        public HttpResponseMessage Message { get; }

        public HttpStatusCode StatusCode => Message.StatusCode;

        public bool IsSuccessStatusCode => Message.IsSuccessStatusCode;

        protected ITypeSerializerRegistry TypeSerializerRegistry { get; }

        protected OperationResponse(HttpResponseMessage message, ITypeSerializerRegistry typeSerializerRegistry)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            TypeSerializerRegistry = typeSerializerRegistry ?? throw new ArgumentNullException(nameof(typeSerializerRegistry));

            // ReSharper disable once VirtualMemberCallInConstructor
            ParseHeaders();
        }

        /// <summary>
        /// Called during construction to parse headers from the message into properties.
        /// </summary>
        protected virtual void ParseHeaders()
        {
        }

        public virtual void Dispose() => Message.Dispose();
    }
}
