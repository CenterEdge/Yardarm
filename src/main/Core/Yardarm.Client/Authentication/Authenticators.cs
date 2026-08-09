using System;
using RootNamespace.Authentication.Internal;
using RootNamespace.Requests;

namespace RootNamespace.Authentication;

/// <summary>
/// A set of configured authenticators. Operations may choose from these when they require authentication.
/// </summary>
public sealed class Authenticators
{
    private readonly SecuritySchemeSetRegistry<Authenticators> _securitySchemeSetRegistry;

    public Authenticators()
    {
        _securitySchemeSetRegistry = new SecuritySchemeSetRegistry<Authenticators>(this);
    }

    public IAuthenticator? SelectAuthenticator(IOperationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        return request.Authenticator ?? _securitySchemeSetRegistry.SelectAuthenticator(request.GetType());
    }
}
