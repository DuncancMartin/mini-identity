using Microsoft.AspNetCore.Identity;

namespace IdentityCore.Models
{
    public class AuthenticatorKey
    {
        public AuthenticatorKey(IdentityError errorResponse)
        {
            ErrorResponse = errorResponse;
        }
        public AuthenticatorKey(string authenticatorKey, string twoFAType)
        {
            AuthenticatorKeyValue = authenticatorKey;
            TwoFAType = twoFAType;
        }
        public string AuthenticatorKeyValue { get; set; }
        public string TwoFAType { get; set; }
        public IdentityError ErrorResponse { get; set; }
        public bool IsSuccess => ErrorResponse == default(IdentityError);
    }
}
