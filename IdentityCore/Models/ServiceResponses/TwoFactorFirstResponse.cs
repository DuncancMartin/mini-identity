using OpenIddict.Abstractions;

namespace IdentityCore.Models.ServiceResponses
{
    public class TwoFactorFirstResponse
    {
        //TODO remove the default and have a means of getting the 2fa type from the DB/factory when we have more than one.
        public TwoFactorFirstResponse(string passwordToken, string twoFAType = "Google Authenticator")
        {
            PasswordToken = passwordToken;
            TwoFAType = twoFAType;
        }

        public string PasswordToken { get; set; }
        public string TwoFAType { get; set; }
        public OpenIddictResponse ErrorResponse { get; set; }

        public bool IsSuccess => ErrorResponse == default(OpenIddictResponse);
    }
}
