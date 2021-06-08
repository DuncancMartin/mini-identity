using System.Threading.Tasks;
using IdentityCore.Models;
using IdentityCore.Models.ServiceResponses;
using Microsoft.AspNetCore.Identity;

namespace IdentityCore.Interfaces.Services
{
    public interface ITwoFactorService
    {
        Task<AuthenticatorKey> GenerateNewAuthenticatorKey(string username);

        Task<IdentityResult> EnableTwoFactor(string username, string twoFactorCode);
        Task<IdentityResult> DisableTwoFactor(string username);
    }
}
