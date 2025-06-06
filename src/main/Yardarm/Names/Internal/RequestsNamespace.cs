﻿using System;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Yardarm.Names.Internal;

// ReSharper disable InconsistentNaming
internal class RequestsNamespace : IRequestsNamespace
{
    public NameSyntax Name { get; }
    public NameSyntax IOperationRequest { get; }
    public NameSyntax OperationRequest { get; }
    public NameSyntax BuildRequestContext { get; }

    public RequestsNamespace(IRootNamespace rootNamespace)
    {
        ArgumentNullException.ThrowIfNull(rootNamespace);

        Name = QualifiedName(rootNamespace.Name, IdentifierName("Requests"));

        IOperationRequest = QualifiedName(
            Name,
            IdentifierName("IOperationRequest"));

        OperationRequest = QualifiedName(
            Name,
            IdentifierName("OperationRequest"));

        BuildRequestContext = QualifiedName(
            Name,
            IdentifierName("BuildRequestContext"));
    }
}
