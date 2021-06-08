using IdentityCore.Models.ServiceResponses;
using OpenIddict.Abstractions;

namespace IdentityCore.Constants
{
    public static class AuthenticationErrorResponses
    {
        public static AuthenticationResponse InvalidUserPass => new AuthenticationResponse
        {
            ErrorResponse = new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "The username/password couple is invalid."
            }
        };

        public static AuthenticationResponse InvalidRefreshToken => new AuthenticationResponse
        {
            ErrorResponse = new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "The refresh token is no longer valid."
            }
        };

        public static AuthenticationResponse UserSignInRevoked => new AuthenticationResponse
        {
            ErrorResponse = new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.InvalidGrant,
                ErrorDescription = "The user is no longer allowed to sign in."
            }
        };

        public static AuthenticationResponse UserNotActive => new AuthenticationResponse
        {
            ErrorResponse = new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.AccessDenied,
                ErrorDescription = "The user account has been disabled."
            }
        };

        public static AuthenticationResponse InActiveTenant => new AuthenticationResponse
        {
            ErrorResponse = new OpenIddictResponse
            {
                Error = OpenIddictConstants.Errors.AccessDenied,
                ErrorDescription = "The tenant is inactive."
            }
        };

        public static AuthenticationResponse MustResetPass => new AuthenticationResponse
        {
            ErrorResponse = new OpenIddictResponse
            {
                Error = "password_expired",
                ErrorDescription = "Password must be reset."
            }
        };
    }
}
