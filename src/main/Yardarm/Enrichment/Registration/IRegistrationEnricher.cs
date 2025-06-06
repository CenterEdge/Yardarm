﻿using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Yardarm.Enrichment.Registration;

/// <summary>
/// An expression enricher used by extensions to register themselves in the client.
/// </summary>
public interface IRegistrationEnricher : IEnricher<BlockSyntax>
{
}
