using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace IdentityCore.Models
{
    public class IdentityMSRole : IdentityRole<int>
    {
        public IdentityMSRole() { }

        public IdentityMSRole(int moduleId, string permissionName, char separator = '_')
            : base($"{ moduleId.ToString()}{separator}{permissionName}")
        {
            PermissionName = permissionName;
            ModuleId = moduleId;
        }

        public int PermissionValue { get; set; }
        public string PermissionName { get; set; }
        public string NormalizedPermissionName { get; set; }
        public string Description { get; set; }
        public int ModuleId { get; set; }
      
    }
}
