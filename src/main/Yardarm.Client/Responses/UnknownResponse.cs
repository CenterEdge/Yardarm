using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using RootNamespace.Serialization;

// ReSharper disable once CheckNamespace
namespace RootNamespace.Responses
{
    public abstract class UnknownResponse : OperationResponse
    {
        protected UnknownResponse(HttpResponseMessage message, ITypeSerializerRegistry typeSerializerRegistry)
            : base(message, typeSerializerRegistry)
        {
        }

        [return: MaybeNull]
        public ValueTask<T> GetBodyAsync<T>(CancellationToken cancellationToken = default) =>
            Message.Content != null
                ? TypeSerializerRegistry.DeserializeAsync<T>(Message.Content, cancellationToken: cancellationToken)
                : new ValueTask<T>(default(T)!);
    }
}
