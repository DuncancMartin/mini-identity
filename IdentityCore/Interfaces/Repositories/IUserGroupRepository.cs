using IdentityCore.Models;

namespace IdentityCore.Interfaces.Repositories
{
    public interface IUserGroupRepository : IRepository<UserGroup>
    {
        void AddUserToUserGroup(int userGroupId, int userId);
        void RemoveUserFromUserGroup(int userGroupId, int userId);
    }
}
