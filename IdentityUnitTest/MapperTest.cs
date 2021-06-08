using AutoMapper;
using IdentityApi.StartupConfig;
using Xunit;

namespace IdentityUnitTest
{
    public class MapperTest
    {
        [Fact]
        public void AssertMapperConfig()
        {
            var mapper = new Mapper(new MapperConfiguration(cfg => cfg.AddProfile(new MapperConfig())));
            var ex = Record.Exception(() => mapper.ConfigurationProvider.AssertConfigurationIsValid());
            Assert.Null(ex);
        }
    }
}
