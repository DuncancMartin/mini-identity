using OpenIddict.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityCore.Interfaces
{
    public interface IExtendedOpenIddictTokenStoreResolver : IOpenIddictTokenStoreResolver
    {
        IExtendedOpenIddictTokenStore<TToken> GetExtended<TToken>() where TToken : class;
    }
}
