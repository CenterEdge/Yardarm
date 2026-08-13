using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.OpenApi;
using Xunit;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Spec;

public class OpenApiReferenceHolderAccessorTests
{
    [Fact]
    public void GetReference_KnownProxy_ReturnsReference()
    {
        var document = new OpenApiDocument();
        var reference = new OpenApiSchemaReference("schema", document, null!);

        BaseOpenApiReference result = OpenApiReferenceHolderAccessor.GetReference(reference);

        result.Should().BeSameAs(reference.Reference);
    }

    [Fact]
    public void GetTarget_KnownProxy_ReturnsResolvedTarget()
    {
        var schema = new OpenApiSchema();
        var document = new OpenApiDocument
        {
            Workspace = new OpenApiWorkspace(),
            Components = new OpenApiComponents
            {
                Schemas = new Dictionary<string, IOpenApiSchema>
                {
                    ["schema"] = schema
                }
            }
        };
        document.RegisterComponents();
        var reference = new OpenApiSchemaReference("schema", document, null!);

        IOpenApiElement result = OpenApiReferenceHolderAccessor.GetTarget(reference);

        result.Should().BeSameAs(schema);
    }

    [Fact]
    public void GetReference_UnknownHolder_ReturnsReference()
    {
        var holder = new CustomReferenceHolder();

        BaseOpenApiReference result = OpenApiReferenceHolderAccessor.GetReference(holder);

        result.Should().BeSameAs(holder.Reference);
    }

    [Fact]
    public void GetTarget_UnknownHolder_ReturnsTarget()
    {
        var holder = new CustomReferenceHolder();

        IOpenApiElement result = OpenApiReferenceHolderAccessor.GetTarget(holder);

        result.Should().BeSameAs(holder.Target);
    }

    [Fact]
    public void GetReference_InvalidUnknownHolder_ThrowsDescriptiveException()
    {
        var holder = new InvalidReferenceHolder();

        Action action = () => OpenApiReferenceHolderAccessor.GetReference(holder);

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*InvalidReferenceHolder*Reference*BaseOpenApiReference*");
    }

    private sealed class CustomReferenceHolder : IOpenApiReferenceHolder
    {
        public JsonSchemaReference Reference { get; } = new();

        public IOpenApiSchema Target { get; } = new OpenApiSchema();

        public bool UnresolvedReference => false;

        public void SerializeAsV2(IOpenApiWriter writer)
        {
        }

        public void SerializeAsV3(IOpenApiWriter writer)
        {
        }

        public void SerializeAsV31(IOpenApiWriter writer)
        {
        }

        public void SerializeAsV32(IOpenApiWriter writer)
        {
        }
    }

    private sealed class InvalidReferenceHolder : IOpenApiReferenceHolder
    {
        public string Reference => string.Empty;

        public string Target => string.Empty;

        public bool UnresolvedReference => false;

        public void SerializeAsV2(IOpenApiWriter writer)
        {
        }

        public void SerializeAsV3(IOpenApiWriter writer)
        {
        }

        public void SerializeAsV31(IOpenApiWriter writer)
        {
        }

        public void SerializeAsV32(IOpenApiWriter writer)
        {
        }
    }
}
