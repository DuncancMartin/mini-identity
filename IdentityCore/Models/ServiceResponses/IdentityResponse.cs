using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace IdentityCore.Models.ServiceResponses
{
    public class IdentityResponse
    {
        public IdentityResponse() { }

        public IdentityResponse(IdentityError error)
        {
            Errors = new List<IdentityError> {error};
        }

        public IdentityResponse(IEnumerable<IdentityError> errors)
        {
            Errors = errors;
        }

        public IEnumerable<IdentityError> Errors { get; set; }

        public bool IsSuccess => Errors == default(IEnumerable<IdentityError>);
    }
}
