using System;

namespace IdentityCore.Models
{
    public class PersonalAccessToken
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int TenantId { get; set; }
        public int UserId { get; set; }
        public string TokenId { get; set; }
        public DateTime TokenExpiry { get; set; }
    }
    public class PersonalAccessTokenMinimum
    {
        public PersonalAccessTokenMinimum(string name, string description)
        {
            Name = name;
            Description = description;
        }
        public string Name { get; }
        public string Description { get; }
    }
}
