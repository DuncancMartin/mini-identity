using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityCore.Interfaces
{
    public interface IExtendedOpenIddictTokenStore<TToken> : IOpenIddictTokenStore<TToken> where TToken : class 
    {
        /// <summary>
        /// Sets the tenantId associated with a token.
        /// </summary>
        /// <param name="token">The token.</param>
        /// <param name="identifier">The tenantId associated with the token.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>A <see cref="ValueTask"/> that can be used to monitor the asynchronous operation.</returns>
        ValueTask SetTenantIdAsync(TToken token, string? identifier, CancellationToken cancellationToken);


        /// <summary>
        /// Retrieves the tokens matching the specified parameters.
        /// </summary>
        /// <param name="userId">The userId associated with the token.</param>
        /// <param name="tenantId">The tenantId associated with the token.</param>
        /// <param name="status">The token status.</param>
        /// <param name="type">The token type.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> that can be used to abort the operation.</param>
        /// <returns>The tokens corresponding to the criteria.</returns>
        IAsyncEnumerable<TToken> FindAsyncByUserAndTenant(
            string userId, string tenantId,
            string status, string type, CancellationToken cancellationToken);

    }
}
