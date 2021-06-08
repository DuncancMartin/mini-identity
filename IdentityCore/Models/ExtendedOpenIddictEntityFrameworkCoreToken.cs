using OpenIddict.Abstractions;
using OpenIddict.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IdentityCore.Models
{
    /// <summary>
    /// Represents an Extended OpenIddict token, inherits from other Extended token.
    /// </summary>
    public class ExtendedOpenIddictEntityFrameworkCoreToken : ExtendedOpenIddictEntityFrameworkCoreToken<long, CustomApplication, CustomAuthorization>
    {
        //public ExtendedOpenIddictEntityFrameworkCoreToken()
        //{
        //    // Generate a new string identifier.
        //    Id = Guid.NewGuid().ToString();
        //}
    }

    /// <summary>
    /// Represents an Extended OpenIddict token, inherits from other Extended token.
    /// </summary>
    public class ExtendedOpenIddictEntityFrameworkCoreToken<TKey> : ExtendedOpenIddictEntityFrameworkCoreToken<TKey, CustomApplication<TKey>, CustomAuthorization<TKey>>
        where TKey : IEquatable<TKey>
    {
    }

    /// <summary>
    /// Represents an Extended OpenIddict token, inherits from other original token.
    /// </summary>
    public class ExtendedOpenIddictEntityFrameworkCoreToken<TKey, TApplication, TAuthorization> : OpenIddictEntityFrameworkCoreToken<TKey, TApplication, TAuthorization>
                where TKey : IEquatable<TKey>
                where TApplication : class
                where TAuthorization : class
    {
        public string TenantId { get; set; }
    }



    public class CustomApplication : OpenIddictEntityFrameworkCoreApplication<long, CustomAuthorization, ExtendedOpenIddictEntityFrameworkCoreToken> { }
    public class CustomApplication<TKey> : OpenIddictEntityFrameworkCoreApplication<TKey, CustomAuthorization<TKey>, ExtendedOpenIddictEntityFrameworkCoreToken<TKey>>
        where TKey : IEquatable<TKey>    { }
    public class CustomApplication<TKey, TAuthorization, TToken> : OpenIddictEntityFrameworkCoreApplication<TKey, TAuthorization, TToken>
        where TKey : IEquatable<TKey>
        where TAuthorization : class
        where TToken : class    { }


    public class CustomAuthorization : OpenIddictEntityFrameworkCoreAuthorization<long, CustomApplication, ExtendedOpenIddictEntityFrameworkCoreToken> { }
    public class CustomAuthorization<TKey> : OpenIddictEntityFrameworkCoreAuthorization<TKey, CustomApplication<TKey>, ExtendedOpenIddictEntityFrameworkCoreToken<TKey>>
        where TKey : IEquatable<TKey>    { }
    public class CustomAuthorization<TKey, TApplication, TToken> : OpenIddictEntityFrameworkCoreAuthorization<TKey, TApplication, TToken>
        where TKey : IEquatable<TKey>
        where TApplication : class
        where TToken : class    { }


    public class CustomScope : OpenIddictEntityFrameworkCoreScope<long> { }

}
