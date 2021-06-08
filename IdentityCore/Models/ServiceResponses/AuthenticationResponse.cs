using Microsoft.AspNetCore.Authentication;
using OpenIddict.Abstractions;

namespace IdentityCore.Models.ServiceResponses
{
    public class AuthenticationResponse
    {
        public AuthenticationResponse() { }

        public AuthenticationResponse(AuthenticationTicket ticket)
        {
            Ticket = ticket;
        }

        public AuthenticationResponse(string passwordToken, string twoFAType)
        {
            PasswordToken = passwordToken;
            TwoFAType = twoFAType;
        }

        public AuthenticationTicket Ticket { get; set; }
        public string PasswordToken { get; set; }
        public string TwoFAType { get; set; }
        public OpenIddictResponse ErrorResponse { get; set; }

        public bool IsSuccess => ErrorResponse == default(OpenIddictResponse);
    }
}
