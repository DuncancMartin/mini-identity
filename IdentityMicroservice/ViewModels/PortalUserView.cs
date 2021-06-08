using IdentityCore.Models;
using System;
using System.Collections.Generic;

namespace IdentityApi.ViewModels
{
    public abstract class PortalUserViewModel
    {
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int TenantId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int? LocalizationId { get; set; }
        public bool IsAdministrator { get; set; }
        public bool Active { get; set; }
        public bool TenantAdministrator { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTimeOffset? LastAccess { get; set; }
    }

    public class PortalUserPostView : PortalUserViewModel
    {
    }

    public class PortalUserGetView : PortalUserViewModel
    {
        public int Id { get; set; }

        public PortalUserGetView()
        {
            UserGroupIdentityMSUserRelations = new List<UserGroupIdentityMSUserRelationViewModel>();
        }

        public IEnumerable<UserGroupIdentityMSUserRelationViewModel> UserGroupIdentityMSUserRelations { get; set; }
    }

    public class PortalUserConfigurationView : PortalUserViewModel
    {
        public int Id { get; set; }

        public PortalUserConfigurationView()
        {
            UserGroupIdentityMSUserRelations = new List<UserGroupIdentityMSUserRelationViewModel>();
        }

        public IEnumerable<UserGroupIdentityMSUserRelationViewModel> UserGroupIdentityMSUserRelations { get; set; }
    }

    public class BasicUserGetView
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }
}