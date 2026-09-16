#nullable enable

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Xunit;
using Yardarm.Generation;
using Yardarm.Generation.Schema;
using Yardarm.Spec;

namespace Yardarm.UnitTests.Generation.Schema;

public class SchemaGeneratorFactoryPriorityTests
{
    [Fact]
    public void Get_LowerPriorityFactory_TakesPrecedence()
    {
        // Arrange

        var services = new ServiceCollection();
        services.AddYardarm(new YardarmGenerationSettings(), new OpenApiDocument());
        services.AddTypeGeneratorFactory<IOpenApiSchema, DefaultPrioritySchemaGeneratorFactory>();
        services.AddTypeGeneratorFactory<IOpenApiSchema, LowerPrioritySchemaGeneratorFactory>();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<ITypeGeneratorRegistry<IOpenApiSchema>>();

        // Act

        ITypeGenerator generator = registry.Get(new OpenApiSchema { Type = JsonSchemaType.String }.CreateRoot("String"));

        // Assert

        generator.Should().BeOfType<BooleanSchemaGenerator>();
    }

    [Fact]
    public void Get_EqualPriorityFactories_UsesRegistrationOrder()
    {
        // Arrange

        var services = new ServiceCollection();
        services.AddYardarm(new YardarmGenerationSettings(), new OpenApiDocument());
        services.AddTypeGeneratorFactory<IOpenApiSchema, FirstDefaultPrioritySchemaGeneratorFactory>();
        services.AddTypeGeneratorFactory<IOpenApiSchema, SecondDefaultPrioritySchemaGeneratorFactory>();
        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        var registry = serviceProvider.GetRequiredService<ITypeGeneratorRegistry<IOpenApiSchema>>();

        // Act

        ITypeGenerator generator = registry.Get(new OpenApiSchema { Type = JsonSchemaType.String }.CreateRoot("String"));

        // Assert

        generator.Should().BeOfType<BooleanSchemaGenerator>();
    }

    public sealed class DefaultPrioritySchemaGeneratorFactory(GenerationContext context) : ITypeGeneratorFactory<IOpenApiSchema>
    {
        public ITypeGenerator Create(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
            new StringSchemaGenerator(element, context, parent);
    }

    public sealed class LowerPrioritySchemaGeneratorFactory : ITypeGeneratorFactory<IOpenApiSchema>
    {
        public int Priority => -1;

        public ITypeGenerator Create(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
            BooleanSchemaGenerator.Instance;
    }

    public sealed class FirstDefaultPrioritySchemaGeneratorFactory : ITypeGeneratorFactory<IOpenApiSchema>
    {
        public ITypeGenerator Create(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
            BooleanSchemaGenerator.Instance;
    }

    public sealed class SecondDefaultPrioritySchemaGeneratorFactory(GenerationContext context) : ITypeGeneratorFactory<IOpenApiSchema>
    {
        public ITypeGenerator Create(ILocatedOpenApiElement<IOpenApiSchema> element, ITypeGenerator? parent) =>
            new StringSchemaGenerator(element, context, parent);
    }
}
