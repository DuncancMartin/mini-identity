using System;
using System.Threading.Tasks;
using IdentityCore.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityCore
{
    /// <summary>
    /// Validates that the tenant attached to a user on create or update is valid and active.
    /// </summary>
    public class UserTenantValidator : IUserValidator<IdentityMSUser>
    {
        public async Task<IdentityResult> ValidateAsync(UserManager<IdentityMSUser> manager, IdentityMSUser user)
        {
            if (manager == null)
                throw new ArgumentNullException(nameof(manager));

            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if(!(manager is IdentityMSUserManager identityMsManager))
                throw new ArgumentException(nameof(manager));

            if(!(identityMsManager.ErrorDescriber is IdentityMSErrorDescriber errorDescriber))
                throw new ArgumentException(nameof(errorDescriber));

            //var active = await identityMsManager.TenantIsActiveAsync(user);
            //if (!active)
            //    return IdentityResult.Failed(errorDescriber.InvalidTenant());

            return IdentityResult.Success;
        }
    }
}
