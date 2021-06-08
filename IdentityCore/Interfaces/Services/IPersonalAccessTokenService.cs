using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityCore.Models;

namespace IdentityCore.Interfaces.Services
{
    public interface IPersonalAccessTokenService
    {
        Task<List<PersonalAccessToken>> GetTokens(int tenantId);
        Task<List<PersonalAccessToken>> GetTokens(int tenantId, int userId);
        Task<bool> RevokeToken(string tokenId);
    }
}
