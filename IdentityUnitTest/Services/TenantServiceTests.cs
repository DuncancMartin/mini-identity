using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityCore.Interfaces.Repositories;
using IdentityCore.Interfaces.Services;
using IdentityCore.Models;
using Moq;
using Xunit;

namespace IdentityUnitTest.Services
{
    public class TenantServiceTests
    {
        private static readonly DateTimeOffset Past = new DateTimeOffset(1999, 1, 1, 1, 1, 1, TimeSpan.Zero);
        private static readonly DateTimeOffset Now = new DateTimeOffset(2019, 1, 1, 1, 1, 1, TimeSpan.Zero);
        private static readonly DateTimeOffset Future = new DateTimeOffset(2199, 1, 1, 1, 1, 1, TimeSpan.Zero);

        private class ActiveTenants : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {new Tenant {Id = 1, Active = true, ExpirationDate = Future}};
                yield return new object[] {new Tenant {Id = 2, Active = true, ExpirationDate = null}};
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        private class InactiveTenants : IEnumerable<object[]>
        {
            public IEnumerator<object[]> GetEnumerator()
            {
                yield return new object[] {new Tenant {Id = 1, Active = true, ExpirationDate = Past}};
                yield return new object[] {new Tenant {Id = 2, Active = false, ExpirationDate = null}};
                yield return new object[] {new Tenant {Id = 3, Active = false, ExpirationDate = Past}};
            }

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        [Theory]
        [ClassData(typeof(ActiveTenants))]
        public async Task ActiveTenantFromModel(Tenant tenant)
        {
            var timeService = new Mock<ITimeService>();
            timeService.Setup(i => i.Now()).Returns(Now);

            var tenantService = MockHelper.TestTenantService(null, null, null,null,timeService.Object);

            // Act
            var result = await tenantService.IsActiveTenant(tenant);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [ClassData(typeof(ActiveTenants))]
        public async Task ActiveTenantFromRepository(Tenant tenant)
        {
            // Arrange
            var tenantId = tenant.Id;
            var records = MockHelper.BuildQueryable(tenant);

            var repo = new Mock<ITenantRepository>();
            repo.Setup(i => i.Records()).Returns(records.Object);

            var timeService = new Mock<ITimeService>();
            timeService.Setup(i => i.Now()).Returns(Now);

            var tenantService = MockHelper.TestTenantService(repo.Object, null, null,null,timeService.Object);

            // Act
            var result = await tenantService.IsActiveTenant(tenantId);

            // Assert
            Assert.True(result);
        }

        [Theory]
        [ClassData(typeof(InactiveTenants))]
        public async Task InactiveTenantFromModel(Tenant tenant)
        {
            var timeService = new Mock<ITimeService>();
            timeService.Setup(i => i.Now()).Returns(Now);

            var tenantService = MockHelper.TestTenantService(null, null, null,null,timeService.Object);

            // Act
            var result = await tenantService.IsActiveTenant(tenant);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [ClassData(typeof(InactiveTenants))]
        public async Task InactiveTenantFromRepository(Tenant tenant)
        {
            // Arrange
            var tenantId = tenant.Id;
            var records = MockHelper.BuildQueryable(tenant);

            var repo = new Mock<ITenantRepository>();
            repo.Setup(i => i.Records()).Returns(records.Object);

            var timeService = new Mock<ITimeService>();
            timeService.Setup(i => i.Now()).Returns(Now);

            var tenantService = MockHelper.TestTenantService(repo.Object, null, null,null,timeService.Object);

            // Act
            var result = await tenantService.IsActiveTenant(tenantId);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [ClassData(typeof(ActiveTenants))]
        [ClassData(typeof(InactiveTenants))]
        public async Task ActiveFromModelAndRepoSameResult(Tenant tenant)
        {
            // Arrange
            var tenantId = tenant.Id;
            var records = MockHelper.BuildQueryable(tenant);

            var repo = new Mock<ITenantRepository>();
            repo.Setup(i => i.Records()).Returns(records.Object);

            var timeService = new Mock<ITimeService>();
            timeService.Setup(i => i.Now()).Returns(Now);

            var tenantService = MockHelper.TestTenantService(repo.Object, null, null,null,timeService.Object);

            // Act
            var modelResult = await tenantService.IsActiveTenant(tenant);
            var repoResult = await tenantService.IsActiveTenant(tenantId);

            // Assert
            Assert.Equal(modelResult, repoResult);
        }
    }
}
