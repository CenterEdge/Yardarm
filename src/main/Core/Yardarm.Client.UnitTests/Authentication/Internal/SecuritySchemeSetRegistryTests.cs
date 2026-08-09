using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using RootNamespace.Authentication;
using RootNamespace.Authentication.Internal;
using RootNamespace.Requests;
using Xunit;

namespace Yardarm.Client.UnitTests.Authentication.Internal
{
    public class SecuritySchemeSetRegistryTests
    {
        #region SelectAuthenticator

        [Fact]
        public void SelectAuthenticator_NoSchemes_ReturnsNull()
        {
            // Arrange

            var authenticators = new TestAuthenticators {Auth1 = new TestAuth1(), Auth2 = new TestAuth2()};

            var registry = new SecuritySchemeSetRegistry<TestAuthenticators>(authenticators);

            // Act

            var result = registry.SelectAuthenticator(typeof(NoSchemesRequest));

            // Assert

            result.Should().BeNull();
        }

        [Fact]
        public void SelectAuthenticator_SingleSchemeNotConfigured_ReturnsNull()
        {
            // Arrange

            var authenticators = new TestAuthenticators {Auth1 = null, Auth2 = new TestAuth2()};

            var registry = new SecuritySchemeSetRegistry<TestAuthenticators>(authenticators);

            // Act

            var result = registry.SelectAuthenticator(typeof(OneSchemeRequest));

            // Assert

            result.Should().BeNull();
        }

        [Fact]
        public void SelectAuthenticator_SingleSchemeConfigured_ReturnsAuthenticator()
        {
            // Arrange

            var authenticators = new TestAuthenticators {Auth1 = new TestAuth1(), Auth2 = new TestAuth2()};

            var registry = new SecuritySchemeSetRegistry<TestAuthenticators>(authenticators);

            // Act

            var result = registry.SelectAuthenticator(typeof(OneSchemeRequest));

            // Assert

            result.Should().BeSameAs(authenticators.Auth1);
        }

        [Fact]
        public void SelectAuthenticator_TwoSchemesFirstConfigured_ReturnsFirstAuthenticator()
        {
            // Arrange

            var authenticators = new TestAuthenticators {Auth1 = new TestAuth1(), Auth2 = null};

            var registry = new SecuritySchemeSetRegistry<TestAuthenticators>(authenticators);

            // Act

            var result = registry.SelectAuthenticator(typeof(TwoSchemesRequest));

            // Assert

            result.Should().BeSameAs(authenticators.Auth1);
        }

        [Fact]
        public void SelectAuthenticator_TwoSchemesSecondConfigured_ReturnsFirstAuthenticator()
        {
            // Arrange

            var authenticators = new TestAuthenticators {Auth1 = null, Auth2 = new TestAuth2()};

            var registry = new SecuritySchemeSetRegistry<TestAuthenticators>(authenticators);

            // Act

            var result = registry.SelectAuthenticator(typeof(TwoSchemesRequest));

            // Assert

            result.Should().BeSameAs(authenticators.Auth2);
        }

        [Fact]
        public void SelectAuthenticator_TwoSchemesBothConfigured_ReturnsFirstAuthenticator()
        {
            // Arrange

            var authenticators = new TestAuthenticators {Auth1 = new TestAuth1(), Auth2 = new TestAuth2()};

            var registry = new SecuritySchemeSetRegistry<TestAuthenticators>(authenticators);

            // Act

            var result = registry.SelectAuthenticator(typeof(TwoSchemesRequest));

            // Assert

            result.Should().BeSameAs(authenticators.Auth1);
        }

        [Fact]
        public void SelectAuthenticator_TwoSchemesNeitherConfigured_ReturnsNull()
        {
            // Arrange

            var authenticators = new TestAuthenticators {Auth1 = null, Auth2 = null};

            var registry = new SecuritySchemeSetRegistry<TestAuthenticators>(authenticators);

            // Act

            var result = registry.SelectAuthenticator(typeof(TwoSchemesRequest));

            // Assert

            result.Should().BeNull();
        }

        [Fact]
        public void SelectAuthenticator_JoinedSchemesNeitherConfigured_ReturnsNull()
        {
            // Arrange

            var authenticators = new TestAuthenticators {Auth1 = null, Auth2 = null};

            var registry = new SecuritySchemeSetRegistry<TestAuthenticators>(authenticators);

            // Act

            var result = registry.SelectAuthenticator(typeof(JoinedSchemesRequest));

            // Assert

            result.Should().BeNull();
        }

        [Fact]
        public void SelectAuthenticator_JoinedSchemesOneConfigured_ReturnsNull()
        {
            // Arrange

            var authenticators = new TestAuthenticators {Auth1 = null, Auth2 = new TestAuth2()};

            var registry = new SecuritySchemeSetRegistry<TestAuthenticators>(authenticators);

            // Act

            var result = registry.SelectAuthenticator(typeof(JoinedSchemesRequest));

            // Assert

            result.Should().BeNull();
        }

        [Fact]
        public void SelectAuthenticator_JoinedSchemesBothConfigured_ReturnsMultiAuthenticator()
        {
            // Arrange

            var authenticators = new TestAuthenticators {Auth1 = new TestAuth1(), Auth2 = new TestAuth2()};

            var registry = new SecuritySchemeSetRegistry<TestAuthenticators>(authenticators);

            // Act

            var result = registry.SelectAuthenticator(typeof(JoinedSchemesRequest));

            // Assert

            result.Should().BeOfType<MultiAuthenticator>().Which
                .Authenticators.Should().BeEquivalentTo(new IAuthenticator[] { authenticators.Auth1, authenticators.Auth2 });
        }

        #endregion

        #region Helpers

        public class TestAuthenticators
        {
            public TestAuth1 Auth1 { get; set; }

            public TestAuth2 Auth2 { get; set; }
        }

        public class TestAuth1 : IAuthenticator
        {
            public ValueTask ApplyAsync(HttpRequestMessage message, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public ValueTask ProcessResponseAsync(HttpResponseMessage message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }

        public class TestAuth2 : IAuthenticator
        {
            public ValueTask ApplyAsync(HttpRequestMessage message, CancellationToken cancellationToken = default) => throw new NotImplementedException();

            public ValueTask ProcessResponseAsync(HttpResponseMessage message, CancellationToken cancellationToken = default) => throw new NotImplementedException();
        }

        public class NoSchemesRequest : IOperationRequest
        {
            public IAuthenticator Authenticator { get; set; }
            public bool EnableResponseStreaming { get; set; }
        }

        [SecuritySchemeSet(typeof(TestAuth1))]
        public class OneSchemeRequest : IOperationRequest
        {
            public IAuthenticator Authenticator { get; set; }
            public bool EnableResponseStreaming { get; set; }
        }

        [SecuritySchemeSet(typeof(TestAuth1))]
        [SecuritySchemeSet(typeof(TestAuth2))]
        public class TwoSchemesRequest : IOperationRequest
        {
            public IAuthenticator Authenticator { get; set; }
            public bool EnableResponseStreaming { get; set; }
        }

        [SecuritySchemeSet(typeof(TestAuth1), typeof(TestAuth2))]
        public class JoinedSchemesRequest : IOperationRequest
        {
            public IAuthenticator Authenticator { get; set; }
            public bool EnableResponseStreaming { get; set; }
        }

        #endregion
    }
}
