using IdentityCore.Interfaces.Repositories;
using IdentityCore.Models;
using IdentityCore.Models.ServiceResponses;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace IdentityCore.Interfaces.Services
{
    public interface IUserService
    {
        Task<UserCreationResponse> CreateUser(IdentityMSUser user, string password);
        Task<IdentityMSUser> GetUser(int id);
        Task<IdentityMSUser> GetUserByUsername(string username);
        Task<IEnumerable<IdentityMSUser>> GetAllUsers();
        Task DeleteUser(IdentityMSUser user);
        Task<IdentityResult> ForgottenPasswordRequest(string username);
        Task<IdentityResult> ForgottenPasswordComplete(string username, string token, string newPassword);
        Task<IdentityResponse> ChangePassword(string username, string currentPassword, string newPassword, bool forcePasswordChange);
        Task<IPagedResults<IdentityMSUser>> SearchUsersPaged(int tenantId, int page, int pageSize, string searchTerm);
        Task<IPagedResults<IdentityMSUser>> SearchUsersPagedAllTenantsByUsername(int page, int pageSize, string searchTerm);
        //Task<IEnumerable<ModuleEntityAccessRelations<int, int>>> GetUserModuleAccessRelations(int userId);
        //Task<UsersConfigurations> SaveUserConfigurations(UsersConfigurations usersModel);
        Task<ChangedUserConfiguration> UpdateUser(ChangedUserConfiguration changedUserConfiguration);
        Task<bool> CheckIfEmailExists(string email);
        Task<bool> CheckIfEmailExistsForOtherUser(string email, int currentUser);
        Task<bool> CheckIfUserNameExists(string userName);
        Task<bool> CheckIfUserNameExistsForOtherUser(string userName, int currentUser);
        Task<IdentityResult> DeleteUserForInactiveTenant(int tenantId);
        Task<IdentityResult> UpdateIsAdministrator(int userId, bool isAdministrator);
        Task<IdentityResponse> PasswordResetByAdministrator(string username, string newPassword, bool forceChangePassword);
    }
}