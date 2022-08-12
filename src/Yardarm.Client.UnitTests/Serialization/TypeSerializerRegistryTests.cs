using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using RootNamespace.Serialization;
using Xunit;

namespace Yardarm.Client.UnitTests.Serialization
{
    public class TypeSerializerRegistryTests
    {
        #region Add<T>

        [Fact]
        public void AddT_DefaultConstructor_Success()
        {
            // Arrange

            var registry = new TypeSerializerRegistry();

            // Act

            registry.Add<DefaultConstructorSerializer>(new[] {"text/plain"});

            // Assert

            registry.Get("text/plain").Should().BeOfType<DefaultConstructorSerializer>();
        }

        [Fact]
        public void AddT_RegistryConstructor_Success()
        {
            // Arrange

            var registry = new TypeSerializerRegistry();

            // Act

            registry.Add<RegistryConstructorSerializer>(new[] {"text/plain"});

            // Assert

            registry.Get("text/plain").Should().BeOfType<RegistryConstructorSerializer>()
                .Subject.Registry.Should().BeSameAs(registry);
        }

        #endregion

        #region Get By Schema Type

        [Fact]
        public void GetBySchemaType_ExactMatch_Success()
        {
            // Arrange

            var registry = new TypeSerializerRegistry();

            // Act

            registry.Add<RegistryConstructorSerializer>(null, new[] { typeof(Stream) });

            // Assert

            registry.Get(typeof(Stream)).Should().BeOfType<RegistryConstructorSerializer>()
                .Subject.Registry.Should().BeSameAs(registry);
        }

        [Fact]
        public void GetBySchemaType_InheritedClassMatch_Success()
        {
            // Arrange

            var registry = new TypeSerializerRegistry();

            // Act

            registry.Add<RegistryConstructorSerializer>(null, new[] { typeof(Stream) });

            // Assert

            registry.Get(typeof(MemoryStream)).Should().BeOfType<RegistryConstructorSerializer>()
                .Subject.Registry.Should().BeSameAs(registry);
        }

        [Fact]
        public void GetBySchemaType_NoMatch_KeyNotFound()
        {
            // Arrange

            var registry = new TypeSerializerRegistry();

            // Act

            registry.Add<RegistryConstructorSerializer>(null, new[] { typeof(Stream) });

            // Assert

            Action action = () => registry.Get(typeof(object));
            action.Should().Throw<KeyNotFoundException>();
        }

        #endregion

        #region Helpers

        public class DefaultConstructorSerializer : ITypeSerializer
        {
            public HttpContent Serialize<T>(T value, string mediaType, ISerializationData serializationData = null) =>
                throw new NotImplementedException();

            public ValueTask<T>
                DeserializeAsync<T>(HttpContent content, ISerializationData serializationData = null) =>
                throw new NotImplementedException();
        }

        public class RegistryConstructorSerializer : ITypeSerializer
        {
            public ITypeSerializerRegistry Registry { get; }

            public RegistryConstructorSerializer()
            {
                // Include a default constructor to ensure that the parameterized constructor is chosen
            }

            public RegistryConstructorSerializer(ITypeSerializerRegistry registry)
            {
                Registry = registry;
            }

            public HttpContent Serialize<T>(T value, string mediaType, ISerializationData serializationData = null) =>
                throw new NotImplementedException();

            public ValueTask<T>
                DeserializeAsync<T>(HttpContent content, ISerializationData serializationData = null) =>
                throw new NotImplementedException();
        }


        #endregion
    }
}
