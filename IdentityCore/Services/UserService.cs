using IdentityCore.Interfaces.Repositories;
using IdentityCore.Interfaces.Services;
using IdentityCore.Interfaces.UnitOfWork;
using IdentityCore.Models;
using IdentityCore.Models.ServiceResponses;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IdentityCore.Services
{
    public class UserService : IUserService
    {
        private readonly IRepository<IdentityMSUser> _userRepository;
        private readonly IdentityMSUserManager _userManager;
        private readonly IdentityMSErrorDescriber _errorDescriber;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserGroupRepository _userGroupRepository;
        private readonly ILookupNormalizer _normalizer;

        public UserService(IRepository<IdentityMSUser> userRepository,
                           IdentityMSUserManager userManager,
                           IdentityErrorDescriber errorDescriber,
                           IUnitOfWork unitOfWork,
                           IUserGroupRepository userGroupRepository,
                           ILookupNormalizer normalizer)
        {
            _userRepository = userRepository;
            _userManager = userManager;
            _errorDescriber = errorDescriber as IdentityMSErrorDescriber;
            _unitOfWork = unitOfWork;
            _userGroupRepository = userGroupRepository;
            _normalizer = normalizer;
        }

        public async Task<UserCreationResponse> CreateUser(IdentityMSUser user, string password)
        {
            var result = await _userManager.CreateAsync(user, password);

            if (!result.Succeeded)
            {
                return new UserCreationResponse
                {
                    Errors = result.Errors
                };
            }

            await AddUserToTenantUserGroups(user);
            await _unitOfWork.SaveChangesAsync();

            return new UserCreationResponse
            {
                User = user
            };
        }

        private async Task AddUserToTenantUserGroups(IdentityMSUser user)
        {
            // A user should inherit the user groups of its tenant
            var userGroupIds = await GetUserGroupIdsOfTenant(user.TenantId);
            foreach (var userGroupId in userGroupIds)
            {
                _userGroupRepository.AddUserToUserGroup(userGroupId, user.Id);
            }
        }

        private Task<List<int>> GetUserGroupIdsOfTenant(int tenantId)
        {
            return _userGroupRepository.Records()
                .Select(x => x.Id)
                .ToListAsync();
        }

        public Task<IdentityMSUser> GetUser(int id)
        {
            return _userRepository.Records()
                .Include(u => u.UserGroupIdentityMSUserRelations)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public Task<IdentityMSUser> GetUserByUsername(string username)
        {
            return _userManager.FindByNameAsync(username);
        }
        public async Task<IEnumerable<IdentityMSUser>> GetAllUsers()
        {
            return await _userRepository.Records()
                .Include(u => u.UserGroupIdentityMSUserRelations)
                .ToListAsync();
        }

        public async Task DeleteUser(IdentityMSUser user)
        {
            await _userManager.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<IdentityResult> ForgottenPasswordRequest(string username)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return IdentityResult.Failed(_errorDescriber.UnknownUserName(username));

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);

            //var result = await _emailService.ForgottenPasswordEmail(user, token);

            //if (!result.Succeeded)
            //    return IdentityResult.Failed(_errorDescriber.EmailSendingFailed());

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> ForgottenPasswordComplete(string username, string token, string newPassword)
        {
            var user = await _userManager.FindByNameAsync(username);

            if (user == null)
                return IdentityResult.Failed(_errorDescriber.UnknownUserName(username));

            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            if (result.Succeeded)
            {
                await _unitOfWork.SaveChangesAsync();
                //await _emailService.PasswordResetNotification(user.Email);
            }

            return result;
        }

        public async Task<IdentityResponse> ChangePassword(string username, string currentPassword, string newPassword, bool forceChangePassword)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return new IdentityResponse(_errorDescriber.PasswordMismatch());
            
            user.PasswordConfirmed = !forceChangePassword;
            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (!result.Succeeded)
                return new IdentityResponse(result.Errors);

            await _unitOfWork.SaveChangesAsync();

            //await _emailService.PasswordResetNotification(user.Email);
            return new IdentityResponse();
        }

        public async Task<IdentityResponse> PasswordResetByAdministrator(string username,  string newPassword, bool forceChangePassword)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return new IdentityResponse(_errorDescriber.InvalidUserName(username));

            var result = await _userManager.UpdatePasswordAsync(user, newPassword);

            if (!result.Succeeded)
                return new IdentityResponse(result.Errors);

            user.PasswordConfirmed = !forceChangePassword;
            await _unitOfWork.SaveChangesAsync();

            //await _emailService.PasswordResetNotification(user.Email);
            return new IdentityResponse();
        }

        public async Task<IPagedResults<IdentityMSUser>> SearchUsersPaged(int tenantId, int page, int pageSize, string searchTerm)
        {
            var users = _userRepository.Records().Where(u => u.TenantId == tenantId);

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = PrepareSearchTerm(searchTerm);
                users = users.Where(u =>
                    u.FirstName.Contains(searchTerm) || u.LastName.Contains(searchTerm) ||
                    u.UserName.Contains(searchTerm) || u.Email.Contains(searchTerm));
            }

            var totalResultsCount = users.Count();

            if (page > 1)
            {
                users = users.Skip((page - 1) * pageSize).OrderBy(x => x.Id);
            }

            var userResults = await users.Take(pageSize).ToListAsync();

            return new PagedResults<IdentityMSUser>(page, pageSize, totalResultsCount, userResults);
        }

        public async Task<IPagedResults<IdentityMSUser>> SearchUsersPagedAllTenantsByUsername(int page, int pageSize, string searchTerm)
        {
            searchTerm = PrepareSearchTerm(searchTerm);

            var users = _userRepository.Records().Where(u => u.UserName.Contains(searchTerm));

            var totalResultsCount = users.Count();

            if (page > 1)
            {
                users = users.Skip((page - 1) * pageSize).OrderBy(x => x.Id);
            }

            var userResults = await users.Take(pageSize).ToListAsync();

            return new PagedResults<IdentityMSUser>(page, pageSize, totalResultsCount, userResults);
        }

        //public async Task<IEnumerable<ModuleEntityAccessRelations<int, int>>> GetUserModuleAccessRelations(int userId)
        //{
        //    var user = await GetUser(userId);

        //    if (user == default(IdentityMSUser)) throw new UserNotFoundException(userId, "User was not found");

        //    var userModules = await _moduleService.GetUserModules(user);

        //    var relations = await _moduleService.GetUserModuleRelations(user, userModules);

        //    return relations.Select(r => new ModuleEntityAccessRelations<int, int> { Module = r.Key.Id, Entities = r.Value });
        //}

        //public async Task<UsersConfigurations> SaveUserConfigurations(UsersConfigurations usersConfigurations)
        //{
        //    await CreateUsersFromConfig(usersConfigurations.Created);
        //    await UpdateUsersFromConfig(usersConfigurations.Changed);

        //    // Don't try to save in case of user validation errors
        //    if (!usersConfigurations.Succeeded)
        //        return usersConfigurations;

        //    await DeleteUsersFromConfig(usersConfigurations.Deleted);
        //    await _unitOfWork.SaveChangesAsync();

        //    return usersConfigurations;
        //}

        public async Task<ChangedUserConfiguration> UpdateUser(ChangedUserConfiguration changedUserConfiguration)
        {
            await UpdateUserFromConfig(changedUserConfiguration);

            // Don't try to save in case of user validation errors
            if (!changedUserConfiguration.Succeeded)
                return changedUserConfiguration;

            await _unitOfWork.SaveChangesAsync();

            return changedUserConfiguration;
        }

        public async Task<IdentityResult> UpdateIsAdministrator(int userId, bool isAdministrator)
        {
            var currentUser = await _userManager.FindByIdAsync(userId, includeModuleAccess: false);
            if (currentUser == default)
                return IdentityResult.Failed(_errorDescriber.UnknownUserId(userId));
            currentUser.IsAdministrator = isAdministrator;
            await _unitOfWork.SaveChangesAsync();
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteUserForInactiveTenant(int tenantId)
        {
            //if (await _tenantService.IsActiveTenant(tenantId))
            //    return IdentityResult.Failed(_errorDescriber.ActiveTenant());

            //Only want one user. If there are more then one, I do not want to delete any
            var users = await _userRepository.Records().Where(x => x.TenantId == tenantId).ToListAsync();
            if (users.Count == 0)
                return IdentityResult.Success;
            if (users.Count == 1)
            {
                await DeleteUser(users[0]);
                return IdentityResult.Success;
            }
            else
                return IdentityResult.Failed(_errorDescriber.MultipleUsersExist());
        }

        private async Task CreateUsersFromConfig(IEnumerable<UserConfiguration> usersToCreate)
        {
            foreach (var userConfiguration in usersToCreate)
            {
                var user = userConfiguration.User;
                //user.UserModuleAccess = userConfiguration.Modules
                //    .Select(i => new UserModuleAccess(0, i.ModuleId, i.Entities))
                //    .ToList();

                var result = await _userManager.CreateAsync(userConfiguration.User, userConfiguration.Password);

                if (result.Succeeded)
                {
                    //From EF Core3.0 on we have to save the changes to the item so we have the right ID before adding the group
                    await _unitOfWork.SaveChangesAsync();
                    await AddUserToTenantUserGroups(user);
                    //await _emailService.WelcomeNewUserEmail(userConfiguration);
                }
                else
                    userConfiguration.Errors = result.Errors;
            }
        }

        private async Task UpdateUsersFromConfig(IEnumerable<ChangedUserConfiguration> usersToChange)
        {
            foreach (var userConfiguration in usersToChange)
            {
                // Includes module and entity access if module access was changed
                var currentUser = await _userManager.FindByIdAsync(userConfiguration.User.Id, includeModuleAccess: userConfiguration.ModuleAccessChanged);

                await UpdateUserDetails(userConfiguration, currentUser);

                //if (userConfiguration.ModuleAccessChanged)
                //{
                //    var userId = currentUser.Id;
                //    var userModuleAccesses = userConfiguration.Modules
                //        .Select(i => new UserModuleAccess(userId, i.ModuleId, i.Entities))
                //        .ToList();

                //    var result = await _userManager.SetUserModuleAndEntityAccessAsync(currentUser, userModuleAccesses);
                //    if (!result.Succeeded)
                //    {
                //        userConfiguration.Errors = result.Errors;
                //        break;
                //    }
                //}
            }
        }

        private async Task UpdateUserFromConfig(ChangedUserConfiguration userToChange)
        {
            //Don't include module and entity access.
            var currentUser = await _userManager.FindByIdAsync(userToChange.User.Id);

            await UpdateUserDetails(userToChange, currentUser);

        }

        private async Task UpdateUserDetails(ChangedUserConfiguration changedUser, IdentityMSUser currentUserData)
        {
            if (changedUser.PasswordChanged)
            {
                var result = await _userManager.UpdatePasswordAsync(currentUserData, changedUser.Password);
                if (!result.Succeeded)
                {
                    changedUser.Errors = result.Errors;
                    return;
                }
            }

            if (changedUser.UserDataChanged)
            {
                currentUserData.UserName = changedUser.User.UserName;
                currentUserData.Email = changedUser.User.Email;
                currentUserData.FirstName = changedUser.User.FirstName;
                currentUserData.LastName = changedUser.User.LastName;
                currentUserData.LocalizationId = changedUser.User.LocalizationId;
                currentUserData.Active = changedUser.User.Active;
                currentUserData.TenantAdministrator = changedUser.User.TenantAdministrator;
                var result = await _userManager.UpdateAsync(currentUserData);
                if (!result.Succeeded)
                {
                    changedUser.Errors = result.Errors;
                    return;
                }
            }
        }

        private async Task DeleteUsersFromConfig(IEnumerable<int> userIdsToDelete)
        {
            foreach (var userId in userIdsToDelete)
            {
                var user = await _userManager.FindByIdAsync(userId);
                await _userManager.DeleteAsync(user);
            }
        }

        private static string PrepareSearchTerm(string searchTerm)
        {
            searchTerm = searchTerm.Trim();
            searchTerm = Regex.Replace(searchTerm, @"\s+", " ");    // Replaces multiple space chars to one: "  caroline  harder" => "caroline harder"
            return searchTerm;
        }

        //private static List<ModuleEntityAccessRelations<int, int>> BuildModuleEntityRelations(IEnumerable<ModuleAccessModel> modules)
        //{
        //    return modules.Select(m => new ModuleEntityAccessRelations<int, int> { Module = m.ModuleId, Entities = m.Entities }).ToList();
        //}

        public Task<bool> CheckIfEmailExists(string email)
        {
            var normalizedEmail = _normalizer.NormalizeEmail(email);

            return _userRepository.Records()
                .AsNoTracking()
                .AnyAsync(i => i.NormalizedEmail == normalizedEmail);
        }

        public Task<bool> CheckIfEmailExistsForOtherUser(string email, int currentUserId)
        {
            var normalizedEmail = _normalizer.NormalizeEmail(email);

            return _userRepository.Records()
                .AsNoTracking()
                .AnyAsync(i => i.NormalizedEmail == normalizedEmail && i.Id != currentUserId);
        }

        public Task<bool> CheckIfUserNameExists(string userName)
        {
            var normalizedUserName = _normalizer.NormalizeName(userName);

            return _userRepository.Records()
                .AsNoTracking()
                .AnyAsync(i => i.NormalizedUserName == normalizedUserName);
        }

        public Task<bool> CheckIfUserNameExistsForOtherUser(string userName, int currentUserId)
        {
            var normalizedUserName = _normalizer.NormalizeName(userName);

            return _userRepository.Records()
                .AsNoTracking()
                .AnyAsync(i => i.NormalizedUserName == normalizedUserName && i.Id != currentUserId);
        }
    }
}
