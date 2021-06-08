using Microsoft.AspNetCore.Identity;

namespace IdentityCore
{
    /// <summary>
    /// Extended error describer for Imagine specific errors.
    /// </summary>
    public class IdentityMSErrorDescriber : IdentityErrorDescriber
    {
        public IdentityError InvalidTenant() => new IdentityError
        {
            Code = nameof(InvalidTenant),
            Description = "Tenant does not exist or is invalid."
        };

        public IdentityError UnknownUserName(string username) => new IdentityError
        {
            Code = nameof(UnknownUserName),
            Description = $"No user found with user name '{username}'."
        };
        public IdentityError UnknownUserId(int id) => new IdentityError
        {
            Code = nameof(UnknownUserId),
            Description = $"No user found with user id '{id}'."
        };
        public IdentityError EmailSendingFailed() => new IdentityError
        {
            Code = nameof(EmailSendingFailed),
            Description = "There was an error sending the email. Please contact support."
        };

        public IdentityError ActiveTenant() => new IdentityError
        {
            Code = nameof(ActiveTenant),
            Description = "Tenant is active."
        };

        public IdentityError MultipleUsersExist() => new IdentityError
        {
            Code = nameof(MultipleUsersExist),
            Description = "Multiple users exist for tenant."
        };

        public IdentityError TwoFactorAlreadyEnabled() => new IdentityError
        {
            Code = nameof(TwoFactorAlreadyEnabled),
            Description = "Two factor authentication is already enabled for this user."
        };

        public IdentityError ResetAuthenticatorKeyFailed() => new IdentityError
        {
            Code = nameof(ResetAuthenticatorKeyFailed),
            Description = "ResetAuthenticatorKeyFailed."
        };

        public IdentityError NewAuthenticatorKeyRequired() => new IdentityError
        {
            Code = nameof(NewAuthenticatorKeyRequired),
            Description = "New authenticator key must be generated."
        };

        public IdentityError InvalidTwoFactorToken() => new IdentityError
        {
            Code = nameof(InvalidTwoFactorToken),
            Description = "Two factor token not valid."
        };

        public IdentityError ModuleNotFound(int id) => new IdentityError
        { 
            Code = nameof(ModuleNotFound), 
            Description = $"Module Id = {id} not found" 
        };
        public IdentityError RoleNotFound(int id) => new IdentityError
        {
            Code = nameof(RoleNotFound),
            Description = $"Role Id = {id} not found"
        };

    }
}
