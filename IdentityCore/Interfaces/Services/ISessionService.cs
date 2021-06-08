namespace IdentityCore.Interfaces.Services
{
    public interface ISessionService
    {
        string GetCurrentTenantId();
        string GetCorrelationId();
    }
}
