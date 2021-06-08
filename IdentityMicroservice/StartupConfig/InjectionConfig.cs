using IdentityCore;
using IdentityCore.Interfaces;
using IdentityCore.Interfaces.Repositories;
using IdentityCore.Interfaces.Services;
using IdentityCore.Interfaces.UnitOfWork;
using IdentityCore.Services;
using IdentityCore.UnitOfWork;
using IdentityInfrastructure;
using IdentityInfrastructure.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace IdentityApi.StartupConfig
{
    public static class InjectionConfig
    {
        public static void AddInjection(this IServiceCollection services)
        {
            services.AddScoped<IAuthenticateService, AuthenticateService>();

            services.AddScoped<IDataContextFactory, DataContextFactory>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<IUserService, UserService>();

            services.AddScoped(typeof(IRepository<>), typeof(CrudRepository<>));

            services.AddScoped<IUserGroupService, UserGroupService>();
            services.AddScoped<IUserGroupRepository, UserGroupRepository>();

            services.AddScoped<ITwoFactorService, TwoFactorService>();

            services.AddScoped<ISessionService, SessionService>();

            services.AddScoped<IPersonalAccessTokenService, PersonalAccessTokenService>();
            services.AddScoped(typeof(ExtendedOpenIddictTokenManager<>));
            //services.AddScoped(typeof(IExtendedOpenIddictTokenStore<>), typeof(ExtendedOpenIddictEntityFrameworkTokenStore<,,,,>)();
            services.AddScoped(typeof(ExtendedOpenIddictEntityFrameworkTokenStore<,,,,>));
            services.AddScoped<IExtendedOpenIddictTokenStoreResolver, ExtendedOpenIddictTokenStoreResolver>();
            
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
          
        }
    }
}