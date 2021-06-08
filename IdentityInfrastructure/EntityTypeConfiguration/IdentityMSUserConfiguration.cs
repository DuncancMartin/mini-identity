using IdentityCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityInfrastructure.EntityTypeConfiguration
{
    class IdentityMSUserConfiguration : IEntityTypeConfiguration<IdentityMSUser>
    {
        public void Configure(EntityTypeBuilder<IdentityMSUser> builder)
        {
            builder.ToTable("IdentityUsers");
            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).ValueGeneratedOnAdd();

            builder.Property(i => i.FirstName).HasMaxLength(100);
            builder.Property(i => i.LastName).HasMaxLength(100);

            builder.Property(i => i.PasswordConfirmed)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(i => i.IsAdministrator)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(i => i.Active)
                .HasDefaultValue(true)
                .IsRequired();
            builder.Property(i => i.TenantAdministrator)
                .HasDefaultValue(false)
                .IsRequired();

            builder.Property(i => i.PasswordToken)
                .HasMaxLength(252);
        }
    }
}
