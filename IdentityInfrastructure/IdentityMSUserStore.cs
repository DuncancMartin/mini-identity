using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using IdentityCore.Interfaces.Repositories;
using IdentityCore.Interfaces.UnitOfWork;
using IdentityCore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace IdentityInfrastructure
{
    /// <summary>
    /// Extended Identity user store. Does not save changes automatically.
    /// </summary>
    public class IdentityMSUserStore : UserStoreBase<IdentityMSUser, int, IdentityUserClaim<int>, IdentityUserLogin<int>,
        IdentityUserToken<int>>, IIdentityMSUserStore
    {
        private readonly IDataContext _context;

        private DbSet<IdentityMSUser> UsersSet => _context.Set<IdentityMSUser>();
        private DbSet<IdentityUserClaim<int>> UserClaims => _context.Set<IdentityUserClaim<int>>();
        private DbSet<IdentityUserLogin<int>> UserLogins => _context.Set<IdentityUserLogin<int>>();
        private DbSet<IdentityUserToken<int>> UserTokens => _context.Set<IdentityUserToken<int>>();
        public override IQueryable<IdentityMSUser> Users => UsersSet;
        private DbSet<IdentityUserRole<int>> UserRoles => _context.Set<IdentityUserRole<int>>();
        private DbSet<IdentityMSRole> Roles => _context.Set<IdentityMSRole>();

        public IdentityMSUserStore(IDataContextFactory contextFactory, IdentityErrorDescriber describer)
            : base(describer)
        {
            _context = contextFactory.GetDataContext();
        }

        public bool AutoSaveChanges { get; set; } = false;

        private Task SaveChanges(CancellationToken cancellationToken)
        {
            return AutoSaveChanges ? _context.SaveChangesAsync(cancellationToken) : Task.CompletedTask;
        }

        public override async Task<IdentityResult> CreateAsync(IdentityMSUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            UsersSet.Add(user);
            await SaveChanges(cancellationToken);
            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> UpdateAsync(IdentityMSUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            UsersSet.Attach(user);
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            UsersSet.Update(user);
            try
            {
                await SaveChanges(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }
            return IdentityResult.Success;
        }

        public override async Task<IdentityResult> DeleteAsync(IdentityMSUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            UsersSet.Remove(user);
            try
            {
                await SaveChanges(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                return IdentityResult.Failed(ErrorDescriber.ConcurrencyFailure());
            }
            return IdentityResult.Success;
        }

        public override Task<IdentityMSUser> FindByIdAsync(string userId, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            var id = ConvertIdFromString(userId);
            return UsersSet.FindAsync(new object[] { id }, cancellationToken).AsTask();
        }

        public override Task<IdentityMSUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return Users.FirstOrDefaultAsync(i => i.NormalizedUserName == normalizedUserName, cancellationToken);
        }

        protected override Task<IdentityMSUser> FindUserAsync(int userId, CancellationToken cancellationToken)
        {
            return Users.SingleOrDefaultAsync(i => i.Id == userId, cancellationToken);
        }

        protected override Task<IdentityUserLogin<int>> FindUserLoginAsync(int userId, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return UserLogins.SingleOrDefaultAsync(i => i.UserId == userId &&
                                                        i.LoginProvider == loginProvider &&
                                                        i.ProviderKey == providerKey,
                                                    cancellationToken);
        }

        protected override Task<IdentityUserLogin<int>> FindUserLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            return UserLogins.SingleOrDefaultAsync(i => i.LoginProvider == loginProvider &&
                                                        i.ProviderKey == providerKey,
                                                   cancellationToken);
        }

        public override async Task<IList<Claim>> GetClaimsAsync(IdentityMSUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return await UserClaims
                .Where(i => i.UserId.Equals(user.Id))
                .Select(i => i.ToClaim())
                .ToListAsync(cancellationToken);
        }

        public override Task AddClaimsAsync(IdentityMSUser user, IEnumerable<Claim> claims,
            CancellationToken cancellationToken = new CancellationToken())
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (claims == null)
                throw new ArgumentNullException(nameof(claims));

            foreach (var claim in claims)
            {
                UserClaims.Add(CreateUserClaim(user, claim));
            }
            return Task.CompletedTask;
        }

        public override async Task ReplaceClaimAsync(IdentityMSUser user, Claim claim, Claim newClaim,
            CancellationToken cancellationToken = new CancellationToken())
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            if (newClaim == null)
                throw new ArgumentNullException(nameof(newClaim));

            var matchedClaims = await UserClaims
                .Where(i => i.UserId.Equals(user.Id) &&
                            i.ClaimValue == claim.Value &&
                            i.ClaimType == claim.Type)
                .ToListAsync(cancellationToken);

            foreach (var matchedClaim in matchedClaims)
            {
                matchedClaim.ClaimValue = newClaim.Value;
                matchedClaim.ClaimType = newClaim.Type;
            }
        }

        public override async Task RemoveClaimsAsync(IdentityMSUser user, IEnumerable<Claim> claims,
            CancellationToken cancellationToken = new CancellationToken())
        {
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (claims == null)
                throw new ArgumentNullException(nameof(claims));

            foreach (var claim in claims)
            {
                var matchedClaims = await UserClaims
                    .Where(i => i.UserId.Equals(user.Id) &&
                                i.ClaimValue == claim.Value &&
                                i.ClaimType == claim.Type)
                    .ToListAsync(cancellationToken);

                foreach (var matchedClaim in matchedClaims)
                {
                    UserClaims.Remove(matchedClaim);
                }
            }
        }

        public override async Task<IList<IdentityMSUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (claim == null)
                throw new ArgumentNullException(nameof(claim));

            return await UserClaims
                .Where(i => i.ClaimValue == claim.Value &&
                            i.ClaimType == claim.Type)
                .Join(Users,
                      userClaim => userClaim.UserId,
                      user => user.Id,
                      (userClaim, user) => user)
                .ToListAsync(cancellationToken);
        }

        protected override Task<IdentityUserToken<int>> FindTokenAsync(IdentityMSUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return UserTokens.FindAsync(new object[] { user.Id, loginProvider, name }, cancellationToken).AsTask();
        }

        protected override Task AddUserTokenAsync(IdentityUserToken<int> token)
        {
            UserTokens.Add(token);
            return Task.CompletedTask;
        }

        protected override Task RemoveUserTokenAsync(IdentityUserToken<int> token)
        {
            UserTokens.Remove(token);
            return Task.CompletedTask;
        }

        public override Task AddLoginAsync(IdentityMSUser user, UserLoginInfo login,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            if (login == null)
                throw new ArgumentNullException(nameof(login));

            UserLogins.Add(CreateUserLogin(user, login));
            return Task.CompletedTask;
        }

        public override async Task RemoveLoginAsync(IdentityMSUser user, string loginProvider, string providerKey,
            CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            var login = await FindUserLoginAsync(user.Id, loginProvider, providerKey, cancellationToken);
            if (login != null)
                UserLogins.Remove(login);
        }

        public override async Task<IList<UserLoginInfo>> GetLoginsAsync(IdentityMSUser user, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            return await UserLogins
                .Where(i => i.UserId == user.Id)
                .Select(i => new UserLoginInfo(i.LoginProvider, i.ProviderKey, i.ProviderDisplayName))
                .ToListAsync(cancellationToken);
        }

        public override Task<IdentityMSUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return Users.FirstOrDefaultAsync(i => i.NormalizedEmail == normalizedEmail, cancellationToken);
        }

        public Task<IdentityMSUser> FindByIdAsync(int userId, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return Users.SingleOrDefaultAsync(i => i.Id == userId, cancellationToken);
        }

        public async Task<IList<IdentityMSUser>> FindByTenantIdAsync(int tenantId, CancellationToken cancellationToken = new CancellationToken())
        {
            cancellationToken.ThrowIfCancellationRequested();
            ThrowIfDisposed();
            return await Users.Where(i => i.TenantId == tenantId).ToListAsync<IdentityMSUser>(cancellationToken);
        }

        //public Task<IdentityMSUser> FindByIdWithModuleAccessAsync(int userId, CancellationToken cancellationToken = new CancellationToken())
        //{
        //    cancellationToken.ThrowIfCancellationRequested();
        //    ThrowIfDisposed();
        //    return Users
        //        .Include(i => i.UserModuleAccess)
        //            .ThenInclude(i => i.UserModuleEntities)
        //        .SingleOrDefaultAsync(i => i.Id == userId, cancellationToken);
        //}


        //public async Task<IList<ModulePermission>> FindRolesDetailAsync(IdentityMSUser user, CancellationToken cancellationToken)
        //{
        //    /* Find all the roles assigned to the user and return the aggregated permission value for each module */
        //    cancellationToken.ThrowIfCancellationRequested();
        //    ThrowIfDisposed();
        //    var userRoles = await (UserRoles
        //        .Where(ur => ur.UserId == user.Id)
        //        .Join(
        //          Roles,
        //          userRole => userRole.RoleId,
        //          msRole => msRole.Id,
        //          (userRole, msRole) => new
        //          {
        //              msRole.PermissionValue,
        //              msRole.PermissionName,
        //              msRole.Module.AuthorizationId
        //          })).ToListAsync();

        //    return userRoles
        //        .GroupBy(r => r.AuthorizationId)
        //        .Select(pm => new ModulePermission()
        //        {
        //            AggregatePermissionValue = pm.Sum(p => p.PermissionValue),
        //            AuthorizationId = pm.Key
        //        })
        //        .ToList();
        //}

    }
}
