using IdentityCore.Interfaces;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore;
using System;
using Microsoft.Extensions.DependencyInjection;
using SR = OpenIddict.Abstractions.OpenIddictResources;

namespace IdentityCore
{
    public class ExtendedOpenIddictTokenStoreResolver : OpenIddictEntityFrameworkCoreTokenStoreResolver, IExtendedOpenIddictTokenStoreResolver
    {
        private readonly TypeResolutionCache _cache;
        private readonly IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> _options;
        private readonly IServiceProvider _provider;

        public ExtendedOpenIddictTokenStoreResolver(
            TypeResolutionCache cache,
            IOptionsMonitor<OpenIddictEntityFrameworkCoreOptions> options,
            IServiceProvider provider) : base(cache, options, provider)
        {
            _cache = cache;
            _options = options;
            _provider = provider;
        }

        public IExtendedOpenIddictTokenStore<TToken> GetExtended<TToken>() where TToken : class
        {
            var store = _provider.GetService<IExtendedOpenIddictTokenStore<TToken>>();


            if (store is not null)
            {
                return store;
            }

            throw new NotImplementedException("Ooops");

        }

    }
}
        

            //TODO work out if this commented code (nicked from OpenIddict source) is needed.
            //var type = _cache.GetOrAdd(typeof(TToken), key =>
            //{
            //    var root = OpenIddictHelpers.FindGenericBaseType(key, typeof(OpenIddictEntityFrameworkCoreToken<,,>));
            //    if (root is null)
            //    {
            //        throw new InvalidOperationException(SR.GetResourceString(SR.ID0256));
            //    }

            //    var context = _options.CurrentValue.DbContextType;
            //    if (context is null)
            //    {
            //        throw new InvalidOperationException(SR.GetResourceString(SR.ID0253));
            //    }

            //    return typeof(OpenIddictEntityFrameworkCoreTokenStore<,,,,>).MakeGenericType(
            //        /* TToken: */ key,
            //        /* TApplication: */ root.GenericTypeArguments[1],
            //        /* TAuthorization: */ root.GenericTypeArguments[2],
            //        /* TContext: */ context,
            //        /* TKey: */ root.GenericTypeArguments[0]);
            //});

            //return (IExtendedOpenIddictTokenStore<TToken>)_provider.GetRequiredService(type);