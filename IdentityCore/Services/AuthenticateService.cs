using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityCore.Constants;
using IdentityCore.Interfaces.Repositories;
using IdentityCore.Interfaces.Services;
using IdentityCore.Interfaces.UnitOfWork;
using IdentityCore.Models;
using IdentityCore.Models.ServiceResponses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace IdentityCore.Services
{
    public class AuthenticateService : IAuthenticateService
    {
        private readonly IOptions<IdentityOptions> _identityOptions;
        private readonly IdentityMSUserManager _userManager;
        private readonly SignInManager<IdentityMSUser> _signInManager;
        private readonly TokenConfig _tokenConfig;
        private readonly IUnitOfWork _unitOfWork;

        public AuthenticateService(IOptions<IdentityOptions> identityOptions,
                                   IdentityMSUserManager userManager,
                                   SignInManager<IdentityMSUser> signInManager,
                                   TokenConfig tokenConfig,
                                   IUnitOfWork unitOfWork)
        {
            _identityOptions = identityOptions;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenConfig = tokenConfig;
            _unitOfWork = unitOfWork;
        }

      
        private static bool HasRoleScope(IEnumerable<string> scopes)
        {
            return scopes.Any(s => s == OpenIddictConstants.Scopes.Roles);
        }

        private async Task<IEnumerable<Claim>> GetClaims(IdentityMSUser user, IEnumerable<string> scopes)
        {
            var claims = new List<Claim>();

            // 4. User information
            claims.Add(new Claim(ClaimType.UserId, user.Id.ToString()));
            claims.Add(new Claim(ClaimType.Name, user.UserName));
            claims.Add(new Claim(ClaimType.TenantId, user.TenantId.ToString()));

            if (user.IsAdministrator) claims.Add(new Claim(ClaimType.IsAdministrator, "true"));
            if (user.TenantAdministrator == true) claims.Add(new Claim(ClaimType.IsTenantAdministrator, "true"));

            return claims;
        }

        private AuthenticationProperties BuildProperties(IdentityMSUser user, IEnumerable<Claim> claims)
        {
            var properties = new Dictionary<string, string>
            {
                [$"{TokenProperties.RefreshTokenExpiresIn}{PublicPropertyTypes.Integer}"] =
                    _tokenConfig.RefreshTokenLifetimeInSeconds.ToString(),

                [$"{ClaimType.UserId}{PublicPropertyTypes.String}"] = user.Id.ToString(),
                [$"{ClaimType.TenantId}{PublicPropertyTypes.Integer}"] = user.TenantId.ToString(),

                [$"{TokenProperties.IsSupportAdmin}{PublicPropertyTypes.Boolean}"] = user.IsAdministrator.ToString(),
                [$"{TokenProperties.IsTenantAdmin}{PublicPropertyTypes.Boolean}"] = user.TenantAdministrator.ToString()
            };

            if (user.LocalizationId != 0)
            {
                var languageClaim = claims.FirstOrDefault(i => i.Type == ClaimType.LanguageCode);
                if (languageClaim != default(Claim))
                    properties[$"{ClaimType.LanguageCode}{PublicPropertyTypes.String}"] = languageClaim.Value;
            }

            return new AuthenticationProperties(properties);
        }

        private async Task<AuthenticationTicket> CreateTicketAsync(IdentityMSUser user, IEnumerable<string> scopes, bool extendDuration = false, bool isPersonalAccessToken = false, string patData = "")
        {
            var principal = await _signInManager.CreateUserPrincipalAsync(user);
            principal.SetScopes(scopes);
            if (extendDuration)
                principal.SetAccessTokenLifetime(_tokenConfig.ExtendedAuthTokenLifeTime);
            if (isPersonalAccessToken)
                // Personal access tokens don't expire
                principal.SetAccessTokenLifetime(TimeSpan.FromDays(365 * 100));

            // Build data embedded in token
            var identity = (ClaimsIdentity)principal.Identity;
            var claims = await GetClaims(user, scopes);
            if (isPersonalAccessToken) claims.ToList().Add(new Claim(ClaimType.PAT, patData));

            identity.AddClaims(claims);
            var properties = BuildProperties(user, claims);

            var ticket = new AuthenticationTicket(principal, properties, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
            
            foreach (var claim in ticket.Principal.Claims)
            {
                // Never include the security stamp in the tokens, it's a secret value.
                if (claim.Type == _identityOptions.Value.ClaimsIdentity.SecurityStampClaimType)
                    continue;

                claim.SetDestinations(OpenIddictConstants.Destinations.AccessToken);
            }

            return ticket;
        }

        private async Task<TwoFactorFirstResponse> CreatePasswordTokenAsync(IdentityMSUser user)
        {
            var token = await _userManager.GeneratePasswordToken(user);
            await _unitOfWork.SaveChangesAsync();
            return new TwoFactorFirstResponse(token);
        }

        public async Task<AuthenticationResponse> PasswordFlow(string username, string password, bool extendDuration = false)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return AuthenticationErrorResponses.InvalidUserPass;

            if (user.Active != true)
                return AuthenticationErrorResponses.UserNotActive;

            //var tenantActive = await _userManager.TenantIsActiveAsync(user);
            //if (!tenantActive)
            //    return AuthenticationErrorResponses.InActiveTenant;

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            if (!result.Succeeded)
                return AuthenticationErrorResponses.InvalidUserPass;

            var passwordConfirmed = await _userManager.GetPasswordConfirmedAsync(user);
            if (!passwordConfirmed)
                return AuthenticationErrorResponses.MustResetPass;

            var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (twoFactorEnabled)
            {
                var twoFactorResponse = await CreatePasswordTokenAsync(user);
                return new AuthenticationResponse(twoFactorResponse.PasswordToken, twoFactorResponse.TwoFAType);
            }

            var requestedScopes = new [] { OpenIddictConstants.Scopes.OfflineAccess, OpenIddictConstants.Scopes.Roles };
            var ticket = await CreateTicketAsync(user, requestedScopes, extendDuration);

            user.LastAccess = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new AuthenticationResponse(ticket);
        }

        public async Task<AuthenticationResponse> RefreshTokenFlow(AuthenticateResult info)
        {
            var user = await _signInManager.ValidateSecurityStampAsync(info.Principal);
            if (user == null)
                return AuthenticationErrorResponses.InvalidRefreshToken;

            if (user.Active != true)
                return AuthenticationErrorResponses.UserNotActive;

            //var tenantActive = await _userManager.TenantIsActiveAsync(user);
            //if (!tenantActive)
            //    return AuthenticationErrorResponses.InActiveTenant;

            var result = await _signInManager.CanSignInAsync(user);
            if (!result)
                return AuthenticationErrorResponses.UserSignInRevoked;

            var requestedScopes = new[] { OpenIddictConstants.Scopes.Roles };
            var ticket = await CreateTicketAsync(user, requestedScopes);

            return new AuthenticationResponse(ticket);
        }

        public async Task<AuthenticationResponse> TwoFactorFlow(string username, string passwordToken, string token)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return AuthenticationErrorResponses.InvalidUserPass;

            var twoFactorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (!twoFactorEnabled)
                return AuthenticationErrorResponses.UserSignInRevoked;

            var passwordTokenValid = await _userManager.VerifyPasswordTokenAsync(user, passwordToken);
            if (!passwordTokenValid)
                return AuthenticationErrorResponses.InvalidUserPass;

            var tokenValid = await _userManager.VerifyTwoFactorTokenAsync(user,
                _userManager.Options.Tokens.AuthenticatorTokenProvider, token);
            if (!tokenValid)
                return AuthenticationErrorResponses.InvalidUserPass;

            var requestedScopes = new[] { OpenIddictConstants.Scopes.OfflineAccess, OpenIddictConstants.Scopes.Roles };
            var ticket = await CreateTicketAsync(user, requestedScopes);

            user.LastAccess = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new AuthenticationResponse(ticket);
        }

        //NB - this is async different flow to the 2fa one, so even if 2fa is setup, we are still only checking the username/password.
        public async Task<AuthenticationResponse> PersonalAccessTokenFlow(string username, string password, PersonalAccessTokenMinimum patData)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return AuthenticationErrorResponses.InvalidUserPass;

            if (user.Active != true)
                return AuthenticationErrorResponses.UserNotActive;

            //var tenantActive = await _userManager.TenantIsActiveAsync(user);
            //if (!tenantActive)
            //    return AuthenticationErrorResponses.InActiveTenant;

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);
            if (!result.Succeeded)
                return AuthenticationErrorResponses.InvalidUserPass;

            var passwordConfirmed = await _userManager.GetPasswordConfirmedAsync(user);
            if (!passwordConfirmed)
                return AuthenticationErrorResponses.MustResetPass;

            var requestedScopes = new [] { OpenIddictConstants.Scopes.OfflineAccess, OpenIddictConstants.Scopes.Roles };
            var ticket = await CreateTicketAsync(user, requestedScopes, extendDuration: false, isPersonalAccessToken: true, JsonConvert.SerializeObject(patData));
            
            //This is the only application that should be allowed access through PATs
            ticket.Principal.SetResources("publicApi");

            user.LastAccess = DateTimeOffset.UtcNow;
            await _userManager.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return new AuthenticationResponse(ticket);
        }
    }
}
