using IdentityCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IdentityInfrastructure.EntityTypeConfiguration
{
    class UserGroupIdentityMSUserRelationConfiguration : IEntityTypeConfiguration<UserGroupIdentityMSUserRelation>
    {
        public void Configure(EntityTypeBuilder<UserGroupIdentityMSUserRelation> builder)
        {
            builder.ToTable("UserGroupIdentityUserRelations");
            builder.HasKey(i => new { i.IdentityMSUserId, i.UserGroupId });

            builder.HasOne(i => i.IdentityMsUser)
                .WithMany(i => i.UserGroupIdentityMSUserRelations)
                .HasForeignKey(i => i.IdentityMSUserId);

            builder.HasOne(i => i.UserGroup)
                .WithMany(i => i.UserGroupIdentityMSUserRelations)
                .HasForeignKey(i => i.UserGroupId);
        }
    }
}
