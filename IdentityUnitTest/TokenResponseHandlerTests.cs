using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityApi.Middleware;
using IdentityCore.Constants;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using Xunit;

namespace IdentityUnitTest
{
    public class TokenResponseHandlerTests
    {
        private static OpenIddictServerEvents.ApplyTokenResponseContext CreateContextWithAuthProperties(IDictionary<string, string> authPropertyDictionary)
        {
            var authenticationProperties = new AuthenticationProperties(authPropertyDictionary);
            var transaction = new OpenIddictServerTransaction();
            transaction.SetProperty(typeof(AuthenticationProperties).FullName!, authenticationProperties);
            return new OpenIddictServerEvents.ApplyTokenResponseContext(transaction)
            {
                Response = new OpenIddictResponse()
            };
        }

        [Fact]
        public async Task PublicPropertyHandler_TokenResponseContainsValue_IfBooleanIsInPublicProperties()
        {
            // Arrange
            const string boolPropertyName = "boolProperty";
            const bool intPropertyValue = true;
            var authProperties = new Dictionary<string, string>
            {
                {$"{boolPropertyName}{PublicPropertyTypes.Boolean}", intPropertyValue.ToString()}
            };

            var context = CreateContextWithAuthProperties(authProperties);

            // Act
            await TokenResponseHandler.PublicPropertyHandler(context);

            // Assert
            Assert.Equal(context.Response[boolPropertyName], intPropertyValue);
        }

        [Fact]
        public async Task PublicPropertyHandler_TokenResponseContainsValue_IfIntegerIsInPublicProperties()
        {
            // Arrange
            const string intPropertyName = "intProperty";
            const int intPropertyValue = 123;
            var authProperties = new Dictionary<string, string>
            {
                {$"{intPropertyName}{PublicPropertyTypes.Integer}", intPropertyValue.ToString()}
            };

            var context = CreateContextWithAuthProperties(authProperties);

            // Act
            await TokenResponseHandler.PublicPropertyHandler(context);

            // Assert
            Assert.Equal(context.Response[intPropertyName], intPropertyValue);
        }

        [Fact]
        public async Task PublicPropertyHandler_TokenResponseContainsValue_IfStringIsInPublicProperties()
        {
            // Arrange
            const string stringPropertyName = "stringProperty";
            const string stringPropertyValue = "asdf";
            var authProperties = new Dictionary<string, string>
            {
                {$"{stringPropertyName}{PublicPropertyTypes.String}", stringPropertyValue}
            };

            var context = CreateContextWithAuthProperties(authProperties);

            // Act
            await TokenResponseHandler.PublicPropertyHandler(context);

            // Assert
            Assert.Equal(context.Response[stringPropertyName], stringPropertyValue);
        }

        [Fact]
        public async Task PublicPropertyHandler_TokenResponseDoesNotContainValue_IfUnknownTypeIsInPublicProperties()
        {
            // Arrange
            const string stringPropertyName = "stringProperty";
            const string stringPropertyValue = "asdf";
            var authProperties = new Dictionary<string, string>
            {
                {$"{stringPropertyName}#public_unknown", stringPropertyValue}
            };

            var context = CreateContextWithAuthProperties(authProperties);

            // Act
            await TokenResponseHandler.PublicPropertyHandler(context);

            // Assert
            Assert.Null(context.Response[stringPropertyName]);
        }

        [Fact]
        public async Task PublicPropertyHandler_TokenResponseIsNotOverwritten_IfUnknownTypeIsInPublicProperties()
        {
            // Arrange
            const string stringPropertyName = "stringProperty";
            const string stringPropertyValue = "asdf";
            const string responseValue = "qwert";
            var authProperties = new Dictionary<string, string>
            {
                {$"{stringPropertyName}#public_unknown", stringPropertyValue}
            };

            var context = CreateContextWithAuthProperties(authProperties);
            context.Response[stringPropertyName] = responseValue;

            // Act
            await TokenResponseHandler.PublicPropertyHandler(context);

            // Assert
            Assert.Equal(context.Response[stringPropertyName], responseValue);
        }
    }
}
