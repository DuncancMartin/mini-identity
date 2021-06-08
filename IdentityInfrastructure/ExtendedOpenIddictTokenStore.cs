using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityCore.Interfaces;
using Microsoft.EntityFrameworkCore;
using OpenIddict.EntityFrameworkCore.Models;
using OpenIddict.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using IdentityCore.Models;
using System.Threading;
using SR = OpenIddict.Abstractions.OpenIddictResources;


namespace IdentityInfrastructure
{
    public class ExtendedOpenIddictEntityFrameworkTokenStore<TToken, TApplication, TAuthorization, TContext, TKey> : OpenIddictEntityFrameworkCoreTokenStore<TToken, TApplication, TAuthorization, TContext, TKey>
        where TToken : ExtendedOpenIddictEntityFrameworkCoreToken<TKey, TApplication, TAuthorization>
        where TApplication : CustomApplication<TKey, TAuthorization, TToken>
        where TAuthorization : CustomAuthorization<TKey, TApplication, TToken>
        where TContext : DbContext
        where TKey : IEquatable<TKey>
        , IExtendedOpenIddictTokenStore<TToken>
    {

        public ExtendedOpenIddictEntityFrameworkTokenStore(
            IMemoryCache cache,
            TContext context,
            IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options)
            : base(cache, context, options)
        {
        }

        /// <summary>
        /// Gets the database set corresponding to the <typeparamref name="TApplication"/> entity.
        /// </summary>
        private DbSet<TApplication> Applications => Context.Set<TApplication>();

        /// <summary>
        /// Gets the database set corresponding to the <typeparamref name="TAuthorization"/> entity.
        /// </summary>
        private DbSet<TAuthorization> Authorizations => Context.Set<TAuthorization>();

        /// <summary>
        /// Gets the database set corresponding to the <typeparamref name="TToken"/> entity.
        /// </summary>
        private DbSet<TToken> Tokens => Context.Set<TToken>();


        /// <inheritdoc/>
        public virtual ValueTask SetTenantIdAsync(TToken token, string? tenantId, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            token.TenantId = tenantId;

            return default;
        }

        /// <inheritdoc/>
        public virtual IAsyncEnumerable<TToken> FindAsyncByUserAndTenant(
            string userId, string tenantId,
            string status, string type, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(userId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException(SR.GetResourceString(SR.ID0124), nameof(tenantId));
            }

            if (string.IsNullOrEmpty(status))
            {
                throw new ArgumentException(SR.GetResourceString(SR.ID0199), nameof(status));
            }

            if (string.IsNullOrEmpty(type))
            {
                throw new ArgumentException(SR.GetResourceString(SR.ID0200), nameof(type));
            }

            // Note: due to a bug in Entity Framework Core's query visitor, the authorizations can't be
            // filtered using token.Application.Id.Equals(key). To work around this issue,
            // this compiled query uses an explicit join before applying the equality check.
            // See https://github.com/openiddict/openiddict-core/issues/499 for more information.

            var key = ConvertIdentifierFromString(tenantId);

            return (from token in Tokens.Include(token => token.Application).Include(token => token.Authorization).AsTracking()
                    where token.Subject == userId &&
                          token.Status == status &&
                          token.Type == type &&
                          token.TenantId == tenantId
                    join application in Applications.AsTracking() on token.Application!.Id equals application.Id
                    where application.Id!.Equals(key)
                    select token).AsAsyncEnumerable();
            //TODO Should be this but I can't get my extension to work???
            //select token).AsAsyncEnumerable(cancellationToken); 
        }

    }
}