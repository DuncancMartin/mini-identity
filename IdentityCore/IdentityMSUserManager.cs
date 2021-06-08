using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using IdentityCore.Interfaces.Repositories;
using IdentityCore.Interfaces.Services;
using IdentityCore.Interfaces.UnitOfWork;
using IdentityCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace IdentityCore
{
    /// <summary>
    /// Extended user manager that does not save changes automatically.
    /// </summary>
    public class IdentityMSUserManager : UserManager<IdentityMSUser>
    {
        private readonly IIdentityMSUserStore _store;
        //private readonly ITenantService _tenantService;
        //private readonly IUserModuleAccessService _userModuleAccessService;
        //private readonly ITimeService _timeService;
        private readonly TwoFactorConfig _twoFactorConfig;

        public IdentityMSUserManager(IUserStore<IdentityMSUser> store,
                                     IOptions<IdentityOptions> optionsAccessor,
                                     IPasswordHasher<IdentityMSUser> passwordHasher,
                                     IEnumerable<IUserValidator<IdentityMSUser>> userValidators,
                                     IEnumerable<IPasswordValidator<IdentityMSUser>> passwordValidators,
                                     ILookupNormalizer keyNormalizer,
                                     IdentityErrorDescriber errors,
                                     IServiceProvider services,
                                     ILogger<UserManager<IdentityMSUser>> logger,
                                     //ITenantService tenantService,
                                     //IUserModuleAccessService userModuleAccessService,
                                     //ITimeService timeService,
                                     TwoFactorConfig twoFactorConfig)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators,
                   keyNormalizer, errors, services, logger)
        {
            _store = store as IIdentityMSUserStore;
            //_tenantService = tenantService;
            //_userModuleAccessService = userModuleAccessService;
            //_timeService = timeService;
            _twoFactorConfig = twoFactorConfig;
        }

        /// <summary>
        /// Creates the specified <paramref name="user"/> in the backing store with no password,
        /// as an asynchronous operation.
        /// </summary>
        public override Task<IdentityResult> CreateAsync(IdentityMSUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return base.CreateAsync(user);
        }

        /// <summary>
        /// Finds and returns a user, if any, who has the specified <paramref name="userId"/>.
        /// Optionally also finds the users module and entity access.
        /// </summary>
        public Task<IdentityMSUser> FindByIdAsync(int userId, bool includeModuleAccess = false)
        {
            ThrowIfDisposed();
            //if (includeModuleAccess)
            //    return _store.FindByIdWithModuleAccessAsync(userId, CancellationToken);

            return _store.FindByIdAsync(userId, CancellationToken);
        }

        public Task<IList<IdentityMSUser>> FindByTenantIdAsync(int tenantId)
        {
            ThrowIfDisposed();
            return _store.FindByTenantIdAsync(tenantId, CancellationToken);
        }

        ///// <summary>
        ///// Sets the specified users module and entity access to the list specified by <paramref name="moduleAccesses"/>.
        ///// </summary>
        //public async Task<IdentityResult> SetUserModuleAndEntityAccessAsync(IdentityMSUser user,
        //    IEnumerable<UserModuleAccess> moduleAccesses)
        //{
        //    ThrowIfDisposed();
        //    if (user == null)
        //        throw new ArgumentNullException(nameof(user));

        //    if (moduleAccesses == null)
        //        throw new ArgumentNullException(nameof(moduleAccesses));

        //    var userId = user.Id;

        //    // Get current access from database if required
        //    var currentModuleAccess = user.UserModuleAccess ??
        //                              await _userModuleAccessService.GetUserModuleAccessTracking(userId);

        //    _userModuleAccessService.SetUserModuleAndEntityAccess(userId, currentModuleAccess, moduleAccesses);

        //    return IdentityResult.Success;
        //}

        /// <summary>
        /// Changes a user's password after confirming the specified <paramref name="currentPassword"/> is correct,
        /// as an asynchronous operation.
        /// </summary>
        public override Task<IdentityResult> ChangePasswordAsync(IdentityMSUser user, string currentPassword,
            string newPassword)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            return base.ChangePasswordAsync(user, currentPassword, newPassword);
        }

        /// <summary>
        /// Gets a flag indicating whether the password for the specified <paramref name="user"/> has been verified,
        /// true if the password is verified otherwise false.
        /// </summary>
        public Task<bool> GetPasswordConfirmedAsync(IdentityMSUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return Task.FromResult(user.PasswordConfirmed);
        }

        /// <summary>
        /// Updates a user's password hash while checking it against password constraints.
        /// </summary>
        public async Task<IdentityResult> UpdatePasswordAsync(IdentityMSUser user, string password)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (password == null)
                throw new ArgumentNullException(nameof(password));

            var result = await UpdatePasswordHash(user, password, true);

            if (!result.Succeeded)
                return result;

            // The user has to reset his password on the next login
            // TODO: Set this to false again once password changes are implemented in the UI
            user.PasswordConfirmed = true;

            return IdentityResult.Success;
        }


        public Task<string> GeneratePasswordToken(IdentityMSUser user)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            // This is updated automatically.
            user.PasswordToken = GeneratePasswordToken(_twoFactorConfig.PasswordTokenBytes);
            //user.PasswordTokenExpiryTime = _timeService.Now() + _twoFactorConfig.PasswordTokenDuration;

            return Task.FromResult(user.PasswordToken);
        }

        private static string GeneratePasswordToken(int numberOfBytes)
        {
            using (var rng = new RNGCryptoServiceProvider())
            {
                var bytes = new byte[numberOfBytes];
                rng.GetBytes(bytes);
                return Base64UrlEncoder.Encode(bytes);
            }
        }

        public Task<bool> VerifyPasswordTokenAsync(IdentityMSUser user, string passwordToken)
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (string.IsNullOrEmpty(passwordToken))
                throw new ArgumentNullException(nameof(passwordToken));

            if (string.IsNullOrEmpty(user.PasswordToken) || user.PasswordTokenExpiryTime == null)
                return Task.FromResult(false);

            //if (user.PasswordTokenExpiryTime < _timeService.Now())
            //    return Task.FromResult(false);

            return Task.FromResult(user.PasswordToken == passwordToken);
        }

        //public Task<IList<ModulePermission>> FindRolesDetailAsync(IdentityMSUser user)
        //{
        //    ThrowIfDisposed();
        //    return _store.FindRolesDetailAsync(user, CancellationToken);
        //}
    }
}
