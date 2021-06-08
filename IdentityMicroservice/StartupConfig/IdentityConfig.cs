using System;
using IdentityApi.Middleware;
using IdentityCore;
using IdentityCore.Interfaces;
using IdentityCore.Models;
using IdentityInfrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenIddict.Abstractions;
using OpenIddict.Server;
using static OpenIddict.Server.OpenIddictServerEvents;

namespace IdentityApi.StartupConfig
{
    public static class IdentityConfig
    {
        // Configured here so it can be injected and then returned with a token response
        private static TimeSpan RefreshTokenLifeTime => TimeSpan.FromDays(14);
        private static TimeSpan ExtendedAuthTokenLifeTime => TimeSpan.FromDays(7);

        // A multiple of 3 is chosen so that no padding is needed for the base 64 string
        private const int PasswordTokenBytes = 36;
        private static readonly TimeSpan PasswordTokenDuration = TimeSpan.FromMinutes(5);

        /// <summary>
        /// Configures user and token settings.
        /// </summary>
        public static void AddImagineIdentityConfig(this IServiceCollection services)
        {
            services.AddIdentity<IdentityMSUser, IdentityMSRole>(options =>
                {
                    // User password settings as dictated by Matt
                    options.Password.RequireDigit = true;
                    options.Password.RequiredLength = 8;
                    options.Password.RequireNonAlphanumeric = true;
                    options.Password.RequireUppercase = true;
                    options.Password.RequireLowercase = true;

                    // Lockout timer
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
                    options.Lockout.MaxFailedAccessAttempts = 10;
                    options.Lockout.AllowedForNewUsers = true;

                    // User settings
                    options.User.RequireUniqueEmail = false;
                })
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddUserStore<IdentityMSUserStore>()
                .AddUserManager<IdentityMSUserManager>()
                .AddUserValidator<UserTenantValidator>()
                .AddErrorDescriber<IdentityMSErrorDescriber>()
                .AddDefaultTokenProviders();

            services.Configure<IdentityOptions>(options =>
            {
                options.ClaimsIdentity.UserNameClaimType = OpenIddictConstants.Claims.Name;
                options.ClaimsIdentity.UserIdClaimType = OpenIddictConstants.Claims.Subject;
                options.ClaimsIdentity.RoleClaimType = OpenIddictConstants.Claims.Role;
            });

            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                        .UseDbContext<IdentityDbContext>()
                        .ReplaceDefaultEntities<CustomApplication, CustomAuthorization, CustomScope, ExtendedOpenIddictEntityFrameworkCoreToken, long>();
                    //  .ReplaceApplicationManager(typeof(OpenIdApplicationManager<>))
                    //  .ReplaceAuthorizationManager(typeof(OpenIdAuthorizationManager<>))
                    options.ReplaceTokenManager(typeof(ExtendedOpenIddictTokenManager<>))
                           .ReplaceTokenStoreResolver(typeof(ExtendedOpenIddictTokenStoreResolver));

                    options.Services.TryAddScoped(provider => (IExtendedOpenIddictTokenManager)
                        provider.GetRequiredService<IOpenIddictTokenManager>());

                })
                .AddServer(options =>
                {
                    options.SetTokenEndpointUris("/token");
                    options.SetIntrospectionEndpointUris("/introspect");

                    //Don't enforce permissions inside the cluster - described here: https://kevinchalet.com/2018/06/20/openiddict-rc3-is-out/
                    options.IgnoreEndpointPermissions()
                      .IgnoreGrantTypePermissions()
                      .IgnoreScopePermissions()
                      .IgnoreResponseTypePermissions();

                    options.DisableRollingRefreshTokens();
                    
                    options.AllowPasswordFlow()
                        .AllowRefreshTokenFlow()
                        .AllowCustomFlow("2fa")
                        .AllowCustomFlow("PAT")
                        .SetAccessTokenLifetime(TimeSpan.FromMinutes(30))
                        .SetRefreshTokenLifetime(RefreshTokenLifeTime);

                    options.UseAspNetCore()
                        .EnableTokenEndpointPassthrough()
                        // Gateways call with http
                        .DisableTransportSecurityRequirement();

                    options.UseDataProtection();
                    options.AddEphemeralSigningKey()
                        .AddEphemeralEncryptionKey();

                    options.AddEventHandler<ApplyTokenResponseContext>(builder =>
                    {
                        builder.UseInlineHandler(TokenResponseHandler.PublicPropertyHandler);
                    });

                    // Disables the ClientId requirement
                    options.AcceptAnonymousClients();
                })
                .AddValidation(options =>
                {
                    options.EnableTokenEntryValidation();
                    options.EnableAuthorizationEntryValidation();
                }
                );

            services.AddSingleton(new TokenConfig(RefreshTokenLifeTime, ExtendedAuthTokenLifeTime));
            services.AddSingleton(new TwoFactorConfig(PasswordTokenBytes, PasswordTokenDuration));
        }
    }
}