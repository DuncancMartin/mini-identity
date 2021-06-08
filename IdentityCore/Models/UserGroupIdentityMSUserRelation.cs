using System.ComponentModel.DataAnnotations.Schema;

namespace IdentityCore.Models
{
    public class UserGroupIdentityMSUserRelation
    {
        public int IdentityMSUserId { get; set; }
        public int UserGroupId { get; set; }

        [NotMapped]
        public IdentityMSUser IdentityMsUser { get; set; }
        [NotMapped]
        public UserGroup UserGroup { get; set; }
    }
}
