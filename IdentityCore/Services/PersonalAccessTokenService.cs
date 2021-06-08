using IdentityCore.Interfaces;
using IdentityCore.Interfaces.Services;
using IdentityCore.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityCore.Services
{
    public class PersonalAccessTokenService : IPersonalAccessTokenService
    {
        private readonly IdentityMSUserManager _userManager;
        private readonly IExtendedOpenIddictTokenManager _tokenManager;
        private readonly ILogger<PersonalAccessTokenService> _logger;

        public PersonalAccessTokenService(IdentityMSUserManager userManager,
                                            IExtendedOpenIddictTokenManager tokenManager,
                                            ILogger<PersonalAccessTokenService> logger)
        {
            _userManager = userManager;
            _tokenManager = tokenManager;
            _logger = logger;
        }

        public async Task<List<PersonalAccessToken>> GetTokens(int tenantId)
        {
            var users = await _userManager.FindByTenantIdAsync(tenantId);
            var personalAccessTokens = new List<PersonalAccessToken>();

            foreach (var user in users)
            {
                personalAccessTokens.AddRange(await GetUsersTokens(tenantId, user));
            }

            return personalAccessTokens;
        }

        public async Task<List<PersonalAccessToken>> GetTokens(int tenantId, int userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            
            return await GetUsersTokens(tenantId, user);
        }

        private async Task<List<PersonalAccessToken>> GetUsersTokens(int tenantId,  IdentityMSUser user)
        {
            List<PersonalAccessToken> personalAccessTokens = new List<PersonalAccessToken>();

            await foreach (OpenIddictEntityFrameworkCoreToken token in _tokenManager.FindBySubjectAsync(user.Id.ToString()).ConfigureAwait(false))
            {
                if (null != token.Properties && token.Type == "access_token" && token.Status == "valid")
                {
                    try
                    {
                        var patDict = JsonConvert.DeserializeObject<Dictionary<string, PersonalAccessToken>>(token.Properties);
                        var pat = patDict["PAT"];
                        pat.TenantId = tenantId;
                        pat.UserId = user.Id;
                        pat.TokenId = token.Id;
                        pat.TokenExpiry = token.ExpirationDate.GetValueOrDefault();
                        personalAccessTokens.Add(pat);
                    }
                    catch (JsonSerializationException ex)
                    {
                        _logger.LogError(ex, $"Error parsing PAT data for Token Id {token.Id}, tenantId {tenantId}, user {user.Id} - check Properties field for non-PAT data.");
                    }
                }
            }

            return personalAccessTokens;
        }

        public async Task<bool> RevokeToken(string tokenId)
        {
            var token = await _tokenManager.FindByIdAsync(tokenId);
            return await _tokenManager.TryRevokeAsync(token);
        }
    }
}
