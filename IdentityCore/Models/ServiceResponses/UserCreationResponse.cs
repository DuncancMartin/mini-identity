using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace IdentityCore.Models.ServiceResponses
{
    public class UserCreationResponse
    {
        public IdentityMSUser User { get; set; }

        public IEnumerable<IdentityError> Errors { get; set; }

        public bool IsSuccess => Errors == default(IEnumerable<IdentityError>);
    }
}
