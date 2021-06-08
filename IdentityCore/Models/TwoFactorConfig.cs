using System;

namespace IdentityCore.Models
{
    public class TwoFactorConfig
    {
        public TwoFactorConfig(int passwordTokenBytes, TimeSpan passwordTokenDuration)
        {
            PasswordTokenBytes = passwordTokenBytes;
            PasswordTokenDuration = passwordTokenDuration;
        }
        public int PasswordTokenBytes { get; }
        public TimeSpan PasswordTokenDuration { get; }
    }
}
