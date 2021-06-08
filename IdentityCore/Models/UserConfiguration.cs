using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace IdentityCore.Models
{
    /// <summary>
    /// The absolute state of a <see cref="IdentityMSUser"/> and his/her module access.
    /// </summary>
    public class UserConfiguration
    {
        public UserConfiguration()
        {
            Errors = null;
        }

        public IdentityMSUser User { get; set; }
        public string ExternalUserId { get; set; }
        public string Password { get; set; }

        public IEnumerable<IdentityError> Errors { get; set; }
        public bool Succeeded => Errors == null;
    }

    public class ChangedUserConfiguration : UserConfiguration
    {
        public bool PasswordChanged { get; set; }
        public bool UserDataChanged { get; set; }
        public bool ModuleAccessChanged { get; set; }
    }
}