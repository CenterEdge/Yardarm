using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RootNamespace.Authentication
{
    /// <summary>
    /// Combines multiple authenticators into a single authenticator. Authenticators are applied in order.
    /// </summary>
    public class MultiAuthenticator : IAuthenticator
    {
        private readonly IAuthenticator[] _authenticators;

        public MultiAuthenticator(params IAuthenticator[] authenticators)
            : this((IEnumerable<IAuthenticator>)authenticators)
        {
        }

        public MultiAuthenticator(IEnumerable<IAuthenticator> authenticators)
        {
            if (authenticators == null)
            {
                throw new ArgumentNullException(nameof(authenticators));
            }

            _authenticators = authenticators.ToArray();
        }

        /// <inheritdoc />
        public async ValueTask ApplyAsync(HttpRequestMessage message, CancellationToken cancellationToken = default)
        {
            foreach (var authenticator in _authenticators)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await authenticator.ApplyAsync(message, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async ValueTask ProcessResponseAsync(HttpResponseMessage message,
            CancellationToken cancellationToken = default)
        {
            foreach (var authenticator in _authenticators)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await authenticator.ProcessResponseAsync(message, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
