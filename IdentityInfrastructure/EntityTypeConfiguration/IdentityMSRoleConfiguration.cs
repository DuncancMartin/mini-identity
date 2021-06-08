using IdentityCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityInfrastructure.EntityTypeConfiguration
{
    class IdentityMSRoleConfiguration : IEntityTypeConfiguration<IdentityMSRole>
    {
        // Configures extended properties of Roles, base properties are set by IdentityDbContext
        public void Configure(EntityTypeBuilder<IdentityMSRole> builder)
        {
            builder.HasIndex(i => new { i.ModuleId, i.NormalizedPermissionName })
                .IsUnique();
             
        }
    }
}
