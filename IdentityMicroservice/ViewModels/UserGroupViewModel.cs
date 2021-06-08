using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityApi.ViewModels
{
    public class UserGroupViewModel
    {
        [Required]
        public string Name { get; set; }
    }

    public class UserGroupGetViewModelSummary : UserGroupViewModel
    {
        public int Id { get; set; }
    }

    public class UserGroupGetViewModelDetail : UserGroupViewModel
    {
        public UserGroupGetViewModelDetail()
        {
            UserGroupIdentityMSUserRelations = new List<UserGroupIdentityMSUserRelationViewModelDetail>();
        }

        public int Id { get; set; }
        public IEnumerable<UserGroupIdentityMSUserRelationViewModelDetail> UserGroupIdentityMSUserRelations { get; set; }

    }
    public class UserGroupGetViewModel : UserGroupViewModel
    {
        public UserGroupGetViewModel()
        {
            UserGroupIdentityMSUserRelations = new List<UserGroupIdentityMSUserRelationViewModel>();
        }

        public int Id { get; set; }
        public IEnumerable<UserGroupIdentityMSUserRelationViewModel> UserGroupIdentityMSUserRelations { get; set; }
    }

    public class UserGroupPostViewModel : UserGroupViewModel
    {
    }

    public class UserGroupPutViewModel : UserGroupViewModel
    {
        [Required]
        public int Id { get; set; }
    }
}
