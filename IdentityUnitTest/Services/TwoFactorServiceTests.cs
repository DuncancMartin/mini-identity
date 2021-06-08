using IdentityCore;
using IdentityCore.Interfaces.Services;
using IdentityCore.Interfaces.UnitOfWork;
using IdentityCore.Models;
using IdentityCore.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace IdentityUnitTest
{
    public class TwoFactorServiceTests
    {
        private const string _userName = "Test";
        private const string _passwordToken = "TestPasswordToken";
        private const string _goodSixDigitCode = "123456";
        private const string _badSixDigitCode = "876543";


        [Fact]
        public async Task GenerateNewAuthenticatorKey_Working()
        {
            // Arrange
            //Create User and UserManager
            var userManager = SetupUserManager();

            //create 2fa service
            var twoFactorService = new TwoFactorService(userManager.Object, new IdentityMSErrorDescriber(), new Mock<IUnitOfWork>().Object);

            // Act
            var result = await twoFactorService.GenerateNewAuthenticatorKey(_userName);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.True(result.AuthenticatorKeyValue== _passwordToken);
        }

        [Fact]
        public async Task EnableTwoFactor_Working()
        {
            // Arrange
            //Create User and UserManager
            var userManager = SetupUserManager();

            //create 2fa service
            var twoFactorService = new TwoFactorService(userManager.Object, new IdentityMSErrorDescriber(), new Mock<IUnitOfWork>().Object);

            // Act
            var result = await twoFactorService.EnableTwoFactor(_userName, _goodSixDigitCode);

            // Assert
            Assert.True(result.Succeeded);
        }
        [Fact]
        public async Task EnableTwoFactor_BadCode()
        {
            // Arrange
            //Create User and UserManager
            var userManager = SetupUserManager();

            //create 2fa service
            var twoFactorService = new TwoFactorService(userManager.Object, new IdentityMSErrorDescriber(), new Mock<IUnitOfWork>().Object);

            // Act
            var result = await twoFactorService.EnableTwoFactor(_userName, _badSixDigitCode);

            // Assert
            Assert.False(result.Succeeded);
        }
        [Fact]
        public async Task DisableTwoFactor_Working()
        {
            // Arrange
            //Create User and UserManager
            var userManager = SetupUserManager();

            //create 2fa service
            var twoFactorService = new TwoFactorService(userManager.Object, new IdentityMSErrorDescriber(), new Mock<IUnitOfWork>().Object);

            // Act
            var result = await twoFactorService.DisableTwoFactor(_userName);

            // Assert
            Assert.True(result.Succeeded);
        }

        private Mock<IdentityMSUserManager> SetupUserManager()
        {
            var user = new IdentityMSUser { UserName = _userName, TenantId = 1 };
            var store = new Mock<IUserStore<IdentityMSUser>>().Object;
            var tenantService = new Mock<ITenantService>().Object;

            var options = new Mock<IOptions<IdentityOptions>>();
            var idOptions = new IdentityOptions { Lockout = { AllowedForNewUsers = false } };
            options.Setup(o => o.Value).Returns(idOptions);

            var userValidators = new List<IUserValidator<IdentityMSUser>>();
            var validator = new Mock<IUserValidator<IdentityMSUser>>();
            userValidators.Add(validator.Object);
            userValidators.Add(new UserTenantValidator());

            var pwdValidators = new List<PasswordValidator<IdentityMSUser>> { new PasswordValidator<IdentityMSUser>() };

            var userManager = new Mock<IdentityMSUserManager>(store, options.Object, new PasswordHasher<IdentityMSUser>(),
                userValidators, pwdValidators, new UpperInvariantLookupNormalizer(),
                new IdentityMSErrorDescriber(), null,
                new Mock<ILogger<IdentityMSUserManager>>().Object,
                tenantService, null, null, new TwoFactorConfig(36, TimeSpan.FromMinutes(5)));


            //Mock properly to avoid errors. If we want to cause named errors, include/exclude these.
            userManager.Setup(i => i.FindByNameAsync(_userName)).Returns(Task.FromResult(user)).Verifiable();
            userManager.Setup(i => i.GetTwoFactorEnabledAsync(user)).Returns(Task.FromResult(false)).Verifiable();
            userManager.Setup(i => i.ResetAuthenticatorKeyAsync(user)).Returns(Task.FromResult(IdentityResult.Success)).Verifiable();
            userManager.Setup(i => i.GetAuthenticatorKeyAsync(user)).Returns(Task.FromResult(_passwordToken)).Verifiable();
            
            userManager.Setup(i => i.VerifyTwoFactorTokenAsync(user,"Authenticator" , _goodSixDigitCode)).Returns(Task.FromResult(true)).Verifiable();

            return userManager;
        }
    }
}
