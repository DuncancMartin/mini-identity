namespace IdentityApi.ViewModels
{
    public class UserGroupIdentityMSUserRelationViewModel
    {
        public int IdentityMSUserId { get; set; }
        public int UserGroupId { get; set; }
    }

    public class UserGroupIdentityMSUserRelationViewModelDetail : UserGroupIdentityMSUserRelationViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
