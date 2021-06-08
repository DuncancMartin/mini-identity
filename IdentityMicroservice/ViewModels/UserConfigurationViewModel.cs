using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace IdentityApi.ViewModels
{
    /// <summary>
    /// The absolute state of a <see cref="PortalUserConfigurationView"/> and his/her module access.
    /// </summary>
    public class UserConfigurationPostView
    {
        public UserConfigurationPostView()
        {
        }

        public PortalUserConfigurationView User { get; set; }
        public string ExternalUserId { get; set; }
        public string Password { get; set; }
    }

    public class UserConfigurationGetView
    {
        public UserConfigurationGetView()
        {
        }

        public PortalUserConfigurationView User { get; set; }
        public string ExternalUserId { get; set; }
        public string Password { get; set; }

        public IEnumerable<IdentityError> Errors { get; set; }
        public bool Succeeded { get; set; }
    }

    public class ChangedUserConfigurationPostView : UserConfigurationPostView
    {
        public bool PasswordChanged { get; set; }
        public bool UserDataChanged { get; set; }
        public bool ModuleAccessChanged { get; set; }
    }

    public class ChangedUserConfigurationGetView : UserConfigurationGetView
    {
        public bool PasswordChanged { get; set; }
        public bool UserDataChanged { get; set; }
        public bool ModuleAccessChanged { get; set; }
    }
}