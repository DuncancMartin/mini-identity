using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityCore.Models;
using IdentityCore.Models.ServiceResponses;
using Microsoft.AspNetCore.Authentication;

namespace IdentityCore.Interfaces.Services
{
    public interface IAuthenticateService
    {
        Task<AuthenticationResponse> PasswordFlow(string username, string password, bool extendDuration = false);
        Task<AuthenticationResponse> RefreshTokenFlow(AuthenticateResult info);
        Task<AuthenticationResponse> TwoFactorFlow(string username, string passwordToken, string token);
        Task<AuthenticationResponse> PersonalAccessTokenFlow(string username, string password, PersonalAccessTokenMinimum patData);
    }
}
