using IdentityCore.Interfaces.Repositories;
using IdentityCore.Interfaces.UnitOfWork;
using IdentityCore.Models;

namespace IdentityInfrastructure.Repositories
{
    public class UserGroupRepository : CrudRepository<UserGroup>, IUserGroupRepository
    {
        private readonly IDataContextFactory _contextFactory;
        private IDataContext Context => _contextFactory.GetDataContext();

        public UserGroupRepository(IDataContextFactory contextFactory)
            : base(contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public void AddUserToUserGroup(int userGroupId, int userId)
        {
            var entity = new UserGroupIdentityMSUserRelation
            {
                UserGroupId = userGroupId,
                IdentityMSUserId = userId
            };

            Context.Set<UserGroupIdentityMSUserRelation>().Add(entity);
        }

        public void RemoveUserFromUserGroup(int userGroupId, int userId)
        {
            var entity = new UserGroupIdentityMSUserRelation
            {
                UserGroupId = userGroupId,
                IdentityMSUserId = userId
            };

            Context.Set<UserGroupIdentityMSUserRelation>().Remove(entity);
        }

   
    }
}
