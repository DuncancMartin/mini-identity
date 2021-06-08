using System.Collections.Generic;

namespace IdentityCore.Models
{
    public class UserGroup
    {
        public UserGroup()
        {
            UserGroupIdentityMSUserRelations = new List<UserGroupIdentityMSUserRelation>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<UserGroupIdentityMSUserRelation> UserGroupIdentityMSUserRelations { get; set; }
    }
}
