using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityCore;
using IdentityCore.Interfaces.Repositories;
using IdentityCore.Interfaces.Services;
using IdentityCore.Interfaces.UnitOfWork;
using IdentityCore.Models;
using IdentityCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MockQueryable.Moq;
using Moq;

namespace IdentityUnitTest
{
    public static class MockHelper
    {
        public static IdentityMSUserManager TestUserManager(IUserStore<IdentityMSUser> store = null, ITenantService tenantService = null)
        {
            store = store ?? new Mock<IUserStore<IdentityMSUser>>().Object;
            tenantService = tenantService ?? new Mock<ITenantService>().Object;

            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions {Lockout = {AllowedForNewUsers = false}};
            options.Setup(o => o.Value).Returns(idOptions);


            var userValidators = new List<IUserValidator<IdentityMSUser>>();
            var validator = new Mock<IUserValidator<IdentityMSUser>>();
            userValidators.Add(validator.Object);

            userValidators.Add(new UserTenantValidator());

            var pwdValidators = new List<PasswordValidator<IdentityMSUser>> {new PasswordValidator<IdentityMSUser>()};

            var userManager = new IdentityMSUserManager(store, options.Object, new PasswordHasher<IdentityMSUser>(),
                userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
                new IdentityMSErrorDescriber(), null,
                new Mock<ILogger<IdentityMSUserManager>>().Object,
                tenantService, null, null,new TwoFactorConfig(36, TimeSpan.FromMinutes(5)));

            validator.Setup(v => v.ValidateAsync(userManager, It.IsAny<IdentityMSUser>()))
                .Returns(Task.FromResult(IdentityResult.Success)).Verifiable();

            return userManager;
        }
        public static TenantService TestTenantService(ITenantRepository tenantRepository = null, 
                                                      IRepository<TenantSqlServerId> tenantSqlServerRepository = null,
                                                      IRepository<IdentityMSUser> userRepository = null,
                                                      IRepository<UserModuleAccess> userModuleAccessRepository = null,
                                                      ITimeService timeService = null,
                                                      IRepository<UserGroupIdentityMSUserRelation> userGroupIdentityMSUserRelationRepository = null,
                                                      IRepository<UserGroupTenantRelation> userGroupTenantRepository = null
            )
        {
            tenantRepository = tenantRepository ?? new Mock<ITenantRepository>().Object;
            userRepository = userRepository ?? new Mock<IRepository<IdentityMSUser>>().Object;
            userModuleAccessRepository = userModuleAccessRepository ?? new Mock<IRepository<UserModuleAccess>>().Object;
            tenantSqlServerRepository = tenantSqlServerRepository ?? new Mock<IRepository<TenantSqlServerId>>().Object;
            timeService = timeService ?? new Mock<ITimeService>().Object;

            userGroupIdentityMSUserRelationRepository = userGroupIdentityMSUserRelationRepository ?? new Mock<IRepository<UserGroupIdentityMSUserRelation>>().Object;

            userGroupTenantRepository = userGroupTenantRepository ?? new Mock<IRepository<UserGroupTenantRelation>>().Object;


            var normalizer = new UpperInvariantLookupNormalizer();
            var unitOfWork = new Mock<IUnitOfWork>().Object;

            return new TenantService(
                tenantRepository, 
                tenantSqlServerRepository,
                userModuleAccessRepository,
                userRepository, 
                normalizer, 
                timeService, 
                unitOfWork,
                new IdentityMSErrorDescriber(),
                userGroupIdentityMSUserRelationRepository,
                userGroupTenantRepository
                );
        }

        public static Mock<IQueryable<T>> BuildQueryable<T>(T entity) where T : class
        {
            return BuildQueryable<T>(new List<T> {entity});
        }

        public static Mock<IQueryable<T>> BuildQueryable<T>(IEnumerable<T> entities) where T : class
        {
            return entities.AsQueryable().BuildMock();
        }
    }
}
