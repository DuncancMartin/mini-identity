using IdentityCore.Interfaces.Repositories;
using IdentityCore.Interfaces.Services;
using IdentityCore.Interfaces.UnitOfWork;
using IdentityCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityCore.Services
{
    public class UserGroupService : IUserGroupService
    {
        private readonly IUserGroupRepository _userGroupRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRepository<UserGroupIdentityMSUserRelation> _userGroupIdentityMSUserRelation;

        public UserGroupService(IUserGroupRepository userGroupRepository, IUnitOfWork unitOfWork
            ,  IRepository<UserGroupIdentityMSUserRelation> userGroupIdentityMSUserRelation)
        {
            _userGroupRepository = userGroupRepository;
            _unitOfWork = unitOfWork;
            _userGroupIdentityMSUserRelation = userGroupIdentityMSUserRelation;
        }

        public async Task<IEnumerable<UserGroup>> GetAll()
        {
            return await _userGroupRepository
                .Records()
                .Include(u => u.UserGroupIdentityMSUserRelations)
                .ToListAsync();
        }
        public async Task<IEnumerable<UserGroup>> GetSummary()
        {
            return await _userGroupRepository
                .Records()
                .ToListAsync();
        }
        public Task<UserGroup> GetById(int id)
        {
            return _userGroupRepository
                .Records()
                .Where(i => i.Id == id)
                .Include(u => u.UserGroupIdentityMSUserRelations)
                .ThenInclude( u => u.IdentityMsUser)
                .FirstOrDefaultAsync();
        }

        public async Task<UserGroup> Post(UserGroup userGroup)
        {
            _userGroupRepository.Add(userGroup);
            await _unitOfWork.SaveChangesAsync();
            return userGroup;
        }

        public async Task Put(UserGroup userGroup)
        {
            _userGroupRepository.Update(userGroup);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var userGroup = await _userGroupRepository
                .Records()
                .Where(i => i.Id == id)
                .FirstOrDefaultAsync();

            if (userGroup != default(UserGroup))
            {
                _userGroupRepository.Delete(userGroup);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task AddUserToUserGroup(int userGroupId, int userId)
        {
            _userGroupRepository.AddUserToUserGroup(userGroupId, userId);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task RemoveUserFromUserGroup(int userGroupId, int userId)
        {
            _userGroupRepository.RemoveUserFromUserGroup(userGroupId, userId);
            await _unitOfWork.SaveChangesAsync();
        }


        public async Task<UserGroup> GetByName(string name)
        {
            var userGroup = await _userGroupRepository.Records()
                 .Include(u => u.UserGroupIdentityMSUserRelations)
                 .ThenInclude(u => u.IdentityMsUser)
                 .FirstOrDefaultAsync(u => u.Name == name);
            if (userGroup == null)
                throw new ArgumentException($"No UserGroup found with name: {name}");
            return userGroup;
        }

        public  Task<bool> IsTenantInUsersUserGroups( int tenantId, int userId)
        {
            return  _userGroupIdentityMSUserRelation.Records()
                .AnyAsync();
        }
    }
}
