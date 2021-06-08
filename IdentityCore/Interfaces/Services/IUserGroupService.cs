using IdentityCore.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IdentityCore.Interfaces.Services
{
    public interface IUserGroupService
    {
        Task<IEnumerable<UserGroup>> GetSummary();
        Task<IEnumerable<UserGroup>> GetAll();
        Task<UserGroup> GetById(int id);
        Task<UserGroup> Post(UserGroup userGroup);
        Task Put(UserGroup userGroup);
        Task Delete(int id);
        Task AddUserToUserGroup(int userGroupId, int userId);
        Task RemoveUserFromUserGroup(int userGroupId, int userId);
        Task<UserGroup> GetByName(string name);
        Task<bool> IsTenantInUsersUserGroups(int tenantId, int userId);
    }
}
