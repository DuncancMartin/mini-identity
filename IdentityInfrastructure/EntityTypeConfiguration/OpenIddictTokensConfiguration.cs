using IdentityCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityInfrastructure.EntityTypeConfiguration
{
    class OpenIddictTokensConfiguration : IEntityTypeConfiguration<ExtendedOpenIddictEntityFrameworkCoreToken>
    {
        public void Configure(EntityTypeBuilder<ExtendedOpenIddictEntityFrameworkCoreToken> builder)
        {
            builder.ToTable("OpenIddictTokens2");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).ValueGeneratedOnAdd();

            builder.Property(i => i.TenantId).HasDefaultValue(false);

            //TODO consider adding PATName and Description
        }
    }
}
