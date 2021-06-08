using IdentityCore.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SR = OpenIddict.Abstractions.OpenIddictResources;

namespace IdentityCore
{
    public class ExtendedOpenIddictTokenManager<TToken> : OpenIddictTokenManager<TToken> where TToken : class,
        IExtendedOpenIddictTokenManager
    {
        public ExtendedOpenIddictTokenManager(
                IOpenIddictTokenCache<TToken> cache,
                ILogger<OpenIddictTokenManager<TToken>> logger,
                IOptionsMonitor<OpenIddictCoreOptions> options,
                IExtendedOpenIddictTokenStoreResolver resolver)
                : base(cache, logger, options, resolver)
        {
            Store = resolver.GetExtended<TToken>();
        }


        /// <summary>
        /// Gets the store associated with the current manager.
        /// NB - Intentionally hiding the existing implementation of Store from the base class.
        /// </summary>
        protected new IExtendedOpenIddictTokenStore<TToken> Store { get; }



        /// <summary>
        /// Populates the token using the specified descriptor.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="descriptor">The descriptor.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>
        /// A <see cref="ValueTask"/> that can be used to monitor the asynchronous operation.
        /// </returns>
        public async override ValueTask PopulateAsync(TToken token,
            OpenIddictTokenDescriptor descriptor, CancellationToken cancellationToken = default)
        {
            if (token is null)
            {
                throw new ArgumentNullException(nameof(token));
            }

            if (descriptor is null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            

            await Store.SetApplicationIdAsync(token, descriptor.ApplicationId, cancellationToken);
            await Store.SetAuthorizationIdAsync(token, descriptor.AuthorizationId, cancellationToken);
            await Store.SetCreationDateAsync(token, descriptor.CreationDate, cancellationToken);
            await Store.SetExpirationDateAsync(token, descriptor.ExpirationDate, cancellationToken);
            await Store.SetPayloadAsync(token, descriptor.Payload, cancellationToken);

            await Store.SetTenantIdAsync(token, descriptor.Principal.GetClaim("TenantId"), cancellationToken);

            var pat = descriptor.Principal.GetClaim("PAT");
            if (null != pat)
            {
                var dict = new Dictionary<string, JsonElement>() { { "PAT", JsonDocument.Parse(pat).RootElement } }.ToImmutableDictionary();
                await Store.SetPropertiesAsync(token, dict, cancellationToken);
            }
            await Store.SetRedemptionDateAsync(token, descriptor.RedemptionDate, cancellationToken);
            await Store.SetReferenceIdAsync(token, descriptor.ReferenceId, cancellationToken);
            await Store.SetStatusAsync(token, descriptor.Status, cancellationToken);
            await Store.SetSubjectAsync(token, descriptor.Subject, cancellationToken);
            await Store.SetTypeAsync(token, descriptor.Type, cancellationToken);
        }



        /// <summary>
        /// Retrieves the tokens matching the specified parameters.
        /// </summary>
        /// <param name="userId">The userId associated with the token.</param>
        /// <param name="tenantId">The tenantId associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>Tokens corresponding to the criteria.</returns>
        public virtual IAsyncEnumerable<TToken> FindPATAsync(
            string userId, string tenantId,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(userId))
            {
                throw new ArgumentException(SR.GetResourceString(SR.ID0198), nameof(userId));
            }

            if (string.IsNullOrEmpty(tenantId))
            {
                throw new ArgumentException(SR.GetResourceString(SR.ID0124), nameof(tenantId));
            }

            var status = "valid";
            var type = "access_token";

            var tokens = Options.CurrentValue.DisableEntityCaching ?
                Store.FindAsyncByUserAndTenant(userId, tenantId, status, type, cancellationToken) :
                Cache.FindAsync(userId, tenantId, status, type, cancellationToken);

            if (Options.CurrentValue.DisableAdditionalFiltering)
            {
                return tokens;
            }

            // SQL engines like Microsoft SQL Server or MySQL are known to use case-insensitive lookups by default.
            // To ensure a case-sensitive comparison is enforced independently of the database/table/query collation
            // used by the store, a second pass using string.Equals(StringComparison.Ordinal) is manually made here.

            return ExecuteAsync(cancellationToken);

            async IAsyncEnumerable<TToken> ExecuteAsync([EnumeratorCancellation] CancellationToken cancellationToken)
            {
                await foreach (var token in tokens)
                {
                    if (string.Equals(await Store.GetSubjectAsync(token, cancellationToken), userId, StringComparison.Ordinal))
                    {
                        yield return token;
                    }
                }
            }
        }
    }

}
