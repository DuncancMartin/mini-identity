using System.Threading.Tasks;
using IdentityCore.Constants;
using Microsoft.AspNetCore.Authentication;
using OpenIddict.Server;

namespace IdentityApi.Middleware
{
    public static class TokenResponseHandler
    {
        /// <summary>
        /// Parses AuthenticationProperties of the current context and adds those of a predefined format to the response payload of the token endpoint. The parsed types are defined in <see cref="PublicPropertyTypes"/>.
        /// </summary>
        public static ValueTask PublicPropertyHandler(OpenIddictServerEvents.ApplyTokenResponseContext context)
        {
            var properties = context.Transaction.GetProperty<AuthenticationProperties>(typeof(AuthenticationProperties).FullName!);
            if (properties is null)
                return default;

            var response = context.Response;
            foreach (var (key, value) in properties.Items)
            {
                var keySplit = key.Split('#', 2);
                if (keySplit.Length != 2)
                    continue;

                var keyName = keySplit[0];
                // # is added back for pattern matching
                var keyType = $"#{keySplit[1]}";
                response[keyName] = keyType switch
                {
                    PublicPropertyTypes.Boolean => bool.Parse(value),
                    PublicPropertyTypes.Integer => int.Parse(value),
                    PublicPropertyTypes.String => value,
                    _ => response[keyName]
                };
            }

            return default;
        }
    }
}
