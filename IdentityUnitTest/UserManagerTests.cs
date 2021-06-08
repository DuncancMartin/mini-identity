using System.Threading;
using System.Threading.Tasks;
using IdentityCore.Interfaces.Services;
using IdentityCore.Models;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace IdentityUnitTest
{
    public class UserManagerTests
    {
        [Fact]
        public async Task CreateUserHitsUserStoreAndTenantService()
        {
            // Arrange
            var user = new IdentityMSUser { UserName = "Test", TenantId = 1 };
            var store = new Mock<IUserStore<IdentityMSUser>>();
            store.Setup(i => i.CreateAsync(user, CancellationToken.None))
                .ReturnsAsync(IdentityResult.Success)
                .Verifiable();

            store.Setup(i => i.GetUserNameAsync(user, CancellationToken.None))
                .Returns(Task.FromResult(user.UserName))
                .Verifiable();

            store.Setup(i => i.SetNormalizedUserNameAsync(user, user.UserName.ToUpperInvariant(), CancellationToken.None))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var tenantService = new Mock<ITenantService>();
            tenantService.Setup(i => i.IsActiveTenant(user.TenantId)).Returns(Task.FromResult(true)).Verifiable();

            var userManager = MockHelper.TestUserManager(store.Object, tenantService.Object);

            // Act
            var result = await userManager.CreateAsync(user);

            // Assert
            Assert.True(result.Succeeded);
            store.VerifyAll();
            tenantService.VerifyAll();
        }

        [Fact]
        public async Task CreateUserFailsOnInvalidTenant()
        {
            // Arrange
            var user = new IdentityMSUser { UserName = "Test", TenantId = 1 };

            var tenantService = new Mock<ITenantService>();
            tenantService.Setup(i => i.IsActiveTenant(user.TenantId)).Returns(Task.FromResult(false));

            var userManager = MockHelper.TestUserManager(null, tenantService.Object);

            // Act
            var result = await userManager.CreateAsync(user);

            // Assert
            Assert.False(result.Succeeded);
        }

        [Fact]
        public async Task ActiveTenantByModel()
        {
            // Arrange
            var user = new IdentityMSUser {TenantId = 1, Tenant = new Tenant {Id = 1}};

            var tenantService = new Mock<ITenantService>();
            tenantService.Setup(i => i.IsActiveTenant(user.Tenant)).Returns(Task.FromResult(true)).Verifiable();

            var userManager = MockHelper.TestUserManager(null, tenantService.Object);

            // Act
            var result = await userManager.TenantIsActiveAsync(user);

            // Assert
            Assert.True(result);
            tenantService.VerifyAll();
        }

        [Fact]
        public async Task ActiveTenantById()
        {
            // Arrange
            var user = new IdentityMSUser {TenantId = 1};

            var tenantService = new Mock<ITenantService>();
            tenantService.Setup(i => i.IsActiveTenant(user.TenantId)).Returns(Task.FromResult(true)).Verifiable();

            var userManager = MockHelper.TestUserManager(null, tenantService.Object);

            // Act
            var result = await userManager.TenantIsActiveAsync(user);

            // Assert
            Assert.True(result);
            tenantService.VerifyAll();
        }

        [Fact]
        public async Task UpdatePasswordBySupport()
        {
            // Arrange
            var user = new IdentityMSUser {TenantId = 1, PasswordConfirmed = true};
            const string password = "Password1!";

            var store = new Mock<IUserPasswordStore<IdentityMSUser>>();
            store.Setup(i => i.SetPasswordHashAsync(It.IsAny<IdentityMSUser>(), It.IsAny<string>(), CancellationToken.None))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var userManager = MockHelper.TestUserManager(store.Object);

            // Act
            var result = await userManager.UpdatePasswordAsync(user, password);

            // Assert
            Assert.True(result.Succeeded);
            // TODO: reenable once UI is ready
            //Assert.False(user.PasswordConfirmed);
            store.VerifyAll();
        }
    }
}
