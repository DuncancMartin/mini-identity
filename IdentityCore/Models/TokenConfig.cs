using System;

namespace IdentityCore.Models
{
    public class TokenConfig
    {
        public TokenConfig(TimeSpan refreshTokenLifeTime, TimeSpan extendedAuthTokenLifeTime)
        {
            RefreshTokenLifetimeInSeconds = (int) Math.Floor(refreshTokenLifeTime.TotalSeconds);
            ExtendedAuthTokenLifeTime = extendedAuthTokenLifeTime;
        }

        public int RefreshTokenLifetimeInSeconds { get; }

        public TimeSpan ExtendedAuthTokenLifeTime { get; }
    }
}
