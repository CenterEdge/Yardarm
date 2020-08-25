using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        public ReadOnlyCollection<IAuthenticator> Authenticators { get; }

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

            Authenticators = new ReadOnlyCollection<IAuthenticator>(authenticators.ToArray());
        }

        /// <inheritdoc />
        public async ValueTask ApplyAsync(HttpRequestMessage message, CancellationToken cancellationToken = default)
        {
            foreach (var authenticator in Authenticators)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await authenticator.ApplyAsync(message, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <inheritdoc />
        public async ValueTask ProcessResponseAsync(HttpResponseMessage message,
            CancellationToken cancellationToken = default)
        {
            foreach (var authenticator in Authenticators)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await authenticator.ProcessResponseAsync(message, cancellationToken).ConfigureAwait(false);
            }
        }
    }
}
