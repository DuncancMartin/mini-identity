using System.Threading.Tasks;
using IdentityCore.Interfaces.Services;
using IdentityCore.Interfaces.UnitOfWork;
using IdentityCore.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityCore.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly IdentityMSUserManager _userManager;
        private readonly IdentityMSErrorDescriber _errorDescriber;
        private readonly IUnitOfWork _unitOfWork;
        private const string twoFAType = "Google Authenticator";

        public TwoFactorService(IdentityMSUserManager userManager,
                                IdentityErrorDescriber errorDescriber,
                                IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _errorDescriber = errorDescriber as IdentityMSErrorDescriber;
            _unitOfWork = unitOfWork;
        }

        public async Task<AuthenticatorKey> GenerateNewAuthenticatorKey(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return new AuthenticatorKey(_errorDescriber.UnknownUserName(username));

            var authenticatorInUse = await _userManager.GetTwoFactorEnabledAsync(user);
            if (authenticatorInUse)
                return new AuthenticatorKey(_errorDescriber.TwoFactorAlreadyEnabled());


            var resetResult = await _userManager.ResetAuthenticatorKeyAsync(user);
            if (!resetResult.Succeeded)
                return new AuthenticatorKey(_errorDescriber.ResetAuthenticatorKeyFailed());


            await _unitOfWork.SaveChangesAsync();
            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            return new AuthenticatorKey(authenticatorKey, twoFAType);
        }

        public async Task<IdentityResult> EnableTwoFactor(string username, string twoFactorCode)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return IdentityResult.Failed(_errorDescriber.UnknownUserName(username));

            var authenticatorEnabled = await _userManager.GetTwoFactorEnabledAsync(user);
            if (authenticatorEnabled)
                return IdentityResult.Failed(_errorDescriber.TwoFactorAlreadyEnabled());

            var authenticatorKey = await _userManager.GetAuthenticatorKeyAsync(user);
            if (string.IsNullOrEmpty(authenticatorKey))
                return IdentityResult.Failed(_errorDescriber.NewAuthenticatorKeyRequired());

            var tokenValid = await _userManager.VerifyTwoFactorTokenAsync(user,
                               _userManager.Options.Tokens.AuthenticatorTokenProvider, twoFactorCode);
            if (!tokenValid)
                return IdentityResult.Failed(_errorDescriber.InvalidTwoFactorToken());

            await _userManager.SetTwoFactorEnabledAsync(user, true);

            await _unitOfWork.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DisableTwoFactor(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return IdentityResult.Failed(_errorDescriber.UnknownUserName(username));

            // ResetAuthenticatorKeyAsync also updates user
            user.TwoFactorEnabled = false;
            // Authenticator key reset so that two factor can't be enabled to the same authenticator again
            await _userManager.ResetAuthenticatorKeyAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return IdentityResult.Success;
        }
    }
}
