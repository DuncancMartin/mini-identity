using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IdentityCore.Interfaces
{
    public interface IExtendedOpenIddictTokenManager : IOpenIddictTokenManager
    {
        IAsyncEnumerable<object> FindPATAsync(string userId, string tenantId, CancellationToken cancellationToken = default);
    }
}
